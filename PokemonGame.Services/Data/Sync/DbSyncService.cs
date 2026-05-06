using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.Repositories;

namespace PokemonGame.Services.Data.Sync
{
    internal sealed class DbSyncService : IDisposable
    {
        private readonly IDbConnectionService _remote;
        private readonly int _intervalSeconds;
        private Timer? _timer;
        private bool _syncInProgress;

        private readonly UserRepository _userRepo;
        private readonly OnlinePlayerRepository _playerRepo;
        private readonly BattlePlayerStatsRepository _statsRepo;
        private readonly BattlePlayerSettingsRepository _settingsRepo;
        private readonly TeamRepository _teamRepo;
        private readonly TeamMemberRepository _teamMemberRepo;
        private readonly BattlerPokemonRepository _battlerPokemonRepo;
        private readonly BattleRepository _battleRepo;
        private readonly ParticipantRepository _participantRepo;

        public event Action<string>? OnSyncCompleted;
        public event Action<string>? OnSyncFailed;

        public DbSyncService(
            IDbConnectionService local,
            IDbConnectionService remote,
            int intervalSeconds = 60)
        {
            _remote = remote ?? throw new ArgumentNullException(nameof(remote));
            _intervalSeconds = intervalSeconds;

            _userRepo = new UserRepository(local);
            _playerRepo = new OnlinePlayerRepository(local);
            _statsRepo = new BattlePlayerStatsRepository(local);
            _settingsRepo = new BattlePlayerSettingsRepository(local);
            _teamRepo = new TeamRepository(local);
            _teamMemberRepo = new TeamMemberRepository(local);
            _battlerPokemonRepo = new BattlerPokemonRepository(local);
            _battleRepo = new BattleRepository(local);
            _participantRepo = new ParticipantRepository(local);
        }

        public void Start()
        {
            if (_timer != null) return;
            var period = TimeSpan.FromSeconds(_intervalSeconds);
            _timer = new Timer(_ => RunSyncCycle(), null, TimeSpan.Zero, period);
            Console.WriteLine($"[DbSyncService] Started — syncing every {_intervalSeconds}s.");
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("[DbSyncService] Stopped.");
        }

        public Task SyncNowAsync() => Task.Run(RunSyncCycle);

        public Task SyncPlayerNowAsync(int battlePlayerId) =>
            Task.Run(() =>
            {
                SyncBattlePlayers(battlePlayerId);
                SyncBattlePlayerStats(battlePlayerId);
                SyncBattlePlayerSettings(battlePlayerId);
                SyncTeams(battlePlayerId);
                SyncTeamMembers(battlePlayerId);
                SyncBattlerPokemon(battlePlayerId);
            });
        public Task SyncUserNowAsync(int userId) =>
            Task.Run(() => SyncUsers(userId));

        

        // ── Core sync logic ───────────────────────────────────────────────────

        private void RunSyncCycle()
        {
            if (_syncInProgress)
            {
                Console.WriteLine("[DbSyncService] Previous cycle still running — skipping.");
                return;
            }

            _syncInProgress = true;
            try
            {
                int rows = 0;
                rows += SyncUsers();
                rows += SyncBattlePlayers();
                rows += SyncBattlePlayerStats();
                rows += SyncBattlePlayerSettings();
                rows += SyncTeams();
                rows += SyncTeamMembers();
                rows += SyncBattlerPokemon();
                rows += SyncBattles();
                rows += SyncParticipants();

                string msg = $"Sync complete — {rows} rows upserted at {DateTime.Now:HH:mm:ss}.";
                Console.WriteLine($"[DbSyncService] {msg}");
                OnSyncCompleted?.Invoke(msg);
            }
            catch (Exception ex)
            {
                string msg = $"Sync failed: {ex.Message}";
                Console.WriteLine($"[DbSyncService] {msg}");
                OnSyncFailed?.Invoke(msg);
            }
            finally
            {
                _syncInProgress = false;
            }
        }

        // ── Per-table sync helpers ────────────────────────────────────────────

        private int SyncUsers()
        {
            var rows = _remote.Query<UserData>("SELECT UserID, UserName, Password FROM Users");
            foreach (var r in rows)
                _userRepo.Upsert(r);
            return rows.Count;
        }
        private int SyncUsers(int? userId = null)
        {
            var sql = "SELECT UserID, UserName, Password FROM Users";
            if (userId.HasValue) sql += " WHERE UserID = @id";

            var rows = userId.HasValue
                ? _remote.Query<UserData>(sql, new { id = userId.Value })
                : _remote.Query<UserData>(sql);

            foreach (var r in rows)
                _userRepo.Upsert(r);
            return rows.Count;
        }
        private int SyncBattlePlayers(int? battlePlayerId = null)
        {
            var sql = "SELECT BattlePlayerID, UserID, Name, CreatedAt FROM BattlePlayer";
            if (battlePlayerId.HasValue) sql += " WHERE BattlePlayerID = @id";

            var rows = battlePlayerId.HasValue
                ? _remote.Query<BattlePlayerData>(sql, new { id = battlePlayerId.Value })
                : _remote.Query<BattlePlayerData>(sql);

            foreach (var r in rows)
                _playerRepo.Upsert(r);
            return rows.Count;
        }

        private int SyncBattlePlayerStats(int? battlePlayerId = null)
        {
            var sql = "SELECT * FROM BattlePlayerStats";
            if (battlePlayerId.HasValue) sql += " WHERE BattlePlayerID = @id";

            var rows = battlePlayerId.HasValue
                ? _remote.Query<BattlePlayerStatsData>(sql, new { id = battlePlayerId.Value })
                : _remote.Query<BattlePlayerStatsData>(sql);

            foreach (var r in rows)
                _statsRepo.Upsert(r);
            return rows.Count;
        }

        private int SyncBattlePlayerSettings(int? battlePlayerId = null)
        {
            var sql = "SELECT * FROM BattlePlayerSettings";
            if (battlePlayerId.HasValue) sql += " WHERE BattlePlayerID = @id";

            var rows = battlePlayerId.HasValue
                ? _remote.Query<BattlePlayerSettingsData>(sql, new { id = battlePlayerId.Value })
                : _remote.Query<BattlePlayerSettingsData>(sql);

            foreach (var r in rows)
                _settingsRepo.Upsert(r);
            return rows.Count;
        }

        private int SyncTeams(int? battlePlayerId = null)
        {
            var sql = "SELECT id AS Id, team_name AS TeamName, battle_player_id AS BattlePlayerId FROM teams";
            if (battlePlayerId.HasValue) sql += " WHERE battle_player_id = @id";

            var rows = battlePlayerId.HasValue
                ? _remote.Query<TeamData>(sql, new { id = battlePlayerId.Value })
                : _remote.Query<TeamData>(sql);

            foreach (var r in rows)
                _teamRepo.Upsert(r);
            return rows.Count;
        }

        private int SyncTeamMembers(int? battlePlayerId = null)
        {
            var sql = battlePlayerId.HasValue
                ? @"SELECT tm.team_id AS Team_id, tm.pokemonID, tm.slot_number AS Slot_number
                    FROM team_members tm
                    JOIN teams t ON t.id = tm.team_id
                    WHERE t.battle_player_id = @id"
                : "SELECT team_id AS Team_id, pokemonID, slot_number AS Slot_number FROM team_members";

            var rows = battlePlayerId.HasValue
                ? _remote.Query<TeamMemberData>(sql, new { id = battlePlayerId.Value })
                : _remote.Query<TeamMemberData>(sql);

            foreach (var r in rows)
                _teamMemberRepo.Upsert(r);
            return rows.Count;
        }

        private int SyncBattlerPokemon(int? battlePlayerId = null)
        {
            var sql = battlePlayerId.HasValue
                ? @"SELECT bp.* FROM battler_pokemon bp
                    WHERE EXISTS (
                        SELECT 1 FROM team_members tm
                        JOIN teams t ON t.id = tm.team_id
                        WHERE tm.pokemonID = bp.pokemonID
                        AND t.battle_player_id = @id)"
                : @"SELECT bp.* FROM battler_pokemon bp
                    WHERE EXISTS (SELECT 1 FROM team_members tm WHERE tm.pokemonID = bp.pokemonID)";

            var rows = battlePlayerId.HasValue
                ? _remote.Query<BattlerPokemon>(sql, new { id = battlePlayerId.Value })
                : _remote.Query<BattlerPokemon>(sql);

            foreach (var r in rows)
                _battlerPokemonRepo.Upsert(r);
            return rows.Count;
        }

        private int SyncBattles()
        {
            var rows = _remote.Query<BattleRecordData>("SELECT BattleID, BattleDate FROM Battle");
            foreach (var r in rows)
                _battleRepo.Upsert(r);
            return rows.Count;
        }

        private int SyncParticipants()
        {
            var rows = _remote.Query<BattleParticipantData>(
                "SELECT BattleID, BattlePlayerID, TeamID, IsWinner FROM BattleParticipants");
            foreach (var r in rows)
                _participantRepo.Upsert(r);
            return rows.Count;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}