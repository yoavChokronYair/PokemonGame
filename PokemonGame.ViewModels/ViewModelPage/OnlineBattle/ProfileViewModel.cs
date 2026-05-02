using System.Collections.ObjectModel;
using System.Linq;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelUserControl;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly ProfileService _handler;
        private readonly UserStore _userStore;

        // ── Identity ──────────────────────────────────────────────
        private string _userName = string.Empty;
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        private string _displayName = string.Empty;
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }

        // ── Stats (Matching your 5-row sketch) ────────────────────
        private int _currentElo1v1, _currentElo2v2;
        public int CurrentElo1v1 { get => _currentElo1v1; set => SetProperty(ref _currentElo1v1, value); }
        public int CurrentElo2v2 { get => _currentElo2v2; set => SetProperty(ref _currentElo2v2, value); }

        private int _wins1v1, _peakElo1v1, _bestStreak1v1, _currentStreak1v1;
        public int Wins1v1 { get => _wins1v1; set => SetProperty(ref _wins1v1, value); }
        public int PeakElo1v1 { get => _peakElo1v1; set => SetProperty(ref _peakElo1v1, value); }
        public int BestStreak1v1 { get => _bestStreak1v1; set => SetProperty(ref _bestStreak1v1, value); }
        public int CurrentStreak1v1 { get => _currentStreak1v1; set => SetProperty(ref _currentStreak1v1, value); }

        private int _wins2v2, _peakElo2v2, _bestStreak2v2, _currentStreak2v2;
        public int Wins2v2 { get => _wins2v2; set => SetProperty(ref _wins2v2, value); }
        public int PeakElo2v2 { get => _peakElo2v2; set => SetProperty(ref _peakElo2v2, value); }
        public int BestStreak2v2 { get => _bestStreak2v2; set => SetProperty(ref _bestStreak2v2, value); }
        public int CurrentStreak2v2 { get => _currentStreak2v2; set => SetProperty(ref _currentStreak2v2, value); }

        // ── Favourite Team ────────────────────────────────────────
        public PokemonTeamViewModel FavouriteTeam { get; } = new();

        // ── Settings ──────────────────────────────────────────────
        public ObservableCollection<SettingOption> TextSpeedOptions { get; } = new();
        public ObservableCollection<SettingOption> BattleSceneOptions { get; } = new();

        private SettingOption _selectedTextSpeed;
        public SettingOption SelectedTextSpeed
        {
            get => _selectedTextSpeed;
            set { if (SetProperty(ref _selectedTextSpeed, value)) SaveSetting("TextSpeed", value?.Id ?? 0); }
        }

        private SettingOption _selectedBattleScene;
        public SettingOption SelectedBattleScene
        {
            get => _selectedBattleScene;
            set { if (SetProperty(ref _selectedBattleScene, value)) SaveSetting("BattleScene", value?.Id ?? 0); }
        }
        public ObservableCollection<SettingOption> BackgroundOptions { get; } = new();

        // Selected properties for the new settings
        private SettingOption _selectedBackground;
        public SettingOption SelectedBackground
        {
            get => _selectedBackground;
            set { if (SetProperty(ref _selectedBackground, value)) SaveSetting("Background", value?.Id ?? 0); }
        }

        private SettingOption _showTypeEffectiveness;
        public SettingOption ShowTypeEffectiveness
        {
            get => _showTypeEffectiveness;
            set { if (SetProperty(ref _showTypeEffectiveness, value)) SaveSetting("TypeEffectiveness", value?.Id ?? 0); }
        }

        public ProfileViewModel(UserStore userStore)
        {
            _userStore = userStore;
            _handler = new ProfileService();

            // 1. Initialize Options FIRST so the UI has items to show
            InitializeSettingsOptions();

            // 2. Load the actual data
            LoadProfileData();
            LoadFavouriteTeam();
        }

        private void InitializeSettingsOptions()
        {
            TextSpeedOptions.Clear();
            TextSpeedOptions.Add(new SettingOption { Id = 1, Name = "SLOW" });
            TextSpeedOptions.Add(new SettingOption { Id = 2, Name = "MID" });
            TextSpeedOptions.Add(new SettingOption { Id = 3, Name = "FAST" });

            BattleSceneOptions.Clear();
            BattleSceneOptions.Add(new SettingOption { Id = 1, Name = "ON" });
            BattleSceneOptions.Add(new SettingOption { Id = 0, Name = "OFF" });
            BackgroundOptions.Clear();
            BackgroundOptions.Add(new SettingOption { Id = 1, Name = "DEFAULT" });
            BackgroundOptions.Add(new SettingOption { Id = 2, Name = "DARK" });
            BackgroundOptions.Add(new SettingOption { Id = 3, Name = "CLASSIC" });

            // Set Initial Selection for new items
            SelectedBackground = BackgroundOptions[0];
            ShowTypeEffectiveness = BattleSceneOptions.FirstOrDefault(o => o.Id == 1); // Default ON
        }

        private void LoadProfileData()
        {
            UserName = _userStore.Username;
            DisplayName = _userStore.Username;

            // FAKE DATA for testing - Replace these with your Service/DB calls
            CurrentElo1v1 = 1500;
            PeakElo1v1 = 1620;
            Wins1v1 = 42;
            BestStreak1v1 = 12;
            CurrentStreak1v1 = 5;

            CurrentElo2v2 = 1100;
            Wins2v2 = 5;
            PeakElo2v2 = 1150;

            // Match selections to saved state
            SelectedTextSpeed = TextSpeedOptions.FirstOrDefault(o => o.Id == 2); // Default to MID
            SelectedBattleScene = BattleSceneOptions.FirstOrDefault(o => o.Id == 1); // Default to ON
        }

        private void LoadFavouriteTeam()
        {
            // Populate the team so it's not empty
            FavouriteTeam.LoadSlots(new[]
            {
                new TeamSlotDisplayEntry { PokedexId = 6, IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 25, IsEmpty = false }
            });
        }

        private void SaveSetting(string key, int value)
        {
            // Logic to update your database settings table
        }
    }

    public class SettingOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}