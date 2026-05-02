using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class OnlineBattleMenuViewModel : ViewModelBase
    {
        private readonly UserStore _userStore;
        private readonly NavigationStore _rootNavigationStore;
        private readonly Func<BattleConnectorViewModel> _createBattleViewModel;
        private readonly TeamBuilderService _teamBuilderService; // inject this

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
                string gameMode = IsRandom ? "Random" : IsSetTeam ? "Set Team" : "No Legendaries";
                string team = IsSetTeam && SelectedTeam != null ? $"  •  {SelectedTeam.TeamName}" : "";
                return $"{mode}  •  {format}  •  {gameMode}{team}";
            }
        }

        public ICommand PlayCommand { get; }

        public OnlineBattleMenuViewModel(
            UserStore userStore,
            NavigationStore rootNavigationStore,
            Func<BattleConnectorViewModel> createBattleConnectorViewModel) // add this param
        {
            _userStore = userStore;
            _rootNavigationStore = rootNavigationStore;
            _createBattleViewModel = createBattleConnectorViewModel;
            _teamBuilderService = new TeamBuilderService();
            RefreshSavedTeams();
            PlayCommand = new RelayCommand(() =>
            {
                userStore.BattleSesion.IsOnlineMode = IsOnline;
                userStore.BattleSesion.IsOneVOne = Is1v1;
                userStore.BattleSesion.BattleMode = IsRandom ? BattleMode.halfTeam
                                                     : IsSetTeam ? BattleMode.fullTeam
                                                     : BattleMode.TwoThirdsTeam;
                userStore.BattleSesion.SelectedTeamId = SelectedTeam?.Id;
                _rootNavigationStore.CurrentViewModel = _createBattleViewModel();

            });
        }

        private void RefreshSavedTeams()
        {
            SavedTeams = _teamBuilderService.GetTeamsByBattlePlayer(_userStore.BattlePlayerID);
            SelectedTeam = SavedTeams.FirstOrDefault();
        }
    }
}