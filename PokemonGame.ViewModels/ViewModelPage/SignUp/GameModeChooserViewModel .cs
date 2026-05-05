using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;

namespace PokemonGame.ViewModels.ViewModelPage.SignUp
{
    public class GameModeChooserViewModel : ViewModelBase
    {
        private readonly IGameModeChooserService _handler;
        private readonly IUserService _loginService;
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
        Func<OnlineBattleShellViewModel> createSideMenuViewModel,
        IGameModeChooserService gameModeChooserService,
        IUserService userService)                               // replaces new LogInService()
        {
            _dialogService = dialogService;
            _navigationStore = navigationStore;
            _handler = gameModeChooserService;
            _loginService = userService;
            _userStore = user;
            Username = user?.Username ?? string.Empty;

            StoryModeCommand = new RelayCommand(OnStoryMode);
            OnlineModeCommand = new AsyncRelayCommand(OnOnlineModeAsync);
            QuickLoginCommand = new AsyncRelayCommand(OnQuickLoginAsync);
            CreateAccountCommand = new AsyncRelayCommand(OnCreateAccountAsync);
            NavigateToSideMenuCommand = new NavigateCommand(_navigationStore, createSideMenuViewModel);
        }


        private void OnStoryMode()
        {
            // TODO: navigate to story mode
        }

        private async Task OnOnlineModeAsync()
        {
            var currentUser = _loginService.GetUser(Username);
            if (currentUser == null)
            {
                await _dialogService.ShowErrorAsync("Error", "User not found. Please create an account first.");
                return;
            }

            var users = _handler.GetAllOnlinePlayers(currentUser)
                                .Select(p => p.Name)
                                .ToList();

            string? selectedUser = null;
            if (users.Count > 0)
            {
                selectedUser = await _dialogService.ShowSelectionAsync(
                    "Quick Login",
                    "Select your username: \n*cancel to create a new account",
                    users);
            }

            if (string.IsNullOrWhiteSpace(selectedUser))
            {
                bool createNew = await _dialogService.ShowConfirmAsync(
                    "Create Account",
                    "No account selected. Do you want to create a new account?");

                if (!createNew)
                {
                    return;
                }

                selectedUser = await _dialogService.ShowInputAsync("Create Account", "Enter a username:");
                if (string.IsNullOrWhiteSpace(selectedUser))
                {
                    return;
                }

                bool created = _handler.AddOnlineModePlayer(selectedUser, currentUser);
                if (!created)
                {
                    await _dialogService.ShowErrorAsync("Error", "Username already exists. Try another one.");
                    return;
                }

                await _dialogService.ShowSuccessAsync("Success", $"Account '{selectedUser}' created successfully!");
            }

            if (_handler.OnlinePlayerLogIn(selectedUser, currentUser))
            {
                var onlinePlayer = _handler.GetOnlinePlayer(selectedUser, _loginService.GetUser(this.Username));

                if (onlinePlayer != null)
                {
                    _userStore.BattlePlayerID = onlinePlayer.BattlePlayerID;
                    await _dialogService.ShowSuccessAsync("Success", $"Logged in successfully as '{selectedUser}'!");
                    NavigateToSideMenuCommand.Execute(null);
                }
                else
                {
                    // Handle the edge case where login was valid but the record failed to fetch
                    await _dialogService.ShowErrorAsync("Error", "Account verified, but profile data could not be retrieved.");
                }
            }
        }

        private async Task OnQuickLoginAsync()
        {
            var currentUser = _loginService.GetUser(Username);
            if (currentUser == null)
            {
                return;
            }

            var users = _handler.GetAllOnlinePlayers(currentUser)
                                .Select(u => u.Name)
                                .ToList();

            string? selectedUser = await _dialogService.ShowSelectionAsync(
                "Select Account",
                "Choose your username:",
                users);

            if (string.IsNullOrWhiteSpace(selectedUser))
            {
                return;
            }

            await OnOnlineModeAsync();
        }

        private async Task OnCreateAccountAsync()
        {
            string? newUser = await _dialogService.ShowInputAsync("Create Account", "Choose a username:");
            if (string.IsNullOrWhiteSpace(newUser))
            {
                return;
            }

            var currentUser = _loginService.GetUser(Username);
            if (currentUser == null)
            {
                return;
            }

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
    }
}