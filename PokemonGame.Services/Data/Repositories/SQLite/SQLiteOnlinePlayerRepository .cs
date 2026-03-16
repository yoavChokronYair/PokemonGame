using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Repositories.SQLite
{
    // Cache key is "username_userID" to avoid clashes between accounts.
    internal class SQLiteOnlinePlayerRepository : SQLiteRepository<string, BattlePlayerData>
    {
        internal SQLiteOnlinePlayerRepository(ISQLiteConnectionService db) : base(db) { }

        private static string Key(string username, int userID) => $"{username}_{userID}";

        public BattlePlayerData? LoadOnlinePlayerByName(string username, int UserID) =>
            GetCached(Key(username, UserID), () => _db.QuerySingle<BattlePlayerData?>(
                "SELECT * FROM BattlePlayer WHERE Name = @name AND UserID = @uid",
                new { name = username, uid = UserID }));


        public bool OnlinePlayerExists(string username, UserData user) =>
            ExistsCached(Key(username, user.UserID), () => LoadOnlinePlayerByName(username, user.UserID) != null);

        // Use this when the player joins the lobby without a team yet
        public BattlePlayerData CreateOnlinePlayer(string username, UserData user)
        {
            _db.Execute("INSERT INTO BattlePlayer (UserID, Name, Level) VALUES (@uid, @name, 1);",
                new { uid = user.UserID, name = username });

            return StoreAndReturn(Key(username, user.UserID), () =>
                _db.QuerySingle<BattlePlayerData>("SELECT * FROM BattlePlayer WHERE BattlePlayerID = last_insert_rowid();"));
        }

        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user) =>
            GetAllCached(
                () => _db.Query<BattlePlayerData>("SELECT * FROM BattlePlayer WHERE UserID = @uid", new { uid = user.UserID }).ToList(),
                p => Key(p.Name, user.UserID));

        // Opponents change per-battle; not worth caching
        public BattlePlayerData? LoadOpponentPlayer(BattlePlayerData player, int battleID) =>
            _db.QuerySingle<BattlePlayerData?>(@"
                SELECT bp.* FROM BattleTeam bt
                JOIN BattlePlayer bp ON bp.BattlePlayerID = bt.BattlePlayerID
                WHERE bt.BattleID = @battleID AND bt.BattlePlayerID != @playerID LIMIT 1;",
                new { battleID, playerID = player.BattlePlayerID });
    }
}