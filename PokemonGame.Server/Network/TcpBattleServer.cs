// PokemonGame.Server/Network/TcpBattleServer.cs
// CHANGE: StartBattleRoomAsync now calls TeamBuilder.BuildFromPlayer(playerId, dtos)
// so teams are loaded from the server DB with full stats.

using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Server.BattleRoom;
using PokemonGame.Services.Network;
using PokemonGame.Services.Network.Packets;

namespace PokemonGame.Server.Network
{
    public class TcpBattleServer
    {
        private readonly int _port;
        private readonly MatchmakingQueue _queue = new();
        private TcpListener? _listener;

        public TcpBattleServer(int port) => _port = port;

        // ── Start / Stop ──────────────────────────────────────────────────────

        public async Task StartAsync(CancellationToken ct = default)
        {
            Console.WriteLine($"[TCP] Server starting on port {_port}");
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"[TcpBattleServer] Listening on port {_port}…");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    Console.WriteLine($"[TcpBattleServer] Accepted from {client.Client.RemoteEndPoint}");
                    _ = Task.Run(() => HandleConnectionAsync(client, ct), ct);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TcpBattleServer] Accept error: {ex.Message}");
                }
            }

            _listener.Stop();
            Console.WriteLine("[TcpBattleServer] Stopped.");
        }

        // ── Per-connection handshake ──────────────────────────────────────────

        private async Task HandleConnectionAsync(TcpClient client, CancellationToken ct)
        {
            Console.WriteLine($"[TCP] New connection from {client.Client.RemoteEndPoint}");
            var player = new ConnectedPlayer(client);
            try
            {
                string? raw = await PacketHelper.ReadRawPacketAsync(player.Stream).ConfigureAwait(false);
                if (raw == null)
                {
                    Console.WriteLine($"[TCP] Connection closed before sending FindMatch");
                    client.Close();
                    return;
                }

                Console.WriteLine($"[TCP] First packet: {raw}");

                using var doc = JsonDocument.Parse(raw);
                string type = doc.RootElement.GetProperty("type").GetString() ?? "";

                if (type != "FindMatch")
                {
                    Console.WriteLine($"[TCP] Expected FindMatch but got '{type}' — dropping");
                    client.Close();
                    return;
                }

                var pkt = JsonSerializer.Deserialize<FindMatchPacket>(raw)!;
                player.PlayerId = pkt.PlayerId;
                player.PlayerName = pkt.PlayerName;
                player.BattleMode = pkt.BattleMode;
                player.IsOneVOne = pkt.IsOneVOne;
                player.TeamId = pkt.TeamId;
                player.Team = pkt.Team;

                Console.WriteLine($"[TCP] Player registered — Name={player.PlayerName} Id={player.PlayerId} Mode={player.BattleMode} 1v1={player.IsOneVOne} TeamId={player.TeamId} TeamSize={player.Team?.Count}");

                ConnectedPlayer? opponent = _queue.Enqueue(player);
                if (opponent == null)
                {
                    Console.WriteLine($"[TCP] {player.PlayerName} is queued — waiting for opponent");
                    return;
                }

                Console.WriteLine($"[TCP] Match found: {opponent.PlayerName} vs {player.PlayerName}");
                await StartBattleRoomAsync(opponent, player, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP] HandleConnection ERROR for {player.PlayerName}: {ex}");
                _queue.Remove(player);
                client.Close();
            }
        }

        private static async Task StartBattleRoomAsync(
            ConnectedPlayer playerA, ConnectedPlayer playerB, CancellationToken ct)
        {
            Console.WriteLine($"[TCP] Building teams — A={playerA.PlayerName} (Id={playerA.PlayerId}) B={playerB.PlayerName} (Id={playerB.PlayerId})");

            PokemonTeam teamA = TeamBuilder.BuildFromPlayer(playerA.PlayerId, playerA.Team);
            PokemonTeam teamB = TeamBuilder.BuildFromPlayer(playerB.PlayerId, playerB.Team);

            Console.WriteLine($"[TCP] TeamA size={teamA?.getAllPokemonCount() ?? -1} TeamB size={teamB?.getAllPokemonCount() ?? -1}");

            var room = new BattleRoom.BattleRoom(playerA, playerB, teamA, teamB);
            playerA.RoomId = playerB.RoomId = room.RoomId;
            Console.WriteLine($"[TCP] Room created: {room.RoomId}");

            try
            {
                await room.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP] Room {room.RoomId} CRASHED: {ex}");
            }
            finally
            {
                Console.WriteLine($"[TCP] Room {room.RoomId} finished — closing connections");
                playerA.TcpClient.Close();
                playerB.TcpClient.Close();
            }
        }
    }
}