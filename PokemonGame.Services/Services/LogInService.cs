using System.Security.Cryptography;
using System.Text;
using PokemonGame.Services.Data.DataCache;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class LogInService
    {
        private readonly UserCacheService _provider;

        public LogInService()
        {
            _provider = ServiceFactory.Instance.UserCache;
        }

        // LOGIN
        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            UserData user = _provider.GetUserByName(username);
            if (user == null)
            {
                return false;
            }

            int hash = HashPassword(password);
            return user.Password == hash;
        }



        // CHECK EXISTS
        public bool UserExists(string username)
        {
            return _provider.UserExists(username);
        }

        // GET USER
        public UserData? GetUser(string username)
        {
            return _provider.GetUserByName(username);
        }

        // PASSWORD HASH
        private int HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToInt32(bytes, 0);
            }
        }
    }
}
