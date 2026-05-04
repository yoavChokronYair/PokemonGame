using PokemonGame.Services.Data.ConnectionsService;

namespace PokemonGame.Services.Data.Sync
{
    /// <summary>
    /// Background service that periodically pulls data from the remote database
    /// into the local database, keeping the local copy up to date.
    /// </summary>
    /// <remarks>
    /// <b>Design contract</b>
    /// <list type="bullet">
    ///   <item>Runs on a <see cref="Timer"/> — never blocks the game thread.</item>
    ///   <item>Only syncs tables that can be updated by the server or other clients
    ///         (player stats, battle records, settings).  Read-only game-data tables
    ///         (Pokédex, moves, abilities …) are never synced at runtime.</item>
    ///   <item>Uses UPSERT (<c>INSERT OR REPLACE</c>) so rows created locally while
    ///         offline are preserved, and rows updated on the server win on the
    ///         next sync cycle.</item>
    ///   <item>All exceptions are caught and logged; a network hiccup must never
    ///         crash or stall the game.</item>
    /// </list>
    /// <b>Usage</b>
    /// <code>
    /// var sync = new DbSyncService(localDb, remoteDb, intervalSeconds: 60);
    /// sync.Start();
    /// // … game runs …
    /// sync.Stop();
    /// </code>
    /// </remarks>
    public sealed class DbSyncService : IDisposable
    {
        private readonly IDbConnectionService _local;
        private readonly IDbConnectionService _remote;
        private readonly int _intervalSeconds;
        private Timer? _timer;
        private bool _syncInProgress;

        /// <summary>Raised after every successful sync cycle with a short status message.</summary>
        public event Action<string>? OnSyncCompleted;

        /// <summary>Raised when a sync cycle fails, with the exception message.</summary>
        public event Action<string>? OnSyncFailed;

        /// <param name="local">Local database to write INTO.</param>
        /// <param name="remote">Remote database to read FROM.</param>
        /// <param name="intervalSeconds">How often to sync (default 60 s).</param>
        public DbSyncService(
            IDbConnectionService local,
            IDbConnectionService remote,
            int intervalSeconds = 60)
        {
            _local = local ?? throw new ArgumentNullException(nameof(local));
            _remote = remote ?? throw new ArgumentNullException(nameof(remote));
            _intervalSeconds = intervalSeconds;
        }

        /// <summary>Starts the periodic sync timer. Safe to call multiple times.</summary>
        public void Start()
        {
            if (_timer != null) return;

            var period = TimeSpan.FromSeconds(_intervalSeconds);
            // Run once immediately, then on the interval.
            _timer = new Timer(_ => RunSyncCycle(), null, TimeSpan.Zero, period);
            Console.WriteLine($"[DbSyncService] Started — syncing every {_intervalSeconds}s.");
        }

        /// <summary>Stops the timer without disposing; can be restarted with <see cref="Start"/>.</summary>
        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("[DbSyncService] Stopped.");
        }

        /// <summary>
        /// Triggers a single sync cycle immediately, regardless of the timer schedule.
        /// Useful to call right after login so the player sees fresh data without waiting.
        /// </summary>
        public Task SyncNowAsync() => Task.Run(RunSyncCycle);

        // ── Core sync logic ───────────────────────────────────────────────────

        private void RunSyncCycle()
        {
            // Guard against overlapping cycles (e.g. slow network + short interval).
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

        // ── Per-table helpers ─────────────────────────────────────────────────
        // Each helper:
        //   1. Fetches all rows from the remote.
        //   2. UPSERTs each row into the local DB using INSERT OR REPLACE.
        // Adjust column lists here if your schema changes.

        private int SyncUsers()
        {
            var rows = _remote.Query<UserSyncRow>("SELECT UserID, UserName, Password FROM Users");
            foreach (var r in rows)
                _local.Execute(
                    "INSERT OR REPLACE INTO Users (UserID, UserName, Password) VALUES (@uid, @name, @pw)",
                    new { uid = r.UserID, name = r.UserName, pw = r.Password });
            return rows.Count;
        }

        private int SyncBattlePlayers()
        {
            var rows = _remote.Query<BattlePlayerSyncRow>(
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
            var rows = _remote.Query<BattlePlayerStatsSyncRow>("SELECT * FROM BattlePlayerStats");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO BattlePlayerStats
                        (BattlePlayerStatsID, BattlePlayerID,
                         CurrentElo1v1, PeakElo1v1, Wins1v1, CurrentStreak1v1, BestStreak1v1,
                         CurrentElo2v2, PeakElo2v2, Wins2v2, CurrentStreak2v2, BestStreak2v2,
                         FaveTeamID)
                      VALUES
                        (@sid, @bpid,
                         @elo1, @peak1, @wins1, @streak1, @best1,
                         @elo2, @peak2, @wins2, @streak2, @best2,
                         @fav)",
                    new
                    {
                        sid = r.BattlePlayerStatsID,
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
            var rows = _remote.Query<BattlePlayerSettingsSyncRow>("SELECT * FROM BattlePlayerSettings");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO BattlePlayerSettings
                        (BattlePlayerSettingsID, BattlePlayerID,
                         AnimationsEnabled, TextSpeedID, BackgroundID, ShowTypeEffectiveness, UpdatedAt)
                      VALUES (@sid, @bpid, @anim, @txt, @bg, @eff, @upd)",
                    new
                    {
                        sid = r.BattlePlayerSettingsID,
                        bpid = r.BattlePlayerID,
                        anim = r.AnimationsEnabled,
                        txt = r.TextSpeedID,
                        bg = r.BackgroundID,
                        eff = r.ShowTypeEffectiveness,
                        upd = r.UpdatedAt
                    });
            return rows.Count;
        }

        private int SyncTeams()
        {
            var rows = _remote.Query<TeamSyncRow>(
                "SELECT id, team_name, battle_player_id FROM teams");
            foreach (var r in rows)
                _local.Execute(
                    "INSERT OR REPLACE INTO teams (id, team_name, battle_player_id) VALUES (@id, @name, @bpid)",
                    new { id = r.Id, name = r.TeamName, bpid = r.BattlePlayerId });
            return rows.Count;
        }

        private int SyncTeamMembers()
        {
            var rows = _remote.Query<TeamMemberSyncRow>(
                "SELECT id, team_id, pokemonID, slot FROM team_members");
            foreach (var r in rows)
                _local.Execute(
                    "INSERT OR REPLACE INTO team_members (id, team_id, pokemonID, slot) VALUES (@id, @tid, @pid, @slot)",
                    new { id = r.Id, tid = r.TeamId, pid = r.PokemonID, slot = r.Slot });
            return rows.Count;
        }

        private int SyncBattlerPokemon()
        {
            // Only sync user-created battler_pokemon rows (those linked to a team_member).
            var rows = _remote.Query<BattlerPokemonSyncRow>(
                @"SELECT bp.* FROM battler_pokemon bp
                  WHERE EXISTS (SELECT 1 FROM team_members tm WHERE tm.pokemonID = bp.pokemonID)");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO battler_pokemon
                        (pokemonID, PokedexID, Nickname, Level, AbilityID,
                         ItemID, Move1ID, Move2ID, Move3ID, Move4ID,
                         HP_IV, Atk_IV, Def_IV, SpAtk_IV, SpDef_IV, Spd_IV,
                         HP_EV, Atk_EV, Def_EV, SpAtk_EV, SpDef_EV, Spd_EV,
                         NatureID, Gender, IsShiny)
                      VALUES
                        (@pokemonID, @pokedexID, @nickname, @level, @abilityID,
                         @itemID, @move1, @move2, @move3, @move4,
                         @hpiv, @atkiv, @defiv, @spatkiv, @spdefiv, @spdiv,
                         @hpev, @atkev, @defev, @spatkev, @spdefev, @spdev,
                         @nature, @gender, @shiny)",
                    new
                    {
                        pokemonID = r.PokemonID,
                        pokedexID = r.PokedexID,
                        nickname = r.Nickname,
                        level = r.Level,
                        abilityID = r.AbilityID,
                        itemID = r.ItemID,
                        move1 = r.Move1ID,
                        move2 = r.Move2ID,
                        move3 = r.Move3ID,
                        move4 = r.Move4ID,
                        hpiv = r.HP_IV,
                        atkiv = r.Atk_IV,
                        defiv = r.Def_IV,
                        spatkiv = r.SpAtk_IV,
                        spdefiv = r.SpDef_IV,
                        spdiv = r.Spd_IV,
                        hpev = r.HP_EV,
                        atkev = r.Atk_EV,
                        defev = r.Def_EV,
                        spatkev = r.SpAtk_EV,
                        spdefev = r.SpDef_EV,
                        spdev = r.Spd_EV,
                        nature = r.NatureID,
                        gender = r.Gender,
                        shiny = r.IsShiny
                    });
            return rows.Count;
        }

        private int SyncBattles()
        {
            var rows = _remote.Query<BattleSyncRow>(
                "SELECT BattleID, CreatedAt, Format, Status FROM Battles");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO Battles (BattleID, CreatedAt, Format, Status)
                      VALUES (@id, @created, @format, @status)",
                    new { id = r.BattleID, created = r.CreatedAt, format = r.Format, status = r.Status });
            return rows.Count;
        }

        private int SyncParticipants()
        {
            var rows = _remote.Query<ParticipantSyncRow>(
                "SELECT ParticipantID, BattleID, BattlePlayerID, TeamID, Result FROM BattleParticipants");
            foreach (var r in rows)
                _local.Execute(
                    @"INSERT OR REPLACE INTO BattleParticipants
                        (ParticipantID, BattleID, BattlePlayerID, TeamID, Result)
                      VALUES (@pid, @bid, @bpid, @tid, @result)",
                    new
                    {
                        pid = r.ParticipantID,
                        bid = r.BattleID,
                        bpid = r.BattlePlayerID,
                        tid = r.TeamID,
                        result = r.Result
                    });
            return rows.Count;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }

        // ── Private sync row types ────────────────────────────────────────────
        // Lightweight POCOs used only by this class for reading remote rows.

        private class UserSyncRow { public int UserID { get; set; } public string UserName { get; set; } public int Password { get; set; } }
        private class BattlePlayerSyncRow { public int BattlePlayerID { get; set; } public int UserID { get; set; } public string Name { get; set; } public string CreatedAt { get; set; } }
        private class BattlePlayerStatsSyncRow
        {
            public int BattlePlayerStatsID { get; set; }
            public int BattlePlayerID { get; set; }
            public int CurrentElo1v1 { get; set; }
            public int PeakElo1v1 { get; set; }
            public int Wins1v1 { get; set; }
            public int CurrentStreak1v1 { get; set; }
            public int BestStreak1v1 { get; set; }
            public int CurrentElo2v2 { get; set; }
            public int PeakElo2v2 { get; set; }
            public int Wins2v2 { get; set; }
            public int CurrentStreak2v2 { get; set; }
            public int BestStreak2v2 { get; set; }
            public int? FaveTeamID { get; set; }
        }
        private class BattlePlayerSettingsSyncRow
        {
            public int BattlePlayerSettingsID { get; set; }
            public int BattlePlayerID { get; set; }
            public int AnimationsEnabled { get; set; }
            public int TextSpeedID { get; set; }
            public int BackgroundID { get; set; }
            public int ShowTypeEffectiveness { get; set; }
            public string UpdatedAt { get; set; }
        }
        private class TeamSyncRow { public int Id { get; set; } public string TeamName { get; set; } public int BattlePlayerId { get; set; } }
        private class TeamMemberSyncRow { public int Id { get; set; } public int TeamId { get; set; } public int PokemonID { get; set; } public int Slot { get; set; } }
        private class BattlerPokemonSyncRow
        {
            public int PokemonID { get; set; }
            public int PokedexID { get; set; }
            public string Nickname { get; set; }
            public int Level { get; set; }
            public int AbilityID { get; set; }
            public int? ItemID { get; set; }
            public int? Move1ID { get; set; }
            public int? Move2ID { get; set; }
            public int? Move3ID { get; set; }
            public int? Move4ID { get; set; }
            public int HP_IV { get; set; }
            public int Atk_IV { get; set; }
            public int Def_IV { get; set; }
            public int SpAtk_IV { get; set; }
            public int SpDef_IV { get; set; }
            public int Spd_IV { get; set; }
            public int HP_EV { get; set; }
            public int Atk_EV { get; set; }
            public int Def_EV { get; set; }
            public int SpAtk_EV { get; set; }
            public int SpDef_EV { get; set; }
            public int Spd_EV { get; set; }
            public int NatureID { get; set; }
            public string Gender { get; set; }
            public int IsShiny { get; set; }
        }
        private class BattleSyncRow { public int BattleID { get; set; } public string CreatedAt { get; set; } public string Format { get; set; } public string Status { get; set; } }
        private class ParticipantSyncRow { public int ParticipantID { get; set; } public int BattleID { get; set; } public int BattlePlayerID { get; set; } public int? TeamID { get; set; } public string Result { get; set; } }
    }
}