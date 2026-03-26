using System.Collections.ObjectModel;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelUserControl;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly ProfileService _handler;

        private string _userName;
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        private string _displayName;
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }

        private string _playerId;
        public string PlayerId { get => _playerId; set => SetProperty(ref _playerId, value); }

        // ── Stats ─────────────────────────────────────────────────
        private int _wins, _losses, _draws, _currentWinStreak, _bestWinStreak;
        private int _pokemonKnockedOut, _pokemonFainted, _criticalHits;

        public int Wins { get => _wins; set { SetProperty(ref _wins, value); OnPropertyChanged(nameof(WinRate)); OnPropertyChanged(nameof(TotalBattles)); } }
        public int Losses { get => _losses; set { SetProperty(ref _losses, value); OnPropertyChanged(nameof(WinRate)); OnPropertyChanged(nameof(TotalBattles)); } }
        public int Draws { get => _draws; set => SetProperty(ref _draws, value); }
        public int CurrentWinStreak { get => _currentWinStreak; set => SetProperty(ref _currentWinStreak, value); }
        public int BestWinStreak { get => _bestWinStreak; set => SetProperty(ref _bestWinStreak, value); }
        public int PokemonKnockedOut { get => _pokemonKnockedOut; set => SetProperty(ref _pokemonKnockedOut, value); }
        public int PokemonFainted { get => _pokemonFainted; set => SetProperty(ref _pokemonFainted, value); }
        public int CriticalHits { get => _criticalHits; set => SetProperty(ref _criticalHits, value); }
        public int TotalBattles => Wins + Losses;
        public string WinRate => TotalBattles == 0 ? "—" : $"{(Wins * 100.0 / TotalBattles):0.#}%";

        // ── Favourite Team ────────────────────────────────────────
        public PokemonTeamViewModel FavouriteTeam { get; } = new();

        // ── Settings ──────────────────────────────────────────────
        private bool _isDarkMode, _showOnlineStatus, _allowBattleRequests;
        private bool _animationsEnabled = true, _showDamageNumbers = true, _showTypeEffectiveness = true;
        private bool _autoConfirmMoves;

        public bool IsDarkMode { get => _isDarkMode; set => SetProperty(ref _isDarkMode, value); }
        public bool ShowOnlineStatus { get => _showOnlineStatus; set => SetProperty(ref _showOnlineStatus, value); }
        public bool AllowBattleRequests { get => _allowBattleRequests; set => SetProperty(ref _allowBattleRequests, value); }
        public bool AnimationsEnabled { get => _animationsEnabled; set => SetProperty(ref _animationsEnabled, value); }
        public bool AutoConfirmMoves { get => _autoConfirmMoves; set => SetProperty(ref _autoConfirmMoves, value); }
        public bool ShowDamageNumbers { get => _showDamageNumbers; set => SetProperty(ref _showDamageNumbers, value); }
        public bool ShowTypeEffectiveness { get => _showTypeEffectiveness; set => SetProperty(ref _showTypeEffectiveness, value); }

        public ProfileViewModel(UserStore userStore)
        {
            _handler = new ProfileService();
            LoadProfile(userStore.Username);
            LoadDummyFavouriteTeam();
        }

        private void LoadDummyFavouriteTeam()
        {
            FavouriteTeam.LoadSlots(new[]
            {
                new TeamSlotDisplayEntry { PokedexId = 6,   Name = "Charizard", Type1 = "Fire",     Type2 = "Flying", IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 25,  Name = "Pikachu",   Type1 = "Electric", Type2 = null,     IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 149, Name = "Dragonite", Type1 = "Dragon",   Type2 = "Flying", IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 131, Name = "Lapras",    Type1 = "Water",    Type2 = "Ice",    IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 143, Name = "Snorlax",   Type1 = "Normal",   Type2 = null,     IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 94,  Name = "Gengar",    Type1 = "Ghost",    Type2 = "Poison", IsEmpty = false },
            });
        }

        private void LoadProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return;

            var user = _handler.GetUser(username);
            if (user == null) return;

            UserName = user.UserName;
            DisplayName = user.UserName;
        }
    }
}