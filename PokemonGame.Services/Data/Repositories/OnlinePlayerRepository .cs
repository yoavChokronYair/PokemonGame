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

        // Updated: This should now target your new stats logic (Elo/Streaks)
        // For now, I've removed the Wins/Losses increment logic
        public void UpdatePlayerStats(int battlePlayerID, bool won)
        {
            // You will likely move this logic to a BattleSettingsRepository 
            // to update CurrentElo1v1, CurrentStreak1v1, etc.
        }

        public BattlePlayerData? LoadOnlinePlayerByID(int battlePlayerID) =>
            _db.QuerySingle<BattlePlayerData>(
                "SELECT * FROM BattlePlayer WHERE BattlePlayerID = @id",
                new { id = battlePlayerID });

        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user) =>
            GetAllCached(
                () => _db.Query<BattlePlayerData>("SELECT * FROM BattlePlayer WHERE UserID = @uid", new { uid = user.UserID }).ToList(),
                p => Key(p.Name, user.UserID));

        public BattlePlayerData? LoadOpponentPlayer(BattlePlayerData player, int battleID) =>
            _db.QuerySingle<BattlePlayerData>(@"
            SELECT bp.* FROM BattleTeam bt
            JOIN BattlePlayer bp ON bp.BattlePlayerID = bt.BattlePlayerID
            WHERE bt.BattleID = @battleID AND bt.BattlePlayerID != @playerID LIMIT 1;",
                new { battleID, playerID = player.BattlePlayerID });
    }
}