using PokemonGame.Services.Data.DataCache;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Factory;
using System.Collections.Generic;

namespace PokemonGame.Services.Handler
{
    public class GameModeChooserService
    {
        private readonly OnlinePlayerCacheService _onlinePlayerCache;

        public GameModeChooserService()
        {
            _onlinePlayerCache = ServiceFactory.Instance.OnlinePlayerCache;
        }

        // Add a new online player
        public bool AddOnlineModePlayer(string username, UserData user)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (UserExists(username, user))
                return false;

            _onlinePlayerCache.CreateOnlinePlayer(username, user);
            return true;
        }

        // Log in an existing online player
        public bool OnlinePlayerLogIn(string username, UserData user)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var player = GetOnlinePlayer(username, user);
            return player != null;
        }

        // Check if a player exists
        public bool UserExists(string username, UserData user)
        {
            return _onlinePlayerCache.OnlinePlayerExists(username, user);
        }

        // Get a specific online player
        public BattlePlayerData? GetOnlinePlayer(string username, UserData user)
        {
            return _onlinePlayerCache.GetOnlinePlayer(username, user);
        }

        // Get all online players for a user
        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user)
        {
            return _onlinePlayerCache.GetAllOnlinePlayers(user);
        }
    }
}
