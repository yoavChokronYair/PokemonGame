using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Repositories
{
    internal class FriendRepository : DbRepository<string, BattlePlayerFriendData>
    {
        internal FriendRepository(IDbConnectionService db) : base(db) { }

        // Composite key to uniquely identify the relationship between two specific players
        private static string Key(int p1, int p2) => $"friend_{p1}_{p2}";

        // Get all accepted friends for a specific player
        public List<BattlePlayerFriendData> GetFriends(int playerID) =>
            _db.Query<BattlePlayerFriendData>(
                "SELECT * FROM BattlePlayerFriends WHERE (PlayerID = @pid OR FriendPlayerID = @pid) AND Status = 'Accepted'",
                new { pid = playerID }).ToList();

        // Get pending friend requests
        public List<BattlePlayerFriendData> GetPendingRequests(int playerID) =>
            _db.Query<BattlePlayerFriendData>(
                "SELECT * FROM BattlePlayerFriends WHERE FriendPlayerID = @pid AND Status = 'Pending'",
                new { pid = playerID }).ToList();

        // Create a new friend request
        public void SendFriendRequest(int senderID, int receiverID)
        {
            _db.Execute(
                "INSERT INTO BattlePlayerFriends (PlayerID, FriendPlayerID, Status) VALUES (@s, @r, 'Pending')",
                new { s = senderID, r = receiverID });
        }

        // Accept a request
        public void AcceptFriendRequest(int senderID, int receiverID)
        {
            _db.Execute(
                "UPDATE BattlePlayerFriends SET Status = 'Accepted' WHERE PlayerID = @s AND FriendPlayerID = @r",
                new { s = senderID, r = receiverID });
        }

        // Update win/loss stats for a rivalry
        public void UpdateRivalryStats(int playerID, int friendID, bool won)
        {
            string query = won
                ? "UPDATE BattlePlayerFriends SET Wins = Wins + 1 WHERE PlayerID = @p AND FriendPlayerID = @f"
                : "UPDATE BattlePlayerFriends SET Losses = Losses + 1 WHERE PlayerID = @p AND FriendPlayerID = @f";

            _db.Execute(query, new { p = playerID, f = friendID });
        }
        // Check if a relationship (pending or accepted) exists
        public bool RelationshipExists(int player1, int player2) =>
            _db.QuerySingle<int>(
                "SELECT COUNT(*) FROM BattlePlayerFriends WHERE (PlayerID = @p1 AND FriendPlayerID = @p2) OR (PlayerID = @p2 AND FriendPlayerID = @p1)",
                new { p1 = player1, p2 = player2 }) > 0;

        // Remove a friend or cancel a request
        public void RemoveFriendship(int player1, int player2)
        {
            _db.Execute(
                "DELETE FROM BattlePlayerFriends WHERE (PlayerID = @p1 AND FriendPlayerID = @p2) OR (PlayerID = @p2 AND FriendPlayerID = @p1)",
                new { p1 = player1, p2 = player2 });
        }
    }
}