using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelUserControl;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class SettingOption
    {
        // The value saved to the database (e.g., 0 for OFF, 1 for ON)
        public int Id { get; set; }

        // The text shown to the user in the UI (e.g., "MID", "DARK", "SLOW")
        public string Name { get; set; } = string.Empty;
    }
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IProfileService _profileService;  // was: ProfileService _handler
        private readonly UserStore _userStore;

        // ── Identity ──────────────────────────────────────────────
        private string _userName = string.Empty;
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        // ── Stats ─────────────────────────────────────────────────
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

        // ── Favourite Team & Collection ───────────────────────────
        public PokemonTeamViewModel FavouriteTeam { get; } = new();
        public ObservableCollection<TeamData> UserTeams { get; } = new();

        private TeamData _selectedTeam;
        public TeamData SelectedTeam
        {
            get => _selectedTeam;
            set => SetProperty(ref _selectedTeam, value);
        }

        public ICommand SetFavouriteTeamCommand { get; }

        // ── Settings ──────────────────────────────────────────────
        public ObservableCollection<SettingOption> TextSpeedOptions { get; } = new();
        public ObservableCollection<SettingOption> BattleSceneOptions { get; } = new();
        public ObservableCollection<SettingOption> BackgroundOptions { get; } = new();

        private SettingOption _selectedTextSpeed;
        public SettingOption SelectedTextSpeed
        {
            get => _selectedTextSpeed;
            set { if (SetProperty(ref _selectedTextSpeed, value)) SaveSetting("TextSpeedID", value?.Id ?? 0); }
        }

        private SettingOption _selectedBattleScene;
        public SettingOption SelectedBattleScene
        {
            get => _selectedBattleScene;
            set { if (SetProperty(ref _selectedBattleScene, value)) SaveSetting("AnimationsEnabled", value?.Id ?? 0); }
        }

        private SettingOption _selectedBackground;
        public SettingOption SelectedBackground
        {
            get => _selectedBackground;
            set { if (SetProperty(ref _selectedBackground, value)) SaveSetting("BackgroundID", value?.Id ?? 0); }
        }

        private SettingOption _showTypeEffectiveness;
        public SettingOption ShowTypeEffectiveness
        {
            get => _showTypeEffectiveness;
            set { if (SetProperty(ref _showTypeEffectiveness, value)) SaveSetting("ShowTypeEffectiveness", value?.Id ?? 0); }
        }

        public ProfileViewModel(IProfileService profileService, UserStore userStore)
        {
            _profileService = profileService;
            _userStore = userStore;

            SetFavouriteTeamCommand = new RelayCommand(OnSetFavouriteTeam);

            InitializeSettingsOptions();
            LoadProfileData();
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
        }

        private void LoadProfileData()
        {
            int bpid = _userStore.BattlePlayerID;
            var data = _profileService.GetFullProfileData(_userStore.BattlePlayerID);

            // Identity
            UserName = data.Player?.Name ?? "Unknown";

            // Stats
            CurrentElo1v1 = data.Stats.CurrentElo1v1;
            PeakElo1v1 = data.Stats.PeakElo1v1;
            Wins1v1 = data.Stats.Wins1v1;
            CurrentStreak1v1 = data.Stats.CurrentStreak1v1;
            BestStreak1v1 = data.Stats.BestStreak1v1;

            CurrentElo2v2 = data.Stats.CurrentElo2v2;
            PeakElo2v2 = data.Stats.PeakElo2v2;
            Wins2v2 = data.Stats.Wins2v2;
            CurrentStreak2v2 = data.Stats.CurrentStreak2v2;
            BestStreak2v2 = data.Stats.BestStreak2v2;

            // Settings - Match selection to DB IDs
            SelectedTextSpeed = TextSpeedOptions.FirstOrDefault(o => o.Id == data.Settings.TextSpeedID);
            SelectedBattleScene = BattleSceneOptions.FirstOrDefault(o => o.Id == data.Settings.AnimationsEnabled);
            SelectedBackground = BackgroundOptions.FirstOrDefault(o => o.Id == data.Settings.BackgroundID);
            ShowTypeEffectiveness = BattleSceneOptions.FirstOrDefault(o => o.Id == data.Settings.ShowTypeEffectiveness);

            // Teams
            UserTeams.Clear();
            foreach (var team in data.Teams) UserTeams.Add(team);

            // Load visual for Favourite Team
            UpdateFavouriteTeamVisual(data.Stats.FaveTeamID);
        }

        private void UpdateFavouriteTeamVisual(int? faveTeamId)
        {
            if (!faveTeamId.HasValue || faveTeamId.Value <= 0)
            {
                FavouriteTeam.LoadSlots(new List<TeamSlotDisplayEntry>());
                return;
            }

            // 1. Get the list of formatted pokemon (BattleHistoryPokemon objects)
            var teamPokemon = _profileService.GetTeamFormattedList(faveTeamId.Value);

            // 2. Map to TeamSlotDisplayEntry format used by your Team ViewModels
            var slots = teamPokemon.Select(p => new TeamSlotDisplayEntry
            {
                PokedexId = p.PokedexId,
                IsEmpty = false
            });

            // 3. Load into the visual ViewModel
            FavouriteTeam.LoadSlots(slots);
        }

        private void OnSetFavouriteTeam()
        {
            // Check if a team is actually selected in the combo box
            if (SelectedTeam == null) return;

            // 1. Tell the service to update the FaveTeamID in the BattlePlayerStats table
            _profileService.SetFavoriteTeam(_userStore.BattlePlayerID, SelectedTeam.Id);

            // 2. Update the visual slots in the FavouriteTeam view model
            // This calls the repo/logic to refresh the actual pokemon icons shown
            UpdateFavouriteTeamVisual(SelectedTeam.Id);
        }

        private void SaveSetting(string columnName, int value)
        {
            _profileService.UpdateSetting(_userStore.BattlePlayerID, columnName, value);
        }
    }
}