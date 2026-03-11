using System.Security.Cryptography;
using System.Text;
using PokemonGame.Services.Data.DataCache;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class SignUpService
    {
        private readonly UserCacheService _provider;

        public SignUpService()
        {
            _provider = ServiceFactory.Instance.UserCache;
        }

        public bool UserNameExists(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return false;
            }

            return _provider.UserExists(userName);

        }

        public bool CreateUser(string userName, string password)
        {
            if (UserNameExists(userName))
            {
                return false;
            }

            var hashedPassword = HashPassword(password);

            _provider.CreateUser(userName, hashedPassword);
            return true;
        }

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
