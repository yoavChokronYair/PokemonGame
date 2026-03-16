using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories.SQLite;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class GameModeChooserService
    {
        private readonly SQLiteOnlinePlayerRepository _onlinePlayers;

        public GameModeChooserService()
        {
            _onlinePlayers = ServiceFactory.Instance.OnlinePlayerRepository;
        }

        public bool AddOnlineModePlayer(string username, UserData user)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            if (UserExists(username, user))
            {
                return false;
            }

            _onlinePlayers.CreateOnlinePlayer(username, user);
            return true;
        }

        public bool OnlinePlayerLogIn(string username, UserData user)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            return GetOnlinePlayer(username, user) != null;
        }

        public bool UserExists(string username, UserData user) =>
            _onlinePlayers.OnlinePlayerExists(username, user);

        public BattlePlayerData? GetOnlinePlayer(string username, UserData user) =>
            _onlinePlayers.LoadOnlinePlayerByName(username, user.UserID);

        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user) =>
            _onlinePlayers.GetAllOnlinePlayers(user);
    }
}