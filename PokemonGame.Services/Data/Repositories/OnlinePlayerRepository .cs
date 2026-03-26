using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Repositories
{
    internal class OnlinePlayerRepository : DbRepository<string, BattlePlayerData>
    {
        internal OnlinePlayerRepository(IDbConnectionService db) : base(db) { }

        private static string Key(string username, int userID) => $"{username}_{userID}";

        // Loads a player persona and their current session stats
        public BattlePlayerData? LoadOnlinePlayerByName(string username, int userID) =>
            GetCached(Key(username, userID), () => _db.QuerySingle<BattlePlayerData>(
                "SELECT * FROM BattlePlayer WHERE Name = @name AND UserID = @uid",
                new { name = username, uid = userID }));

        public bool OnlinePlayerExists(string username, UserData user) =>
            ExistsCached(Key(username, user.UserID), () => LoadOnlinePlayerByName(username, user.UserID) != null);

        // Creates a new mini-account with default level and clean history
        public BattlePlayerData CreateOnlinePlayer(string username, UserData user)
        {
            _db.Execute(@"
                INSERT INTO BattlePlayer (UserID, Name, Level, Wins, Losses, CreatedAt) 
                VALUES (@uid, @name, 5, 0, 0, datetime('now'));",
                new { uid = user.UserID, name = username });

            return StoreAndReturn(Key(username, user.UserID), () =>
                _db.QuerySingle<BattlePlayerData>("SELECT * FROM BattlePlayer WHERE BattlePlayerID = last_insert_rowid();"));
        }

        // Logic to update stats after a battle completes
        public void UpdatePlayerStats(int battlePlayerID, bool won)
        {
            string query = won
                ? "UPDATE BattlePlayer SET Wins = Wins + 1 WHERE BattlePlayerID = @id"
                : "UPDATE BattlePlayer SET Losses = Losses + 1 WHERE BattlePlayerID = @id";

            _db.Execute(query, new { id = battlePlayerID });
        }

        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user) =>
            GetAllCached(
                () => _db.Query<BattlePlayerData>("SELECT * FROM BattlePlayer WHERE UserID = @uid", new { uid = user.UserID }).ToList(),
                p => Key(p.Name, user.UserID));

        // Opponents change per-battle; direct query for real-time data
        public BattlePlayerData? LoadOpponentPlayer(BattlePlayerData player, int battleID) =>
            _db.QuerySingle<BattlePlayerData>(@"
                SELECT bp.* FROM BattleTeam bt
                JOIN BattlePlayer bp ON bp.BattlePlayerID = bt.BattlePlayerID
                WHERE bt.BattleID = @battleID AND bt.BattlePlayerID != @playerID LIMIT 1;",
                new { battleID, playerID = player.BattlePlayerID });
    }
}