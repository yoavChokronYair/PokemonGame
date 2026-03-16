using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories.SQLite;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class ProfileService
    {
        private readonly SQLiteUserRepository _userCache;

        public ProfileService()
        {
            _userCache = ServiceFactory.Instance.UserRepository;
        }

        public UserData? GetUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return _userCache.LoadUserByName(username);
        }

        public bool UserExists(string username)
        {
            return _userCache.UserExists(username);
        }
    }
}