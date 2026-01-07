using PokemonGame.ViewModels;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;
using PokemonGame.ViewModels.ViewModelPage.SignUp;
using System;
using System.Windows;

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
            _navigationStore.CurrentViewModel = CreateLogInViewModel();

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
                CreateTeamViewModel,
                CreateProfileViewModel
            );
        }

        // ---------------- CONTENT VIEWMODELS ----------------

        private HistoryBattleViewModel CreateHistoryViewModel()
        {
            return new HistoryBattleViewModel();
        }

        private OnlineFriendsViewModel CreateFriendsViewModel()
        {
            return new OnlineFriendsViewModel();
        }

        private TeamViewModel CreateTeamViewModel()
        {
            return new TeamViewModel("yoavyair");
        }

        private ProfileViewModel CreateProfileViewModel()
        {
            return new ProfileViewModel();
        }
    }
}
