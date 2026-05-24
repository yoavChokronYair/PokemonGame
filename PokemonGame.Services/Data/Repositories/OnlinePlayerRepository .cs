using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Repositories
{
    internal class OnlinePlayerRepository : DbRepository<string, BattlePlayerData>
    {
        internal OnlinePlayerRepository(IDbConnectionService db) : base(db) { }

        private static string Key(string username, int userID) => $"{username}_{userID}";

        public BattlePlayerData? LoadOnlinePlayerByName(string username, int userID) =>
            GetCached(Key(username, userID), () => _db.QuerySingle<BattlePlayerData>(
                "SELECT * FROM BattlePlayer WHERE Name = @name AND UserID = @uid",
                new { name = username, uid = userID }));

        public bool OnlinePlayerExists(string username, UserData user) =>
            ExistsCached(Key(username, user.UserID), () => LoadOnlinePlayerByName(username, user.UserID) != null);

        // Updated: Removed Level, Wins, Losses from INSERT
        public BattlePlayerData CreateOnlinePlayer(string username, UserData user)
        {
            _db.Execute(@"
            INSERT INTO BattlePlayer (UserID, Name, CreatedAt) 
            VALUES (@uid, @name, datetime('now'));",
                new { uid = user.UserID, name = username });

            return StoreAndReturn(Key(username, user.UserID), () =>
                _db.QuerySingle<BattlePlayerData>("SELECT * FROM BattlePlayer WHERE BattlePlayerID = last_insert_rowid();"));
        }

        public BattlePlayerData? LoadOnlinePlayerByID(int battlePlayerID) =>
            _db.QuerySingle<BattlePlayerData>(
                "SELECT * FROM BattlePlayer WHERE BattlePlayerID = @id",
                new { id = battlePlayerID });

        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user) =>
            GetAllCached(
                () => _db.Query<BattlePlayerData>("SELECT * FROM BattlePlayer WHERE UserID = @uid", new { uid = user.UserID }).ToList(),
                p => Key(p.Name, user.UserID));

        public void Upsert(BattlePlayerData r)
        {
            _db.Execute(
                @"INSERT OR REPLACE INTO BattlePlayer (BattlePlayerID, UserID, Name, CreatedAt)
          VALUES (@id, @uid, @name, @createdAt)",
                new { id = r.BattlePlayerID, uid = r.UserID, name = r.Name, createdAt = r.CreatedAt });

            // Keep cache consistent — cache key is "Name_UserID"
            StoreAndReturn(Key(r.Name, r.UserID), () => r);
        }
    }
}