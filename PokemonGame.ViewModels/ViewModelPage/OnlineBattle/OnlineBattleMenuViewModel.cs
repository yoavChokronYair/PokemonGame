using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class OnlineBattleMenuViewModel : ViewModelBase,IDisposable
    {
        private readonly UserStore _userStore;
        public UserSettings Settings => _userStore.Settings;

        private readonly NavigationStore _rootNavigationStore;
        private readonly Func<BattleConnectorViewModel> _createBattleViewModel;
        private readonly ITeamService _teamService;

        private bool _isOnline = true;
        public bool IsOnline
        {
            get => _isOnline;
            set { SetProperty(ref _isOnline, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        private bool _is1v1 = true;
        public bool Is1v1
        {
            get => _is1v1;
            set { SetProperty(ref _is1v1, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        private bool _isRandom = true;
        public bool IsRandom
        {
            get => _isRandom;
            set
            {
                SetProperty(ref _isRandom, value);
                OnPropertyChanged(nameof(SelectionSummary));
                OnPropertyChanged(nameof(ShowTeamSelector));
            }
        }

        private bool _isSetTeam;
        public bool IsSetTeam
        {
            get => _isSetTeam;
            set
            {
                SetProperty(ref _isSetTeam, value);
                OnPropertyChanged(nameof(SelectionSummary));
                OnPropertyChanged(nameof(ShowTeamSelector));
                if (value) RefreshSavedTeams();
            }
        }

        private bool _isNoLegendaries;
        public bool IsNoLegendaries
        {
            get => _isNoLegendaries;
            set
            {
                SetProperty(ref _isNoLegendaries, value);
                OnPropertyChanged(nameof(SelectionSummary));
                OnPropertyChanged(nameof(ShowTeamSelector));
            }
        }

        // Only show team picker when "Set Team" is selected
        public bool ShowTeamSelector => IsSetTeam;

        private List<TeamData> _savedTeams = new();
        public List<TeamData> SavedTeams
        {
            get => _savedTeams;
            set => SetProperty(ref _savedTeams, value);
        }

        private TeamData _selectedTeam;
        public TeamData SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                SetProperty(ref _selectedTeam, value);
                OnPropertyChanged(nameof(SelectionSummary));
            }
        }

        public string SelectionSummary
        {
            get
            {
                string mode = IsOnline ? "Online" : "Offline";
                string format = Is1v1 ? "1v1" : "2v2";
                string gameMode = IsRandom ? "3 members" : IsSetTeam ? "6 members" : "4 members";
                string team = SelectedTeam != null ? $"  •  {SelectedTeam.TeamName}" : "";
                return $"{mode}  •  {format}  •  {gameMode}{team}";
            }
        }

        public ICommand PlayCommand { get; }

        public OnlineBattleMenuViewModel(
            UserStore userStore,
            NavigationStore rootNavigationStore,
            Func<BattleConnectorViewModel> createBattleConnectorViewModel)
        {
            _userStore = userStore;
            _rootNavigationStore = rootNavigationStore;
            _createBattleViewModel = createBattleConnectorViewModel;
            _teamService = userStore.Resolver.GetTeamService();

            RefreshSavedTeams();

            // Subscribe to navigation changes
            _rootNavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            PlayCommand = new RelayCommand(() =>
            {
                if (IsOnline && !userStore.IsOnline)
                {
                    System.Windows.MessageBox.Show(
                        "You are not connected to the server.\nPlease check your connection and try again.",
                        "Connection Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;

                }
                userStore.BattleSesion.IsOnlineMode = IsOnline;
                userStore.BattleSesion.IsOneVOne = Is1v1;
                userStore.BattleSesion.BattleMode = IsRandom ? BattleMode.halfTeam
                                                     : IsSetTeam ? BattleMode.fullTeam
                                                     : BattleMode.TwoThirdsTeam;
                userStore.BattleSesion.SelectedTeamId = SelectedTeam?.Id;
                _rootNavigationStore.CurrentViewModel = _createBattleViewModel();
            });
        }

        private void OnCurrentViewModelChanged()
        {
            // Check if the newly navigated-to page is of this type
            if (_rootNavigationStore.CurrentViewModel is OnlineBattleMenuViewModel)
            {
                RefreshSavedTeams();
            }
        }

        private void RefreshSavedTeams()
        {
            // Keep a reference to the ID of the team they had selected
            int? previouslySelectedId = SelectedTeam?.Id;

            SavedTeams = _teamService.GetTeamsByBattlePlayer(_userStore.BattlePlayerID);

            // Try to restore their previous selection if it still exists in the refreshed list
            var restoredSelection = SavedTeams.FirstOrDefault(t => t.Id == previouslySelectedId);

            // Default to the first team in the list if the previously selected team is gone
            SelectedTeam = restoredSelection ?? SavedTeams.FirstOrDefault();
        }
        public void Dispose()
        {
            // Clean up event handlers to prevent memory leaks when the VM is destroyed
            _rootNavigationStore.CurrentViewModelChanged -= OnCurrentViewModelChanged;
        }
    }
}