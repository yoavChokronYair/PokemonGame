// PokemonGame.Server/BattleRoom/BattleRoom.cs
// CHANGE: SendForcedSwitchAsync now calls _battleManager.PlayerTeam/BotTeam.GetSwitchableIndices().
// Everything else is identical to your existing file.

using System.Text.Json;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Server.Network;
using PokemonGame.Services.Network;
using PokemonGame.Services.Network.Packets;

namespace PokemonGame.Server.BattleRoom
{
    public class BattleRoom
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public string RoomId { get; } = Guid.NewGuid().ToString("N")[..8];

        // ── Players ───────────────────────────────────────────────────────────
        private readonly ConnectedPlayer _playerA;
        private readonly ConnectedPlayer _playerB;

        // ── Battle engine ─────────────────────────────────────────────────────
        private readonly BattleManager _battleManager;

        // ── Turn synchronisation ──────────────────────────────────────────────
        private int _actionA = -1;
        private int _actionB = -1;
        private bool _isSwitch_A = false;
        private bool _isSwitch_B = false;
        private readonly SemaphoreSlim _turnReady = new(0, 1);

        // ── Lifecycle ─────────────────────────────────────────────────────────
        private bool _battleOver;
        private int _logCountBeforeTurn;

        public BattleRoom(ConnectedPlayer playerA, ConnectedPlayer playerB,
                          PokemonTeam teamA, PokemonTeam teamB)
        {
            _playerA = playerA;
            _playerB = playerB;
            _battleManager = new BattleManager(teamA, teamB, BotLevel.Hard);
        }

        // ── Entry point ───────────────────────────────────────────────────────

        public async Task RunAsync()
        {
            Console.WriteLine($"[ROOM {RoomId}] Started — {_playerA.PlayerName} vs {_playerB.PlayerName}");
            await SendMatchFoundAsync().ConfigureAwait(false);
            Console.WriteLine($"[ROOM {RoomId}] MatchFound sent to both players");

            using var ctsA = new CancellationTokenSource();
            using var ctsB = new CancellationTokenSource();

            var listenA = Task.Run(() => ListenAsync(_playerA, isPlayerA: true, ctsA.Token));
            var listenB = Task.Run(() => ListenAsync(_playerB, isPlayerA: false, ctsB.Token));

            while (!_battleOver)
            {
                Console.WriteLine($"[ROOM {RoomId}] Waiting for both players to submit actions...");
                await _turnReady.WaitAsync().ConfigureAwait(false);
                if (_battleOver) break;

                Console.WriteLine($"[ROOM {RoomId}] Both actions received — resolving turn (A={_actionA} switch={_isSwitch_A}, B={_actionB} switch={_isSwitch_B})");
                await ResolveTurnAsync().ConfigureAwait(false);

                _actionA = _actionB = -1;
                _isSwitch_A = _isSwitch_B = false;
            }

            Console.WriteLine($"[ROOM {RoomId}] Battle over — cancelling listeners");
            ctsA.Cancel();
            ctsB.Cancel();
            await Task.WhenAll(listenA, listenB).ConfigureAwait(false);
            Console.WriteLine($"[ROOM {RoomId}] Listeners stopped. Room closed.");
        }

        private async Task ListenAsync(ConnectedPlayer player, bool isPlayerA, CancellationToken ct)
        {
            Console.WriteLine($"[ROOM {RoomId}] Listener started for {player.PlayerName} (isPlayerA={isPlayerA})");
            try
            {
                while (!ct.IsCancellationRequested && !_battleOver)
                {
                    string? raw = await PacketHelper.ReadRawPacketAsync(player.Stream).ConfigureAwait(false);
                    if (raw == null)
                    {
                        Console.WriteLine($"[ROOM {RoomId}] {player.PlayerName} disconnected (null packet)");
                        await HandleForfeitAsync(player).ConfigureAwait(false);
                        return;
                    }

                    Console.WriteLine($"[ROOM {RoomId}] Packet from {player.PlayerName}: {raw}");

                    using var doc = JsonDocument.Parse(raw);
                    string type = doc.RootElement.GetProperty("type").GetString() ?? "";

                    switch (type)
                    {
                        case "MoveAction":
                            var ma = JsonSerializer.Deserialize<MoveActionPacket>(raw)!;
                            Console.WriteLine($"[ROOM {RoomId}] {player.PlayerName} chose move index {ma.MoveIndex}");
                            SetAction(isPlayerA, ma.MoveIndex, isSwitch: false);
                            break;

                        case "SwitchAction":
                            var sa = JsonSerializer.Deserialize<SwitchActionPacket>(raw)!;
                            Console.WriteLine($"[ROOM {RoomId}] {player.PlayerName} switched to slot {sa.SlotIndex}");
                            SetAction(isPlayerA, sa.SlotIndex, isSwitch: true);
                            break;

                        case "Forfeit":
                            Console.WriteLine($"[ROOM {RoomId}] {player.PlayerName} forfeited");
                            await HandleForfeitAsync(player).ConfigureAwait(false);
                            return;

                        default:
                            Console.WriteLine($"[ROOM {RoomId}] Unknown packet type '{type}' from {player.PlayerName}");
                            break;
                    }
                }
            }
            catch (Exception) when (ct.IsCancellationRequested || _battleOver)
            {
                Console.WriteLine($"[ROOM {RoomId}] Listener for {player.PlayerName} cancelled cleanly");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ROOM {RoomId}] Listener ERROR for {player.PlayerName}: {ex}");
                await HandleForfeitAsync(player).ConfigureAwait(false);
            }
        }

        private async Task ResolveTurnAsync()
        {
            Console.WriteLine($"[ROOM {RoomId}] Resolving turn — PlayerHP={_battleManager.PlayerActive.CurrentHP} BotHP={_battleManager.BotActive.CurrentHP}");

            _logCountBeforeTurn = _battleManager.logger.Entries.Count;
            BattleAction playerAction = _isSwitch_A ? BattleAction.Switch : BattleAction.Move;
            _battleManager.RunTurn(_actionA, playerAction);

            var newLogLines = _battleManager.logger.Entries
                .Skip(_logCountBeforeTurn)
                .Select(e => e.Message)
                .ToList();

            Console.WriteLine($"[ROOM {RoomId}] Turn log ({newLogLines.Count} lines):");
            foreach (var line in newLogLines)
                Console.WriteLine($"[ROOM {RoomId}]   > {line}");

            bool playerFainted = _battleManager.PlayerActive.IsFainted;
            bool enemyFainted = _battleManager.BotActive.IsFainted;
            Console.WriteLine($"[ROOM {RoomId}] After turn — PlayerHP={_battleManager.PlayerActive.CurrentHP} playerFainted={playerFainted} | BotHP={_battleManager.BotActive.CurrentHP} enemyFainted={enemyFainted}");

            // ... rest of method unchanged, add at the end:
            if (_battleManager.Winner != null)
                Console.WriteLine($"[ROOM {RoomId}] Winner detected: {ResolveWinnerName()}");

            if (playerFainted)
            {
                Console.WriteLine($"[ROOM {RoomId}] Sending ForcedSwitch to {_playerA.PlayerName}");
                await SendForcedSwitchAsync(_playerA, isPlayerSide: true).ConfigureAwait(false);
            }
            if (enemyFainted)
            {
                Console.WriteLine($"[ROOM {RoomId}] Sending ForcedSwitch to {_playerB.PlayerName}");
                await SendForcedSwitchAsync(_playerB, isPlayerSide: false).ConfigureAwait(false);
            }
        }

       

        // ── Helpers ───────────────────────────────────────────────────────────

        private string ResolveWinnerName()
            => (_battleManager.Loser == null || _battleManager.PlayerActive.IsFainted)
                ? _playerB.PlayerName
                : _playerA.PlayerName;

       

        private async Task SendMatchFoundAsync()
        {
            // Player A gets their own move names and both pokemon info
            var toA = new MatchFoundPacket
            {
                RoomId = RoomId,
                RivalName = _playerB.PlayerName,
                RivalTeamId = _playerB.TeamId,
                RivalTeam = _playerB.Team,
                PlayerMoveNames = _battleManager.PlayerActive.Moves
                                    .Select(m => ((MoveState) m).Name ?? "—").ToList(),
                PlayerMaxHp = _battleManager.PlayerActive.MaxHP,
                EnemyMaxHp = _battleManager.BotActive.MaxHP,
                PlayerPokedexId = _battleManager.PlayerActive.PokedexId,
                EnemyPokedexId = _battleManager.BotActive.PokedexId,
                PlayerLevel = _battleManager.PlayerActive.Level,
                EnemyLevel = _battleManager.BotActive.Level,
            };
            // Player B — their "player" side is BotActive from A's perspective
            var toB = new MatchFoundPacket
            {
                RoomId = RoomId,
                RivalName = _playerA.PlayerName,
                RivalTeamId = _playerA.TeamId,
                RivalTeam = _playerA.Team,
                PlayerMoveNames = _battleManager.BotActive.Moves
                                    .Select(m => ((MoveState)m).Name ?? "—").ToList(),
                PlayerMaxHp = _battleManager.BotActive.MaxHP,
                EnemyMaxHp = _battleManager.PlayerActive.MaxHP,
                PlayerPokedexId = _battleManager.BotActive.PokedexId,
                EnemyPokedexId = _battleManager.PlayerActive.PokedexId,
                PlayerLevel = _battleManager.BotActive.Level,
                EnemyLevel = _battleManager.PlayerActive.Level,
            };
            await Task.WhenAll(
                PacketHelper.WritePacketAsync(_playerA.Stream, toA),
                PacketHelper.WritePacketAsync(_playerB.Stream, toB)
            ).ConfigureAwait(false);
        }
        private void SetAction(bool isPlayerA, int value, bool isSwitch)
        {
            if (isPlayerA) { _actionA = value; _isSwitch_A = isSwitch; }
            else { _actionB = value; _isSwitch_B = isSwitch; }

            Console.WriteLine($"[ROOM {RoomId}] Action set — A={_actionA} B={_actionB} (need both >=0 to proceed)");

            if (_actionA >= 0 && _actionB >= 0)
            {
                Console.WriteLine($"[ROOM {RoomId}] Both actions ready — releasing turn semaphore");
                _turnReady.Release();
            }
        }

        private async Task SendForcedSwitchAsync(ConnectedPlayer player, bool isPlayerSide)
        {
            // Now works because BattleManager.PlayerTeam / BotTeam are public.
            PokemonTeam team = isPlayerSide
                ? _battleManager.PlayerTeam
                : _battleManager.BotTeam;

            var packet = new ForcedSwitchPacket
            {
                RoomId = RoomId,
                AvailableSlots = team.GetSwitchableIndices().ToList()
            };

            await PacketHelper.WritePacketAsync(player.Stream, packet).ConfigureAwait(false);
        }

        private async Task SendBattleEndAsync(string winnerName)
        {
            _battleOver = true;
            string loserName = winnerName == _playerA.PlayerName
                ? _playerB.PlayerName
                : _playerA.PlayerName;

            var packetToA = new BattleEndPacket
            {
                RoomId = RoomId,
                WinnerName = winnerName,
                LoserName = loserName,
                OpponentBattlePlayerId = _playerB.PlayerId
            };
            var packetToB = new BattleEndPacket
            {
                RoomId = RoomId,
                WinnerName = winnerName,
                LoserName = loserName,
                OpponentBattlePlayerId = _playerA.PlayerId
            };

            await Task.WhenAll(
                PacketHelper.WritePacketAsync(_playerA.Stream, packetToA),
                PacketHelper.WritePacketAsync(_playerB.Stream, packetToB)
            ).ConfigureAwait(false);
        }

        private async Task HandleForfeitAsync(ConnectedPlayer forfeitingPlayer)
        {
            if (_battleOver) return;
            string winner = forfeitingPlayer == _playerA
                ? _playerB.PlayerName
                : _playerA.PlayerName;
            await SendBattleEndAsync(winner).ConfigureAwait(false);
        }
    }
}