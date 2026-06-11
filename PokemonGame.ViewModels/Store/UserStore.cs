using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;

namespace PokemonGame.ViewModels.Store
{
    public class UserStore : ViewModelBase
    {
        private static readonly Lazy<UserStore> _instance =
            new Lazy<UserStore>(() => new UserStore());

        public static UserStore Instance => _instance.Value;

        private UserStore()
        {
            BattleSesion = new BattleSession();
            
        }
        
        // ── Identity ──────────────────────────────────────────────────────────
        public string Username { get; set; } = "Guest";
        public int BattlePlayerID { get; set; }
        public int UserID { get; set; }
        public int PlayerID { get; set; }

        private UserSettings _settings = new();
        public OnlineReconnectMonitor? ReconnectMonitor { get; set; }
        public UserSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        // ── Pre-battle session ────────────────────────────────────────────────
        public BattleSession BattleSesion { get; set; }

        // ── Online infrastructure ─────────────────────────────────────────────
        private bool _isOnline;
        public bool IsOnline
        {
            get => _isOnline;
            set => SetProperty(ref _isOnline, value);
        }

        private string _serverBaseUrl = string.Empty;
        public string ServerBaseUrl
        {
            get => _serverBaseUrl;
            set => SetProperty(ref _serverBaseUrl, value);
        }

        private ServiceResolver _resolver = new ServiceResolver(false, string.Empty);
        public ServiceResolver Resolver
        {
            get => _resolver;
            set => SetProperty(ref _resolver, value);
        }

        // ── Matchmaking service ───────────────────────────────────────────────
        private IMatchmakingService? _matchmaking;
        public IMatchmakingService? Matchmaking
        {
            get => _matchmaking;
            set => SetProperty(ref _matchmaking, value);
        }

        private IBattleService? _battleService;
        public IBattleService? BattleService
        {
            get => _battleService ?? (Resolver.IsOnline ? Resolver.BattleService : null);
            set => SetProperty(ref _battleService, value);
        }

        // ── Active session ID ─────────────────────────────────────────────────
        private string? _activeSessionId;
        public string? ActiveSessionId
        {
            get => _activeSessionId;
            set => SetProperty(ref _activeSessionId, value);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    public enum BattleMode
    {
        halfTeam,
        TwoThirdsTeam,
        fullTeam
    }

    public enum BotDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum TextSpeed
    {
        Slow,
        Mid,
        Fast
    }

    public enum Background
    {
        White,
        Red,
        Blue
    }

    public class BattleSession
    {
        public int? RivalTeamId { get; set; }

        public bool IsOnlineMode { get; set; } = false;
        public bool IsOneVOne { get; set; } = false;

        public BattleMode BattleMode { get; set; } = BattleMode.fullTeam;

        public int? SelectedTeamId { get; set; }

        public List<int> SelectedPokemonIds { get; set; } = new();

        public BotDifficulty BotDifficulty { get; set; } = BotDifficulty.Medium;

        public List<int> RivalPokemonIds { get; set; } = new();

        // Resolved by BattleConnectorViewModel before BattleViewModel is created.
        // Offline mode uses these directly.
        // Online mode sends SelectedPokemonIds to the server, and the server builds the real teams.
        public PokemonTeam? ResolvedPlayerTeam { get; set; }
        public PokemonTeam? ResolvedBotTeam { get; set; }
    }

    public class UserSettings : ViewModelBase
    {
        public TextSpeed textSpeed { get; set; } = TextSpeed.Mid;
        public bool AnimationOn { get; set; } = false;

        private Background _background = Background.White;
        public Background background
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }

        public bool ShowTypeEffect { get; set; } = false;
    }

    public static class SettingsMapper
    {
        public static void ApplyToUserSettings(
            BattlePlayerSettingsData data,
            UserSettings target)
        {
            target.AnimationOn = data.AnimationsEnabled == 1;
            target.ShowTypeEffect = data.ShowTypeEffectiveness == 1;
            target.textSpeed = data.TextSpeedID > 0
                ? (TextSpeed)(data.TextSpeedID - 1)
                : TextSpeed.Slow;

            target.background = data.BackgroundID > 0
                ? (Background)(data.BackgroundID - 1)
                : Background.White;
        }
    }
}