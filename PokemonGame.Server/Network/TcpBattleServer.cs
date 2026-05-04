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
            var player = new ConnectedPlayer(client);
            try
            {
                string? raw = await PacketHelper.ReadRawPacketAsync(player.Stream).ConfigureAwait(false);
                if (raw == null) { client.Close(); return; }

                using var doc = JsonDocument.Parse(raw);
                string type = doc.RootElement.GetProperty("type").GetString() ?? "";

                if (type != "FindMatch")
                {
                    Console.WriteLine($"[TcpBattleServer] Unexpected first packet: {type}. Dropping.");
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

                Console.WriteLine($"[TcpBattleServer] {player.PlayerName} queued ({player.BattleMode}, 1v1={player.IsOneVOne})");

                ConnectedPlayer? opponent = _queue.Enqueue(player);
                if (opponent == null) return; // waiting for second player

                await StartBattleRoomAsync(opponent, player, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TcpBattleServer] Connection error: {ex.Message}");
                _queue.Remove(player);
                client.Close();
            }
        }

        // ── Room creation ─────────────────────────────────────────────────────

        private static async Task StartBattleRoomAsync(
            ConnectedPlayer playerA, ConnectedPlayer playerB, CancellationToken ct)
        {
            Console.WriteLine($"[TcpBattleServer] Match: {playerA.PlayerName} vs {playerB.PlayerName}");

            // Load full teams from the server DB (falls back to DTO stubs on error)
            PokemonTeam teamA = TeamBuilder.BuildFromPlayer(playerA.PlayerId, playerA.Team);
            PokemonTeam teamB = TeamBuilder.BuildFromPlayer(playerB.PlayerId, playerB.Team);

            var room = new BattleRoom.BattleRoom(playerA, playerB, teamA, teamB);
            playerA.RoomId = playerB.RoomId = room.RoomId;

            try
            {
                await room.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TcpBattleServer] Room {room.RoomId} crashed: {ex.Message}");
            }
            finally
            {
                playerA.TcpClient.Close();
                playerB.TcpClient.Close();
                Console.WriteLine($"[TcpBattleServer] Room {room.RoomId} closed.");
            }
        }
    }
}