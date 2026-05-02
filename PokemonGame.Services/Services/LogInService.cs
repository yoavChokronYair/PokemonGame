using System.Security.Cryptography;
using System.Text;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class LogInService
    {
        private readonly UserRepository _users;

        public LogInService()
        {
            _users = ServiceFactory.Instance.UserRepository;
        }
        internal LogInService(UserRepository users)
        {
            _users = users;
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var user = _users.LoadUserByName(username);
            if (user == null)
            {
                return false;
            }

            return user.Password == HashPassword(password);
        }

        public bool UserExists(string username) => _users.UserExists(username);

        public UserData? GetUser(string username) => _users.LoadUserByName(username);

        private int HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}