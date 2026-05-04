using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.Data.Sync
{
    /// <summary>
    /// Background service that periodically pulls data from the remote database
    /// into the local database, keeping the local copy up to date.
    /// </summary>
    public sealed class DbSyncService : IDisposable
    {
        private readonly IDbConnectionService _local;
        private readonly IDbConnectionService _remote;
        private readonly int _intervalSeconds;
        private Timer? _timer;
        private bool _syncInProgress;

        public event Action<string>? OnSyncCompleted;
        public event Action<string>? OnSyncFailed;

        public DbSyncService(
            IDbConnectionService local,
            IDbConnectionService remote,
            int intervalSeconds = 60)
        {
            _local = local ?? throw new ArgumentNullException(nameof(local));
            _remote = remote ?? throw new ArgumentNullException(nameof(remote));
            _intervalSeconds = intervalSeconds;
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
        // Table/column names verified against UserRepository, OnlinePlayerRepository,
        // BattlePlayerStatsRepository, BattlePlayerSettingsRepository, TeamRepository,
        // TeamMemberRepository, BattlerPokemonRepository, BattleRepository,
        // and ParticipantRepository.

        private int SyncUsers()
        {
            // Table: Users — UserRepository
            var rows = _remote.Query<UserData>("SELECT UserID, UserName, Password FROM Users");
            foreach (var r in rows)
                _local.Execute(
                    "INSERT OR REPLACE INTO Users (UserID, UserName, Password) VALUES (@uid, @name, @pw)",
                    new { uid = r.UserID, name = r.UserName, pw = r.Password });
            return rows.Count;
        }

        private int SyncBattlePlayers()
        {
            // Table: BattlePlayer — OnlinePlayerRepository
            var rows = _remote.Query<BattlePlayerData>(
                "SELECT BattlePlayerID, UserID, Name, CreatedAt FROM BattlePlayer");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO BattlePlayer (BattlePlayerID, UserID, Name, CreatedAt)
                      VALUES (@id, @uid, @name, @createdAt)",
                    new { id = r.BattlePlayerID, uid = r.UserID, name = r.Name, createdAt = r.CreatedAt });
            return rows.Count;
        }

        private int SyncBattlePlayerStats()
        {
            // Table: BattlePlayerStats — BattlePlayerStatsRepository
            // No surrogate PK; BattlePlayerID is the key (no BattlePlayerStatsID in the model).
            var rows = _remote.Query<BattlePlayerStatsData>("SELECT * FROM BattlePlayerStats");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO BattlePlayerStats
                        (BattlePlayerID,
                         CurrentElo1v1, PeakElo1v1, Wins1v1, CurrentStreak1v1, BestStreak1v1,
                         CurrentElo2v2, PeakElo2v2, Wins2v2, CurrentStreak2v2, BestStreak2v2,
                         FaveTeamID)
                      VALUES
                        (@bpid,
                         @elo1, @peak1, @wins1, @streak1, @best1,
                         @elo2, @peak2, @wins2, @streak2, @best2,
                         @fav)",
                    new
                    {
                        bpid = r.BattlePlayerID,
                        elo1 = r.CurrentElo1v1,
                        peak1 = r.PeakElo1v1,
                        wins1 = r.Wins1v1,
                        streak1 = r.CurrentStreak1v1,
                        best1 = r.BestStreak1v1,
                        elo2 = r.CurrentElo2v2,
                        peak2 = r.PeakElo2v2,
                        wins2 = r.Wins2v2,
                        streak2 = r.CurrentStreak2v2,
                        best2 = r.BestStreak2v2,
                        fav = r.FaveTeamID
                    });
            return rows.Count;
        }

        private int SyncBattlePlayerSettings()
        {
            // Table: BattlePlayerSettings — BattlePlayerSettingsRepository
            // No surrogate PK; BattlePlayerID is the key (no BattlePlayerSettingsID in the model).
            // UpdatedAt is managed locally by SaveSetting() via datetime('now'), not synced.
            var rows = _remote.Query<BattlePlayerSettingsData>("SELECT * FROM BattlePlayerSettings");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO BattlePlayerSettings
                        (BattlePlayerID, AnimationsEnabled, TextSpeedID, BackgroundID, ShowTypeEffectiveness)
                      VALUES (@bpid, @anim, @txt, @bg, @eff)",
                    new
                    {
                        bpid = r.BattlePlayerID,
                        anim = r.AnimationsEnabled,
                        txt = r.TextSpeedID,
                        bg = r.BackgroundID,
                        eff = r.ShowTypeEffectiveness
                    });
            return rows.Count;
        }

        private int SyncTeams()
        {
            // Table: teams — TeamRepository (columns: id, team_name, battle_player_id)
            var rows = _remote.Query<TeamData>(
                "SELECT id AS Id, team_name AS TeamName, battle_player_id AS BattlePlayerId FROM teams");
            foreach (var r in rows)
                _local.Execute(
                    "INSERT OR REPLACE INTO teams (id, team_name, battle_player_id) VALUES (@id, @name, @bpid)",
                    new { id = r.Id, name = r.TeamName, bpid = r.BattlePlayerId });
            return rows.Count;
        }

        private int SyncTeamMembers()
        {
            // Table: team_members — TeamMemberRepository (columns: team_id, pokemonID, slot_number)
            // TeamMemberData.Team_id → team_id, TeamMemberData.Slot_number → slot_number
            var rows = _remote.Query<TeamMemberData>(
                "SELECT team_id AS Team_id, pokemonID, slot_number AS Slot_number FROM team_members");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO team_members (team_id, pokemonID, slot_number)
                      VALUES (@tid, @pid, @slot)",
                    new { tid = r.Team_id, pid = r.PokemonID, slot = r.Slot_number });
            return rows.Count;
        }

        private int SyncBattlerPokemon()
        {
            // Table: battler_pokemon — BattlerPokemonRepository
            // Only sync pokemon that belong to a team (same filter as before).
            // BattlerPokemon.Nature is a string; column is 'nature'.
            // BattlerPokemon.Shiny maps to column 'shiny' (0/1 int).
            // BattlerPokemon.Name maps to column 'nickname' (per original intent).
            var rows = _remote.Query<BattlerPokemon>(
                @"SELECT bp.* FROM battler_pokemon bp
                  WHERE EXISTS (SELECT 1 FROM team_members tm WHERE tm.pokemonID = bp.pokemonID)");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO battler_pokemon
                        (pokemonID, pokedexID, abilityID, itemID, shiny, gender, level,
                         move1ID, move2ID, move3ID, move4ID,
                         iv_hp, iv_atk, iv_def, iv_spAtk, iv_spDef, iv_speed,
                         ev_hp, ev_atk, ev_def, ev_spAtk, ev_spDef, ev_speed,
                         nature)
                      VALUES
                        (@pokemonID, @pokedexID, @abilityID, @itemID, @shiny, @gender, @level,
                         @move1, @move2, @move3, @move4,
                         @ivHp, @ivAtk, @ivDef, @ivSpAtk, @ivSpDef, @ivSpeed,
                         @evHp, @evAtk, @evDef, @evSpAtk, @evSpDef, @evSpeed,
                         @nature)",
                    new
                    {
                        pokemonID = r.PokemonID,
                        pokedexID = r.PokedexID,
                        abilityID = r.AbilityID,
                        itemID = r.ItemID,
                        shiny = r.Shiny,
                        gender = r.Gender,
                        level = r.Level,
                        move1 = r.Move1ID,
                        move2 = r.Move2ID,
                        move3 = r.Move3ID,
                        move4 = r.Move4ID,
                        ivHp = r.Iv_hp,
                        ivAtk = r.Iv_atk,
                        ivDef = r.Iv_def,
                        ivSpAtk = r.Iv_spAtk,
                        ivSpDef = r.Iv_spDef,
                        ivSpeed = r.Iv_speed,
                        evHp = r.Ev_hp,
                        evAtk = r.Ev_atk,
                        evDef = r.Ev_def,
                        evSpAtk = r.Ev_spAtk,
                        evSpDef = r.Ev_spDef,
                        evSpeed = r.Ev_speed,
                        nature = r.Nature
                    });
            return rows.Count;
        }

        private int SyncBattles()
        {
            // Table: Battle — BattleRepository (columns: BattleID, BattleDate)
            // BattleRecordData has BattleID and BattleDate (not CreatedAt, Format, or Status).
            var rows = _remote.Query<BattleRecordData>("SELECT BattleID, BattleDate FROM Battle");
            foreach (var r in rows)
                _local.Execute(
                    "INSERT OR REPLACE INTO Battle (BattleID, BattleDate) VALUES (@id, @date)",
                    new { id = r.BattleID, date = r.BattleDate });
            return rows.Count;
        }

        private int SyncParticipants()
        {
            // Table: BattleParticipants — ParticipantRepository
            // BattleParticipantData: BattleID, BattlePlayerID, TeamID, IsWinner (not Result).
            var rows = _remote.Query<BattleParticipantData>(
                "SELECT BattleID, BattlePlayerID, TeamID, IsWinner FROM BattleParticipants");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO BattleParticipants
                        (BattleID, BattlePlayerID, TeamID, IsWinner)
                      VALUES (@bid, @bpid, @tid, @winner)",
                    new
                    {
                        bid = r.BattleID,
                        bpid = r.BattlePlayerID,
                        tid = r.TeamID,
                        winner = r.IsWinner
                    });
            return rows.Count;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}