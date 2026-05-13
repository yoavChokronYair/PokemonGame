using System;
using System.Runtime.InteropServices;
using System.Windows;
using PokemonGame.ViewModels;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;
using PokemonGame.ViewModels.ViewModelPage;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;
using PokemonGame.ViewModels.ViewModelPage.SignUp;
using PokemonGame.ViewModels.ViewModelPage.Trainer;

namespace PokemonGame
{
    public partial class App : Application
    {
        // Must match applicationUrl in server's launchSettings.json
        public const string ServerBaseUrl = "http://localhost:5000";

        private readonly NavigationStore _navigationStore;

        public App()
        {
            _navigationStore = new NavigationStore();
            UserStore.Instance.BattleSesion = new BattleSession();
            UserStore.Instance.Settings = new UserSettings();

            // Store the URL now so GameModeChooserViewModel can read it when
            // the player logs in — no other online wiring happens at startup.
            UserStore.Instance.ServerBaseUrl = ServerBaseUrl;
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

        // ---------------- ROOT NAVIGATION ----------------------------------------

        private LogInViewModel CreateLogInViewModel()
        {
            return new LogInViewModel(
                UserStore.Instance,
                _navigationStore,
                CreateSignUpViewModel,
                CreateGameModeChooserViewModel
            );
        }

        private SignUpViewModel CreateSignUpViewModel()
        {
            return new SignUpViewModel(
                UserStore.Instance,
                _navigationStore,
                CreateLogInViewModel,
                CreateGameModeChooserViewModel
            );
        }

        private GameModeChooserViewModel CreateGameModeChooserViewModel()
        {
            return new GameModeChooserViewModel(
                UserStore.Instance,
                _navigationStore,
                new DialogService(),
                CreateOnlineBattleShellViewModel,
                CreateStoryLogInViewModel
            );
        }

        // ---------------- ONLINE BATTLE SHELL ------------------------------------

        private OnlineBattleShellViewModel _onlineBattleShellViewModel;
        private NavigationStore _contentNavigationStore;

        private OnlineBattleMenuViewModel _battleMenuViewModel;
        private HistoryBattleViewModel _historyBattleViewModel;
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
                GetTeamSelectPageViewModel,
                GetProfileViewModel,
                CreateGameModeChooserViewModel
            );
        }

        // ---------------- CACHED CONTENT VIEWMODELS ------------------------------

        private OnlineBattleMenuViewModel GetOnlineBattleMenuViewModel()
        {
            if (_battleMenuViewModel == null)
                _battleMenuViewModel = new OnlineBattleMenuViewModel(
                    UserStore.Instance,
                    _navigationStore,
                    CreateBattleConnectorViewModel);

            return _battleMenuViewModel;
        }

        private HistoryBattleViewModel GetHistoryViewModel()
        {
            if (_historyBattleViewModel == null)
                _historyBattleViewModel = new HistoryBattleViewModel(UserStore.Instance);

            return _historyBattleViewModel;
        }

        private TeamBuilderViewModel GetTeamSelectPageViewModel()
        {
            if (_teamBuilderViewModel == null)
                _teamBuilderViewModel = new TeamBuilderViewModel(UserStore.Instance);

            return _teamBuilderViewModel;
        }

        private ProfileViewModel GetProfileViewModel()
        {
            if (_profileViewModel == null)
                _profileViewModel = new ProfileViewModel(UserStore.Instance);

            return _profileViewModel;
        }

        // ---------------- BATTLE -------------------------------------------------

        private BattleViewModel CreateBattleViewModel()
        {
            return new BattleViewModel(
                UserStore.Instance,
                _navigationStore,
                new DialogService(),
                CreateOnlineBattleShellViewModel
            );
        }

        // ServerBaseUrl passed so OnMatchFound inside BattleConnectorViewModel
        // can create OnlineBattleService pointing at the correct hub URL.
        private BattleConnectorViewModel CreateBattleConnectorViewModel()
        {
            return new BattleConnectorViewModel(
                UserStore.Instance,
                _navigationStore,
                CreateBattleViewModel,
                CreateOnlineBattleShellViewModel
            );
        }
        private StoryLogInViewModel CreateStoryLogInViewModel()
        {
            return new StoryLogInViewModel(
                _navigationStore,
                CreateMapViewModel
            );
        }
        private PokedexPageViewModel CreatePokedexPageViewModel()
        {
            return new PokedexPageViewModel(_navigationStore, CreateMapViewModel);
        }
        private TrainerCardViewModel CreateTrainerCardViewModel()
        {
            return new TrainerCardViewModel(_navigationStore,CreateMapViewModel);
        }
        private MapViewModel CreateMapViewModel() => new MapViewModel(_navigationStore, CreateTrainerCardViewModel,CreatePokedexPageViewModel);
    }
}