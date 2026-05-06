using PokemonGame.Services.Data.ConnectionsService;

namespace PokemonGame.Services.Data.Sync
{
    // ── Describes one syncable table ──────────────────────────────────────────

    /// <summary>
    /// Defines how a single remote table is synced into the local database.
    /// Extend by adding new SyncTableRule instances — no engine changes needed.
    /// </summary>
    public sealed class SyncTableRule
    {
        /// <summary>Logical name used as the key in _SyncMeta (e.g. "Users").</summary>
        public string TableName { get; set; }

        /// <summary>
        /// SQL to fetch rows from the remote DB.
        /// Must SELECT the UpdatedAt column and all columns needed for the upsert.
        /// </summary>
        public  string RemoteSelectSql { get; set; }

        /// <summary>
        /// Given a raw row (dynamic), returns the string key that uniquely identifies
        /// it (e.g. single PK or composite "battleId:playerId").
        /// </summary>
        public  Func<dynamic, string> RowKeySelector { get; set; }

        /// <summary>Extracts the remote UpdatedAt value from a raw row.</summary>
        public  Func<dynamic, DateTime> UpdatedAtSelector { get; set; }

        /// <summary>
        /// Performs the local upsert for a single row.
        /// Called only when the engine decides the row is newer than what we have locally.
        /// </summary>
        public Action<IDbConnectionService, dynamic> LocalUpsert { get; set; }
    }

    // ── Engine ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Applies timestamp-guard sync rules against a _SyncMeta table.
    /// Add new tables by registering additional <see cref="SyncTableRule"/> instances;
    /// the engine itself never needs to change.
    /// </summary>
    public sealed class SyncRuleEngine
    {
        private readonly IDbConnectionService _local;
        private readonly IDbConnectionService _remote;
        private readonly List<SyncTableRule> _rules = new();

        public SyncRuleEngine(IDbConnectionService local, IDbConnectionService remote)
        {
            _local = local ?? throw new ArgumentNullException(nameof(local));
            _remote = remote ?? throw new ArgumentNullException(nameof(remote));

            EnsureMetaTable();
        }

        // ── Rule registration ─────────────────────────────────────────────────

        /// <summary>Register a table rule. Call before the first sync.</summary>
        public SyncRuleEngine AddRule(SyncTableRule rule)
        {
            _rules.Add(rule ?? throw new ArgumentNullException(nameof(rule)));
            return this; // fluent
        }

        // ── Sync execution ────────────────────────────────────────────────────

        /// <summary>
        /// Runs all registered rules and returns the total number of rows upserted.
        /// </summary>
        public int RunAll()
        {
            int total = 0;
            foreach (var rule in _rules)
                total += RunRule(rule);
            return total;
        }

        /// <summary>Runs a single named rule. Useful for targeted refreshes.</summary>
        public int RunRule(string tableName)
        {
            var rule = _rules.Find(r => r.TableName == tableName)
                       ?? throw new KeyNotFoundException($"No rule registered for '{tableName}'.");
            return RunRule(rule);
        }

        // ── Core per-rule logic ───────────────────────────────────────────────

        private int RunRule(SyncTableRule rule)
        {
            var rows = _remote.Query<dynamic>(rule.RemoteSelectSql);
            int count = 0;

            foreach (var row in rows)
            {
                string key = rule.RowKeySelector(row);
                DateTime remoteTs = rule.UpdatedAtSelector(row);

                if (!ShouldUpdate(rule.TableName, key, remoteTs))
                    continue;

                rule.LocalUpsert(_local, row);
                RecordMeta(rule.TableName, key, remoteTs);
                count++;
            }

            Console.WriteLine($"[SyncRuleEngine] {rule.TableName}: {count}/{rows.Count} rows upserted.");
            return count;
        }

        // ── _SyncMeta helpers ─────────────────────────────────────────────────

        private void EnsureMetaTable()
        {
            _local.Execute(@"
                CREATE TABLE IF NOT EXISTS _SyncMeta (
                    TableName       TEXT     NOT NULL,
                    RowKey          TEXT     NOT NULL,
                    RemoteUpdatedAt DATETIME NOT NULL,
                    PRIMARY KEY (TableName, RowKey)
                )");
        }

        private bool ShouldUpdate(string table, string rowKey, DateTime remoteUpdatedAt)
        {
            var existing = _local.QuerySingle<DateTime?>(
                "SELECT RemoteUpdatedAt FROM _SyncMeta WHERE TableName = @t AND RowKey = @k",
                new { t = table, k = rowKey });

            return existing == null || remoteUpdatedAt > existing;
        }

        private void RecordMeta(string table, string rowKey, DateTime remoteUpdatedAt)
        {
            _local.Execute(@"
                INSERT INTO _SyncMeta (TableName, RowKey, RemoteUpdatedAt)
                VALUES (@t, @k, @ts)
                ON CONFLICT(TableName, RowKey)
                DO UPDATE SET RemoteUpdatedAt = excluded.RemoteUpdatedAt",
                new { t = table, k = rowKey, ts = remoteUpdatedAt });
        }
    }

    // ── Pre-built rules for every table in DbSyncService ─────────────────────

    /// <summary>
    /// Factory that builds all game-specific sync rules.
    /// To add a new table: add a new static method and call it from BuildAll().
    /// </summary>
    public static class GameSyncRules
    {
        public static IEnumerable<SyncTableRule> BuildAll() =>
        new[]
        {
            Users(),
            BattlePlayers(),
            BattlePlayerStats(),
            BattlePlayerSettings(),
            Teams(),
            TeamMembers(),
            BattlerPokemon(),
            Battles(),
            Participants(),
        };

        public static SyncTableRule Users() => new()
        {
            TableName = "Users",
            RemoteSelectSql = "SELECT UserID, UserName, Password, UpdatedAt FROM Users",
            RowKeySelector = row => ((object)row.UserID).ToString()!,
            UpdatedAtSelector = row => (DateTime)row.UpdatedAt,
            LocalUpsert = (db, r) => db.Execute(
                "INSERT OR REPLACE INTO Users (UserID, UserName, Password) VALUES (@uid, @name, @pw)",
                new { uid = r.UserID, name = r.UserName, pw = r.Password })
        };

        public static SyncTableRule BattlePlayers() => new()
        {
            TableName = "BattlePlayer",
            RemoteSelectSql = "SELECT BattlePlayerID, UserID, Name, CreatedAt, UpdatedAt FROM BattlePlayer",
            RowKeySelector = row => ((object)row.BattlePlayerID).ToString()!,
            UpdatedAtSelector = row => (DateTime)row.UpdatedAt,
            LocalUpsert = (db, r) => db.Execute(
                @"INSERT OR REPLACE INTO BattlePlayer (BattlePlayerID, UserID, Name, CreatedAt)
                  VALUES (@id, @uid, @name, @createdAt)",
                new { id = r.BattlePlayerID, uid = r.UserID, name = r.Name, createdAt = r.CreatedAt })
        };

        public static SyncTableRule BattlePlayerStats() => new()
        {
            TableName = "BattlePlayerStats",
            RemoteSelectSql = "SELECT *, UpdatedAt FROM BattlePlayerStats",
            RowKeySelector = row => ((object)row.BattlePlayerID).ToString()!,
            UpdatedAtSelector = row => (DateTime)row.UpdatedAt,
            LocalUpsert = (db, r) => db.Execute(
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
                })
        };

        public static SyncTableRule BattlePlayerSettings() => new()
        {
            TableName = "BattlePlayerSettings",
            RemoteSelectSql = "SELECT *, UpdatedAt FROM BattlePlayerSettings",
            RowKeySelector = row => ((object)row.BattlePlayerID).ToString()!,
            UpdatedAtSelector = row => (DateTime)row.UpdatedAt,
            LocalUpsert = (db, r) => db.Execute(
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
                })
        };

        public static SyncTableRule Teams() => new()
        {
            TableName = "teams",
            RemoteSelectSql = "SELECT id, team_name, battle_player_id, UpdatedAt FROM teams",
            RowKeySelector = row => ((object)row.id).ToString()!,
            UpdatedAtSelector = row => (DateTime)row.UpdatedAt,
            LocalUpsert = (db, r) => db.Execute(
                "INSERT OR REPLACE INTO teams (id, team_name, battle_player_id) VALUES (@id, @name, @bpid)",
                new { id = r.id, name = r.team_name, bpid = r.battle_player_id })
        };

        public static SyncTableRule TeamMembers() => new()
        {
            TableName = "team_members",
            RemoteSelectSql = "SELECT team_id, pokemonID, slot_number, UpdatedAt FROM team_members",
            // Composite PK → combine into one string key
            RowKeySelector = row => $"{row.team_id}:{row.pokemonID}",
            UpdatedAtSelector = row => (DateTime)row.UpdatedAt,
            LocalUpsert = (db, r) => db.Execute(
                @"INSERT OR REPLACE INTO team_members (team_id, pokemonID, slot_number)
                  VALUES (@tid, @pid, @slot)",
                new { tid = r.team_id, pid = r.pokemonID, slot = r.slot_number })
        };

        public static SyncTableRule BattlerPokemon() => new()
        {
            TableName = "battler_pokemon",
            RemoteSelectSql =
                @"SELECT bp.*, bp.UpdatedAt FROM battler_pokemon bp
                  WHERE EXISTS (SELECT 1 FROM team_members tm WHERE tm.pokemonID = bp.pokemonID)",
            RowKeySelector = row => ((object)row.pokemonID).ToString()!,
            UpdatedAtSelector = row => (DateTime)row.UpdatedAt,
            LocalUpsert = (db, r) => db.Execute(
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
                    pokemonID = r.pokemonID,
                    pokedexID = r.pokedexID,
                    abilityID = r.abilityID,
                    itemID = r.itemID,
                    shiny = r.shiny,
                    gender = r.gender,
                    level = r.level,
                    move1 = r.move1ID,
                    move2 = r.move2ID,
                    move3 = r.move3ID,
                    move4 = r.move4ID,
                    ivHp = r.iv_hp,
                    ivAtk = r.iv_atk,
                    ivDef = r.iv_def,
                    ivSpAtk = r.iv_spAtk,
                    ivSpDef = r.iv_spDef,
                    ivSpeed = r.iv_speed,
                    evHp = r.ev_hp,
                    evAtk = r.ev_atk,
                    evDef = r.ev_def,
                    evSpAtk = r.ev_spAtk,
                    evSpDef = r.ev_spDef,
                    evSpeed = r.ev_speed,
                    nature = r.nature
                })
        };

        public static SyncTableRule Battles() => new()
        {
            TableName = "Battle",
            RemoteSelectSql = "SELECT BattleID, BattleDate, UpdatedAt FROM Battle",
            RowKeySelector = row => ((object)row.BattleID).ToString()!,
            UpdatedAtSelector = row => (DateTime)row.UpdatedAt,
            LocalUpsert = (db, r) => db.Execute(
                "INSERT OR REPLACE INTO Battle (BattleID, BattleDate) VALUES (@id, @date)",
                new { id = r.BattleID, date = r.BattleDate })
        };

        public static SyncTableRule Participants() => new()
        {
            TableName = "BattleParticipants",
            RemoteSelectSql = "SELECT BattleID, BattlePlayerID, TeamID, IsWinner, UpdatedAt FROM BattleParticipants",
            // Composite PK
            RowKeySelector = row => $"{row.BattleID}:{row.BattlePlayerID}",
            UpdatedAtSelector = row => (DateTime)row.UpdatedAt,
            LocalUpsert = (db, r) => db.Execute(
                @"INSERT OR REPLACE INTO BattleParticipants (BattleID, BattlePlayerID, TeamID, IsWinner)
                  VALUES (@bid, @bpid, @tid, @winner)",
                new { bid = r.BattleID, bpid = r.BattlePlayerID, tid = r.TeamID, winner = r.IsWinner })
        };
    }
}