using PokemonGame.Services.ApiClients;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Handler
{
    public class LocalGameModeChooserService : IGameModeChooserService
    {
        private readonly OnlinePlayerRepository _onlinePlayers;
        private readonly BattlePlayerSettingsRepository _settingsRepo;

        public LocalGameModeChooserService()
        {
            _onlinePlayers = ServiceFactory.Instance.OnlinePlayerRepository;
            _settingsRepo = ServiceFactory.Instance.BattlePlayerSettingsRepository;
        }

        internal LocalGameModeChooserService(OnlinePlayerRepository onlinePlayers, BattlePlayerSettingsRepository settingsRepo)
        {
            _onlinePlayers = onlinePlayers;
            _settingsRepo = settingsRepo;
        }

        public BattlePlayerSettingsData GetSettings(int battlePlayerId) =>
            _settingsRepo.GetSettings(battlePlayerId);

        public bool AddOnlineModePlayer(string username, UserData user)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (UserExists(username, user)) return false;

            var currentPlayers = GetAllOnlinePlayers(user);
            if (currentPlayers.Count >= 3) return false;

            _onlinePlayers.CreateOnlinePlayer(username, user);
            return true;
        }

        public bool OnlinePlayerLogIn(string username, UserData user)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return GetOnlinePlayer(username, user) != null;
        }

        public bool UserExists(string username, UserData user) =>
            _onlinePlayers.OnlinePlayerExists(username, user);

        public BattlePlayerData? GetOnlinePlayer(string username, UserData user) =>
            _onlinePlayers.LoadOnlinePlayerByName(username, user.UserID);

        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user) =>
            _onlinePlayers.GetAllOnlinePlayers(user);
    }
    public class OnlineGameModeChooserService : IGameModeChooserService
    {
        private readonly LocalGameModeChooserService _local;
        private readonly IGameModeApiClient _api;

        public OnlineGameModeChooserService(IGameModeApiClient api)
        {
            _local = new LocalGameModeChooserService();
            _api = api;
        }
        public BattlePlayerSettingsData GetSettings(int battlePlayerId)
        {
            var dto = _api.GetSettings(battlePlayerId);
            if (dto != null)
                ServiceFactory.Instance.Sync?.SyncPlayerAsync(battlePlayerId).Wait();

            return _local.GetSettings(battlePlayerId);
        }
        public bool AddOnlineModePlayer(string username, UserData user)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (UserExists(username, user)) return false;

            var currentPlayers = GetAllOnlinePlayers(user);
            if (currentPlayers.Count >= 3) return false;

            var success = _api.CreateOnlinePlayer(username, user.UserID);
            if (!success) return false;

            ServiceFactory.Instance.Sync?.SyncPlayerAsync(user.UserID).Wait();
            return true;
        }

        public bool OnlinePlayerLogIn(string username, UserData user)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return GetOnlinePlayer(username, user) != null;
        }

        public bool UserExists(string username, UserData user) =>
            _api.PlayerExists(username, user.UserID) ?? _local.UserExists(username, user);

        public BattlePlayerData? GetOnlinePlayer(string username, UserData user)
        {
            var dto = _api.GetOnlinePlayer(username, user.UserID);
            if (dto is null) return _local.GetOnlinePlayer(username, user);

            ServiceFactory.Instance.Sync?.SyncPlayerAsync(dto.BattlePlayerID).Wait();
            return _local.GetOnlinePlayer(username, user);
        }

        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user)
        {
            var result = _api.GetAllOnlinePlayers(user.UserID);
            if (result is null) return _local.GetAllOnlinePlayers(user);

            foreach (var player in result)
                ServiceFactory.Instance.Sync?.SyncPlayerAsync(player.BattlePlayerID).Wait();

            return _local.GetAllOnlinePlayers(user);
        }
    }
}