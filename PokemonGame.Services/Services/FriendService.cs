using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class FriendService
    {
        private readonly FriendRepository _friends;

        public FriendService()
        {
            _friends = ServiceFactory.Instance.FriendRepository;
        }

        // Get confirmed friends
        public List<BattlePlayerFriendData> GetActiveFriends(int playerID)
        {
            return _friends.GetFriends(playerID);
        }

        // Get received requests
        public List<BattlePlayerFriendData> GetIncomingRequests(int playerID)
        {
            return _friends.GetPendingRequests(playerID);
        }

        // Business logic for sending a request
        public bool SendRequest(int senderID, int receiverID)
        {
            // Prevent adding self or sending request if relationship already exists
            if (senderID == receiverID || _friends.RelationshipExists(senderID, receiverID))
            {
                return false;
            }

            _friends.SendFriendRequest(senderID, receiverID);
            return true;
        }

        // Business logic for accepting
        public void AcceptRequest(int senderID, int receiverID)
        {
            _friends.AcceptFriendRequest(senderID, receiverID);
        }

        // Common method to terminate any relationship (Accepted or Pending)
        public void RemoveFriendship(int player1, int player2)
        {
            _friends.RemoveFriendship(player1, player2);
        }

        // Helper to update stats after a battle
        public void UpdateBattleStats(int playerID, int friendID, bool won)
        {
            _friends.UpdateRivalryStats(playerID, friendID, won);
        }
    }
}