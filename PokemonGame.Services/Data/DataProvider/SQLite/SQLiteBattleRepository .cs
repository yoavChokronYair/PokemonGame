using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.GameData.User.OnlinePlayer;
using PokemonGame.Services.Data.Interfaces;

namespace PokemonGame.Services.Data.DataProvider.SQLite
{
    internal class SQLiteBattleRepository : IBattleRepository
    {
        private readonly ISQLiteConnectionService _db;

        public SQLiteBattleRepository(ISQLiteConnectionService dbService)
        {
            _db = dbService;
        }

        public List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player)
        {
            const string sql = @"
                SELECT
                    b.BattleID,
                    b.BattleDate,
                    opp.Name AS OpponentName,
                    CASE WHEN b.WinnerBattlePlayerID = @pid THEN 1 ELSE 0 END AS IsWin
                FROM Battle b
                JOIN BattleTeam myTeam
                    ON myTeam.BattleID = b.BattleID
                   AND myTeam.BattlePlayerID = @pid
                JOIN BattleTeam oppTeam
                    ON oppTeam.BattleID = b.BattleID
                   AND oppTeam.BattlePlayerID != @pid
                JOIN BattlePlayer opp
                    ON opp.BattlePlayerID = oppTeam.BattlePlayerID
                ORDER BY b.BattleDate DESC;";

            return _db.Query<BattleHistoryEntryData>(sql, new { pid = player.BattlePlayerID }).ToList();
        }

        public List<PokemonData> GetBattleTeamPokemonForPlayer(int battleID, int battlePlayerID)
        {
            const string sql = @"
                SELECT p.PokemonID, p.SpeciesName
                FROM BattleTeamPokemon btp
                JOIN BattleTeam bt ON bt.BattleTeamID = btp.BattleTeamID
                JOIN Pokemon p ON p.PokemonID = btp.PokemonID
                WHERE bt.BattleID = @bid AND bt.BattlePlayerID = @pid;";

            return _db.Query<PokemonData>(sql, new { bid = battleID, pid = battlePlayerID }).ToList();
        }

        public BattlePlayerData? GetOpponentPlayer(int battleID, int playerID)
        {
            const string sql = @"
                SELECT * FROM BattlePlayer
                WHERE BattleID = @bid AND BattlePlayerID != @pid;";

            return _db.QuerySingle<BattlePlayerData?>(sql, new { bid = battleID, pid = playerID });
        }
    }
}
