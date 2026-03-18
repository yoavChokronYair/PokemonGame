#if DEBUG
using PokemonGame.Services.Handler;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.ViewModels
{
    public class DebugAutoLoginService
    {
        // ── Configure your debug credentials here ──
        private const string _debugUsername = "yoavyair";
        private const string _debugPassword = "123456";
        private const string _debugPlayerUsername = "yoav";

        private readonly LogInService _loginService;
        private readonly GameModeChooserService _gameModeService;

        public UserData? CurrentUser { get; private set; }
        public BattlePlayerData? CurrentPlayer { get; private set; }

        // ── Convenience accessors ──
        public string? CurrentUserName => CurrentUser?.UserName;
        public int CurrentPlayerID => CurrentPlayer.BattlePlayerID;

        public DebugAutoLoginService()
        {
            _loginService = new LogInService();
            _gameModeService = new GameModeChooserService();
        }

        /// <summary>
        /// Logs in a user + player automatically.
        /// Returns true if both succeeded.
        /// </summary>
        public bool AutoLogin()
        {
            // Step 1 — Log in the user
            if (!_loginService.Login(_debugUsername, _debugPassword))
                return false;

            CurrentUser = _loginService.GetUser(_debugUsername);
            if (CurrentUser == null)
                return false;

            // Step 2 — Log in or create the online player
            if (!_gameModeService.OnlinePlayerLogIn(_debugPlayerUsername, CurrentUser))
            {
                // Player doesn't exist yet — create them
                if (!_gameModeService.AddOnlineModePlayer(_debugPlayerUsername, CurrentUser))
                    return false;
            }

            CurrentPlayer = _gameModeService.GetOnlinePlayer(_debugPlayerUsername, CurrentUser);
            return CurrentPlayer != null;
        }
    }
}
#endif