using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class OnlineBattleMenuViewModel : ViewModelBase
    {
        private readonly UserStore _userStore;
        private readonly NavigationStore _rootNavigationStore;
        private readonly Func<BattleViewModel> _createBattleViewModel;

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
            set { SetProperty(ref _isRandom, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        private bool _isSetTeam;
        public bool IsSetTeam
        {
            get => _isSetTeam;
            set { SetProperty(ref _isSetTeam, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        private bool _isNoLegendaries;
        public bool IsNoLegendaries
        {
            get => _isNoLegendaries;
            set { SetProperty(ref _isNoLegendaries, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        // ✅ Computed every time any radio changes
        public string SelectionSummary
        {
            get
            {
                string mode = IsOnline ? "Online" : "Offline";
                string format = Is1v1 ? "1v1" : "2v2";
                string gameMode = IsRandom ? "Random" : IsSetTeam ? "Set Team" : "No Legendaries";
                return $"{mode}  •  {format}  •  {gameMode}";
            }
        }

        public ICommand PlayCommand { get; }

        public OnlineBattleMenuViewModel(
        UserStore userStore,
        NavigationStore rootNavigationStore,
        Func<BattleViewModel> createBattleViewModel)
        {
            _userStore = userStore;
            _rootNavigationStore = rootNavigationStore;
            _createBattleViewModel = createBattleViewModel;

            PlayCommand = new RelayCommand(() =>
            {
                // This triggers the factory in App.xaml.cs 
                // which will check the current "IsRandom" state
                _rootNavigationStore.CurrentViewModel = _createBattleViewModel();
            });
        }
    }
}