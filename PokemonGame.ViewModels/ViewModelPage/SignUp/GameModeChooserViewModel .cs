// PokemonGame.ViewModels/ViewModelPage/SignUp/GameModeChooserViewModel.cs
// CHANGE: OnOnlineModeAsync now creates SyncService and OnlineBattleService on UserStore
// before navigating to the online shell. Everything else is identical.

using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;

namespace PokemonGame.ViewModels.ViewModelPage.SignUp
{
    public class GameModeChooserViewModel : ViewModelBase
    {
        // ── Config — set these to match your hosted server ────────────────────
        // REST base URL (HTTP port 5000)
        private const string ServerHttpUrl = "http://192.168.0.7:5000";
        private const string TcpHost = "192.168.0.7";
        private const int TcpPort = 5001;
        // ─────────────────────────────────────────────────────────────────────

        private readonly GameModeChooserService _handler;
        private readonly LogInService _loginService;
        private readonly UserStore? _userStore;
        private readonly IDialogService _dialogService;
        private readonly NavigationStore _navigationStore;

        private string _userName = string.Empty;
        public string Username
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public ICommand StoryModeCommand { get; }
        public ICommand OnlineModeCommand { get; }
        public ICommand QuickLoginCommand { get; }
        public ICommand CreateAccountCommand { get; }
        public ICommand NavigateToSideMenuCommand { get; }

        public GameModeChooserViewModel(
            UserStore? user,
            NavigationStore navigationStore,
            IDialogService dialogService,
            Func<OnlineBattleShellViewModel> createSideMenuViewModel)
        {
            _dialogService = dialogService;
            _navigationStore = navigationStore;
            _handler = new GameModeChooserService();
            _loginService = new LogInService();
            _userStore = user;
            Username = user?.Username ?? string.Empty;

            StoryModeCommand = new RelayCommand(OnStoryMode);
            OnlineModeCommand = new AsyncRelayCommand(OnOnlineModeAsync);
            QuickLoginCommand = new AsyncRelayCommand(OnQuickLoginAsync);
            CreateAccountCommand = new AsyncRelayCommand(OnCreateAccountAsync);
            NavigateToSideMenuCommand = new NavigateCommand(navigationStore, createSideMenuViewModel);
        }

        private void OnStoryMode()
        {
            // TODO: navigate to story mode
        }

        private async void OnOnlineModeSelected()
        {
            _userStore.SyncService = new SyncService("http://server-ip:5000", ServiceFactory.Instance);
            _userStore.OnlineBattleService = new OnlineBattleService();

            await _userStore.OnlineBattleService.ConnectAsync("server-ip", 5001);

            // Flush any queued syncs from offline sessions
            await _userStore.SyncService.RetryPendingAsync();

            // Push current player data to server
            await _userStore.SyncService.SyncPlayerToServerAsync(_userStore.BattlePlayerID);
        }
        private async Task OnQuickLoginAsync()
        {
            var currentUser = _loginService.GetUser(Username);
            if (currentUser == null) return;

            var users = _handler.GetAllOnlinePlayers(currentUser)
                                .Select(u => u.Name)
                                .ToList();

            string? selectedUser = await _dialogService.ShowSelectionAsync(
                "Select Account", "Choose your username:", users);

            if (string.IsNullOrWhiteSpace(selectedUser)) return;

            await OnOnlineModeAsync();
        }

        private async Task OnCreateAccountAsync()
        {
            string? newUser = await _dialogService.ShowInputAsync("Create Account", "Choose a username:");
            if (string.IsNullOrWhiteSpace(newUser)) return;

            var currentUser = _loginService.GetUser(Username);
            if (currentUser == null) return;

            if (_handler.AddOnlineModePlayer(newUser, currentUser))
            {
                Username = newUser;
                await _dialogService.ShowSuccessAsync("Success", $"Account '{newUser}' created! You can now log in.");
                NavigateToSideMenuCommand.Execute(null);
            }
            else
            {
                await _dialogService.ShowErrorAsync("Error", "Username already taken.");
            }
        }
        private async Task OnOnlineModeAsync()
        {
            Console.WriteLine($"[GAMEMODE] OnOnlineModeAsync — user={_userStore?.Username} playerId={_userStore?.BattlePlayerID}");

            if (_userStore == null)
            {
                Console.WriteLine($"[GAMEMODE] ERROR — UserStore is null");
                await _dialogService.ShowErrorAsync("Error", "User session not found.");
                return;
            }

            try
            {
                _userStore.SyncService = new SyncService(ServerHttpUrl, ServiceFactory.Instance);
                _userStore.OnlineBattleService = new OnlineBattleService();

                Console.WriteLine($"[GAMEMODE] Connecting TCP — {TcpHost}:{TcpPort}");
                await _userStore.OnlineBattleService.ConnectAsync(TcpHost, TcpPort);
                Console.WriteLine($"[GAMEMODE] TCP connected");

                await _userStore.SyncService.RetryPendingAsync();
                await _userStore.SyncService.SyncPlayerToServerAsync(_userStore.BattlePlayerID);

                Console.WriteLine($"[GAMEMODE] Navigating to online shell");
                NavigateToSideMenuCommand.Execute(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GAMEMODE] OnOnlineModeAsync ERROR: {ex.Message}");
                await _dialogService.ShowErrorAsync("Connection Error", $"Could not connect to server: {ex.Message}");
            }
        }
    }
}