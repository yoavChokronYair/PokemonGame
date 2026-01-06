using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.OnlineBattle;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;
using PokemonGame.ViewModels.ViewModelPage.SignUp;
using System.Windows.Input;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class SideMenuViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;

        private bool _isMenuOpen = true;
        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set
            {
                if (_isMenuOpen != value)
                {
                    _isMenuOpen = value;
                    OnPropertyChanged(nameof(IsMenuOpen));
                }
            }
        }

        public ICommand ToggleMenuCommand { get; }
        public ICommand HistoryCommand { get; }
        public ICommand FriendsCommand { get; }
        public ICommand TeamCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand ExitCommand { get; }

        public SideMenuViewModel(
            NavigationStore navigationStore,
            Func<HistoryBattleViewModel> createHistoryViewModel,
            Func<OnlineFriendsViewModel> createFriendsViewModel,
            Func<TeamViewModel> createTeamViewModel,
            Func<ProfileViewModel> createProfileViewModel,
            Func<GameModeChooserViewModel> createMainMenuViewModel)
        {
            _navigationStore = navigationStore;

            ToggleMenuCommand = new RelayCommand(ToggleMenu);

            HistoryCommand = new NavigateCommand(
                navigationStore,
                createHistoryViewModel
            );

            FriendsCommand = new NavigateCommand(
                navigationStore,
                createFriendsViewModel
            );

            TeamCommand = new NavigateCommand(
                navigationStore,
                createTeamViewModel
            );

            ProfileCommand = new NavigateCommand(
                navigationStore,
                createProfileViewModel
            );

            ExitCommand = new NavigateCommand(
                navigationStore,
                createMainMenuViewModel
            );
        }

        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }
    }
}
