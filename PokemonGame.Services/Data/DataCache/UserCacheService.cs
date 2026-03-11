using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Interfaces;

namespace PokemonGame.Services.Data.DataCache
{
    public class UserCacheService
    {
        private readonly IUserRepository _repository;

        // --- Caches ---
        private readonly Dictionary<string, UserData> _userCache = new();

        internal UserCacheService(IUserRepository repository)
        {
            _repository = repository;
        }

        // Get a user by username (cached)
        public UserData? GetUserByName(string username, bool useCache = true)
        {
            if (useCache && _userCache.TryGetValue(username, out var user))
            {
                return user;
            }

            user = _repository.LoadUserByName(username);
            if (user != null && useCache)
            {
                _userCache[username] = user;
            }

            return user;
        }

        // Check if user exists
        public bool UserExists(string username, bool useCache = true)
        {
            if (useCache && _userCache.ContainsKey(username))
            {
                return true;
            }

            return _repository.UserExists(username);
        }

        // Create a new user and add to cache
        public UserData CreateUser(string username, int passwordHash, bool useCache = true)
        {
            var user = _repository.CreateUser(username, passwordHash);

            if (useCache)
            {
                _userCache[username] = user;
            }

            return user;
        }

        // Get all users (optional caching)
        public List<UserData> GetAllUsers(bool useCache = true)
        {
            if (useCache && _userCache.Count > 0)
            {
                return new List<UserData>(_userCache.Values);
            }

            var allUsers = _repository.GetAllUsers();

            if (useCache)
            {
                _userCache.Clear();
                foreach (var u in allUsers)
                {
                    _userCache[u.UserName] = u;
                }
            }

            return allUsers;
        }
    }
}
