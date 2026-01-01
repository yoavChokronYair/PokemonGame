using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.User.OnlinePlayer;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Services.Data.DataProvider.SQLite
{
    internal class SQLiteOnlinePlayerRepository : IOnlinePlayerRepository
    {
        private readonly ISQLiteConnectionService _db;

        public SQLiteOnlinePlayerRepository(ISQLiteConnectionService dbService)
        {
            _db = dbService;
        }

        public BattlePlayerData CreateOnlinePlayer(string username, UserData user)
        {
            _db.Execute(
                "INSERT INTO BattlePlayer (UserID, Name, Level) VALUES (@uid, @name, 1);",
                new { uid = user.UserID, name = username });

            return _db.QuerySingle<BattlePlayerData>(
                "SELECT * FROM BattlePlayer WHERE BattlePlayerID = last_insert_rowid();");
        }

        public bool OnlinePlayerExists(string username, UserData user)
        {
            const string sql = @"SELECT COUNT(*) FROM BattlePlayer 
                                 WHERE Name = @name AND UserID = @uid;";
            int count = _db.QuerySingle<int>(sql, new { name = username, uid = user.UserID });
            return count > 0;
        }

        public BattlePlayerData? LoadOnlinePlayerByName(string username, UserData user)
        {
            const string sql = @"SELECT * FROM BattlePlayer 
                                 WHERE Name = @name AND UserID = @uid;";
            return _db.QuerySingle<BattlePlayerData?>(sql, new { name = username, uid = user.UserID });
        }

        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user) =>
            _db.Query<BattlePlayerData>(
                "SELECT * FROM BattlePlayer WHERE UserID = @uid;",
                new { uid = user.UserID }).ToList();

        public BattlePlayerData? LoadOpponentPlayer(BattlePlayerData player, int battleID)
        {
            const string sql = @"
                SELECT bp.*
                FROM BattleTeam bt
                JOIN BattlePlayer bp ON bp.BattlePlayerID = bt.BattlePlayerID
                WHERE bt.BattleID = @battleID
                  AND bt.BattlePlayerID != @playerID
                LIMIT 1;";

            return _db.QuerySingle<BattlePlayerData?>(sql,
                new { battleID, playerID = player.BattlePlayerID });
        }
    }
}
