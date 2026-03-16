using System.Windows;
using PokemonGame.ViewModels;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;
using PokemonGame.ViewModels.ViewModelPage.SignUp;
using PokemonGame.ViewModels.ViewModelPage.Summery;

namespace PokemonGame
{
    public partial class App : Application
    {
        private readonly NavigationStore _navigationStore;
        private readonly UserStore _userStore;

        public App()
        {
            _navigationStore = new NavigationStore();
            _userStore = new UserStore();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _navigationStore.CurrentViewModel = CreateMoveSummaryViewModel();
            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_navigationStore)
            };
            MainWindow.Show();
            base.OnStartup(e);
        }

        // ---------------- ROOT NAVIGATION ----------------

        private LogInViewModel CreateLogInViewModel()
        {
            return new LogInViewModel(
                _userStore,
                _navigationStore,
                CreateSignUpViewModel,
                CreateGameModeChooserViewModel
            );
        }

        private SignUpViewModel CreateSignUpViewModel()
        {
            return new SignUpViewModel(
                _userStore,
                _navigationStore,
                CreateLogInViewModel,
                CreateGameModeChooserViewModel
            );
        }

        private GameModeChooserViewModel CreateGameModeChooserViewModel()
        {
            return new GameModeChooserViewModel(
                _userStore,
                _navigationStore,
                new DialogService(),
                CreateOnlineBattleShellViewModel
            );
        }

        // ---------------- ONLINE BATTLE SHELL ----------------

        private OnlineBattleShellViewModel CreateOnlineBattleShellViewModel()
        {
            var contentNavigationStore = new NavigationStore();
            return new OnlineBattleShellViewModel(
                contentNavigationStore,
                CreateSideMenuViewModel(contentNavigationStore)
            );
        }

        private SideMenuViewModel CreateSideMenuViewModel(NavigationStore contentNavigationStore)
        {
            return new SideMenuViewModel(
                contentNavigationStore,
                CreateHistoryViewModel,
                CreateFriendsViewModel,
                CreateTeamSelectPageViewModel,
                CreateProfileViewModel
            );
        }

        // ---------------- CONTENT VIEWMODELS ----------------

        private HistoryBattleViewModel CreateHistoryViewModel()
        {
            return new HistoryBattleViewModel(_userStore);
        }

        private OnlineFriendsViewModel CreateFriendsViewModel()
        {
            return new OnlineFriendsViewModel();
        }

        private TeamSelectPageViewModel CreateTeamSelectPageViewModel()
        {
            return new TeamSelectPageViewModel(_userStore);
        }

        private ProfileViewModel CreateProfileViewModel()
        {
            return new ProfileViewModel(_userStore);
        }
        private MoveSummaryViewModel CreateMoveSummaryViewModel()
        {
            return new MoveSummaryViewModel();
        }

    }
}