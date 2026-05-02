using System;
using System.Windows;
using PokemonGame.ViewModels;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;
using PokemonGame.ViewModels.ViewModelPage;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;
using PokemonGame.ViewModels.ViewModelPage.SignUp;
using PokemonGame.ViewModels.ViewModelPage.Summery;
using PokemonGame.Views.Pages.OnlineBattlePages;

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
            _userStore.BattleSesion = new BattleSesion();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _navigationStore.CurrentViewModel = CreateLogInViewModel();
            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_navigationStore)
            };
            MainWindow.Show();
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

        private OnlineBattleShellViewModel _onlineBattleShellViewModel;
        private NavigationStore _contentNavigationStore;

        // cached content VMs
        private ViewModels.ViewModelPage.OnlineBattle.OnlineBattleMenuViewModel _battleMenuViewModel;
        private HistoryBattleViewModel _historyBattleViewModel;
        private OnlineFriendsViewModel _onlineFriendsViewModel;
        private TeamBuilderViewModel _teamBuilderViewModel;
        private ProfileViewModel _profileViewModel;

        private OnlineBattleShellViewModel CreateOnlineBattleShellViewModel()
        {
            if (_onlineBattleShellViewModel != null)
                return _onlineBattleShellViewModel;

            _contentNavigationStore = new NavigationStore();
            _contentNavigationStore.CurrentViewModel = GetOnlineBattleMenuViewModel();

            _onlineBattleShellViewModel = new OnlineBattleShellViewModel(
                _contentNavigationStore,
                CreateSideMenuViewModel(_contentNavigationStore)
            );

            return _onlineBattleShellViewModel;
        }

        private SideMenuViewModel CreateSideMenuViewModel(NavigationStore contentNavigationStore)
        {
            return new SideMenuViewModel(
                contentNavigationStore,
                _navigationStore,
                GetOnlineBattleMenuViewModel,
                GetHistoryViewModel,
                GetFriendsViewModel,
                GetTeamSelectPageViewModel,
                GetProfileViewModel,
                CreateGameModeChooserViewModel
            );
        }

        // ---------------- CACHED CONTENT VIEWMODELS ----------------

        private OnlineBattleMenuViewModel GetOnlineBattleMenuViewModel()
        {
            if (_battleMenuViewModel == null)
                _battleMenuViewModel = new OnlineBattleMenuViewModel(_userStore, _navigationStore, CreateBattleConnectorViewModel);
            return _battleMenuViewModel;
        }

        private HistoryBattleViewModel GetHistoryViewModel()
        {
            if (_historyBattleViewModel == null)
                _historyBattleViewModel = new HistoryBattleViewModel(_userStore);
            return _historyBattleViewModel;
        }

        private OnlineFriendsViewModel GetFriendsViewModel()
        {
            if (_onlineFriendsViewModel == null)
                _onlineFriendsViewModel = new OnlineFriendsViewModel(_userStore, new DialogService());
            return _onlineFriendsViewModel;
        }

        private TeamBuilderViewModel GetTeamSelectPageViewModel()
        {
            if (_teamBuilderViewModel == null)
                _teamBuilderViewModel = new TeamBuilderViewModel(_userStore);
            return _teamBuilderViewModel;
        }

        private ProfileViewModel GetProfileViewModel()
        {
            if (_profileViewModel == null)
                _profileViewModel = new ProfileViewModel(_userStore);
            return _profileViewModel;
        }
        private MoveSummaryViewModel CreateMoveSummaryViewModel()
        {
            return new MoveSummaryViewModel();
        }
        private BattleViewModel CreateBattleViewModel()
        {
            return new BattleViewModel(_userStore);
        }
        private BattleConnectorViewModel CreateBattleConnectorViewModel()
        {
            return new BattleConnectorViewModel(
                _userStore,
                _navigationStore,
                CreateBattleViewModel
            );
        }
        private MapViewModel CreateMapViewModel()
        {
            return new MapViewModel();

        }
    }
}