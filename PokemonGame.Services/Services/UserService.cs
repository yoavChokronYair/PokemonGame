using System.Security.Cryptography;
using System.Text;
using PokemonGame.Services.ApiClients;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Handler
{
    public class LocalUserService : IUserService
    {
        private readonly UserRepository _userRepo;

        public LocalUserService()
        {
            _userRepo = ServiceFactory.Instance.UserRepository;
        }

        internal LocalUserService(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // ── Auth ──────────────────────────────────────────────────────────────
        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = _userRepo.LoadUserByName(username);
            if (user is null) return false;

            return user.Password == PasswordHelper.Hash(password);
        }

        public bool CreateUser(string username, string password)
        {
            if (UserExists(username)) return false;
            _userRepo.CreateUser(username, PasswordHelper.Hash(password));
            return true;
        }

        public bool UserExists(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            return _userRepo.UserExists(username);
        }

        // ── User data ─────────────────────────────────────────────────────────
        public UserData? GetUser(string username) =>
            _userRepo.LoadUserByName(username);
    }
    public class OnlineUserService : IUserService
    {
        private readonly LocalUserService _local;
        private readonly IUserApiClient _api;

        public OnlineUserService(IUserApiClient api)
        {
            _local = new LocalUserService();
            _api = api;
        }

        public bool Login(string username, string password)
        {
            var hashedPassword = PasswordHelper.Hash(password);

            var result = _api.Login(username, hashedPassword);
            if (result is null)
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return false;
                return _local.Login(username, password);
            }

            if (result.Success)
                SyncUserToLocal(result.UserData!);

            return result.Success;
        }

        public bool CreateUser(string username, string password)
        {
            if (_api.UserExists(username) ?? _local.UserExists(username))
                return false;

            var hashedPassword = PasswordHelper.Hash(password);

            var success = _api.CreateUser(username, hashedPassword);
            if (!success) return false;

            _local.CreateUser(username, password);
            return true;
        }

        public bool UserExists(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            return _api.UserExists(username) ?? _local.UserExists(username);
        }

        public UserData? GetUser(string username)
        {
            var dto = _api.GetUser(username);
            if (dto is null) return _local.GetUser(username);

            SyncUserToLocal(dto);
            return _local.GetUser(username);
        }

        private void SyncUserToLocal(UserData dto)
        {
            ServiceFactory.Instance.Sync?.SyncUserAsync(dto.UserID).Wait();
        }
    }
    internal static class PasswordHelper
    {
        internal static int Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}