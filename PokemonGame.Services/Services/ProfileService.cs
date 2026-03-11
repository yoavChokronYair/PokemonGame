using PokemonGame.Services.Data.DataCache;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class ProfileService
    {
        private readonly UserCacheService _userCache;

        public ProfileService()
        {
            _userCache = ServiceFactory.Instance.UserCache;
        }

        public UserData? GetUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return _userCache.GetUserByName(username);
        }

        public bool UserExists(string username)
        {
            return _userCache.UserExists(username);
        }
    }
}