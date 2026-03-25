using System.Collections.ObjectModel;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

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




        // ── Stats (extended) ──────────────────────────────────────
        private int _draws;
        private int _currentWinStreak;
        private int _bestWinStreak;
        private int _pokemonKnockedOut;
        private int _pokemonFainted;
        private int _criticalHits;

        public int Draws { get => _draws; set => SetProperty(ref _draws, value); }
        public int CurrentWinStreak { get => _currentWinStreak; set => SetProperty(ref _currentWinStreak, value); }
        public int BestWinStreak { get => _bestWinStreak; set => SetProperty(ref _bestWinStreak, value); }
        public int PokemonKnockedOut { get => _pokemonKnockedOut; set => SetProperty(ref _pokemonKnockedOut, value); }
        public int PokemonFainted { get => _pokemonFainted; set => SetProperty(ref _pokemonFainted, value); }
        public int CriticalHits { get => _criticalHits; set => SetProperty(ref _criticalHits, value); }

        // ── Favourite Team ────────────────────────────────────────
        public ObservableCollection<TeamSlotEntry> FavouriteTeamSlots { get; } = new();

        // ── Settings (extended) ───────────────────────────────────
        private bool _animationsEnabled = true;
        private bool _autoConfirmMoves;
        private bool _showDamageNumbers = true;
        private bool _showTypeEffectiveness = true;

        public bool AnimationsEnabled { get => _animationsEnabled; set => SetProperty(ref _animationsEnabled, value); }
        public bool AutoConfirmMoves { get => _autoConfirmMoves; set => SetProperty(ref _autoConfirmMoves, value); }
        public bool ShowDamageNumbers { get => _showDamageNumbers; set => SetProperty(ref _showDamageNumbers, value); }
        public bool ShowTypeEffectiveness { get => _showTypeEffectiveness; set => SetProperty(ref _showTypeEffectiveness, value); }

        public ProfileViewModel(UserStore userStore)
        {
            _handler = new ProfileService();
            LoadProfile(userStore.Username);
        }

        private void LoadProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            var user = _handler.GetUser(username);
            if (user == null)
            {
                return;
            }

            UserName = user.UserName;
            DisplayName = user.UserName; // defaults to username; extend later with a DisplayName column
        }
    }
}