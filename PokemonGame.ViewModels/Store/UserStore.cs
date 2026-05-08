using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.ViewModelHelper;

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
        public string Username { get; set; }
        public int BattlePlayerID { get; set; }

        private UserSettings _settings = new();
        public UserSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        // ── Pre-battle session ────────────────────────────────────────────────
        public BattleSession BattleSesion { get; set; }

        // ── Online infrastructure ─────────────────────────────────────────────
        public string ServerBaseUrl { get; set; } = string.Empty;
        public ServiceResolver Resolver { get; set; } = new ServiceResolver(false, string.Empty);

        // ── Active matchmaking — null when not searching ──────────────────────
        public IMatchmakingService? Matchmaking =>
            Resolver.IsOnline ? Resolver.MatchmakingService : null;

        // ── Active battle session — set when match is found ───────────────────
        public string? ActiveSessionId { get; set; }
    }

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


        // ── NEW: resolved before BattleViewModel is created ──────────────────
        public PokemonTeam? ResolvedPlayerTeam { get; set; }
        public PokemonTeam? ResolvedBotTeam { get; set; }
    }
    public class UserSettings:ViewModelBase
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
        public static void ApplyToUserSettings(BattlePlayerSettingsData data, UserSettings target)
        {
            target.AnimationOn = data.AnimationsEnabled == 1;
            target.ShowTypeEffect = data.ShowTypeEffectiveness == 1;

            // Safely convert 1-based database integers to 0-based Enums
            target.textSpeed = data.TextSpeedID > 0
                ? (TextSpeed)(data.TextSpeedID - 1)
                : TextSpeed.Slow;

            target.background = data.BackgroundID > 0
                ? (Background)(data.BackgroundID - 1)
                : Background.White;
        }
    }
}