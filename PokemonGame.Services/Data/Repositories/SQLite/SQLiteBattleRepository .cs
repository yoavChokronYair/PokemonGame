using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.GameData.User.OnlinePlayer;

namespace PokemonGame.Services.Data.Repositories.SQLite
{
    // Base cache handles BattleHistory keyed by playerID.
    // Team pokemon uses a tuple key so it gets its own dictionary.
    internal class SQLiteBattleRepository : SQLiteRepository<int, List<BattleHistoryEntryData>>
    {
        private readonly Dictionary<(int battleID, int playerID), List<PokemonData>> _teamCache = new();

        internal SQLiteBattleRepository(ISQLiteConnectionService db) : base(db) { }

        public List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player) =>
            GetCached(player.BattlePlayerID, () => _db.Query<BattleHistoryEntryData>(@"
                SELECT b.BattleID, b.BattleDate,
                       opp.Name AS OpponentName,
                       CASE WHEN b.WinnerBattlePlayerID = @pid THEN 1 ELSE 0 END AS IsWin
                FROM Battle b
                JOIN BattleTeam myTeam  ON myTeam.BattleID = b.BattleID  AND myTeam.BattlePlayerID = @pid
                JOIN BattleTeam oppTeam ON oppTeam.BattleID = b.BattleID AND oppTeam.BattlePlayerID != @pid
                JOIN BattlePlayer opp   ON opp.BattlePlayerID = oppTeam.BattlePlayerID
                ORDER BY b.BattleDate DESC;", new { pid = player.BattlePlayerID }).ToList());

        public List<PokemonData> GetBattleTeamPokemonForPlayer(int battleID, int battlePlayerID)
        {
            var key = (battleID, battlePlayerID);
            if (_teamCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var team = _db.Query<PokemonData>(@"
                SELECT p.PokemonID, p.SpeciesName
                FROM BattleTeamPokemon btp
                JOIN BattleTeam bt ON bt.BattleTeamID = btp.BattleTeamID
                JOIN Pokemon p     ON p.PokemonID = btp.PokemonID
                WHERE bt.BattleID = @bid AND bt.BattlePlayerID = @pid;",
                new { bid = battleID, pid = battlePlayerID }).ToList();
            _teamCache[key] = team;
            return team;
        }

        // Opponent lookup is contextual; not worth caching
        public BattlePlayerData? GetOpponentPlayer(int battleID, int playerID) =>
            _db.QuerySingle<BattlePlayerData?>(
                "SELECT * FROM BattlePlayer WHERE BattleID = @bid AND BattlePlayerID != @pid;",
                new { bid = battleID, pid = playerID });
    }
}
