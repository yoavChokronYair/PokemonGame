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
        private readonly GameModeChooserService _handler;
        private readonly IDialogService _dialogService;
        private readonly NavigationStore _navigationStore;

        private string userName = string.Empty;
        public string Username
        {
            get => userName;
            set
            {
                if (userName != value)
                {
                    userName = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public ICommand StoryModeCommand { get; }
        public ICommand OnlineModeCommand { get; }
        public ICommand QuickLoginCommand { get; }
        public ICommand CreateAccountCommand { get; }
        public ICommand NavigateToSideMenuCommand { get; }  // ✅ Command for navigation

        public GameModeChooserViewModel(UserStore? user, NavigationStore navigationStore, IDialogService dialogService,
            Func<OnlineBattleShellViewModel> createSideMenuViewModel)
        {
            _dialogService = dialogService;
            _navigationStore = navigationStore;
            _handler = new GameModeChooserService();

            Username = user?.Username ?? string.Empty;

            StoryModeCommand = new RelayCommand(OnStoryMode);
            OnlineModeCommand = new AsyncRelayCommand(OnOnlineModeAsync);
            QuickLoginCommand = new AsyncRelayCommand(OnQuickLoginAsync);
            CreateAccountCommand = new AsyncRelayCommand(OnCreateAccountAsync);

            // ✅ Inject navigation command
            NavigateToSideMenuCommand = new NavigateCommand(_navigationStore, createSideMenuViewModel);
        }

        private void OnStoryMode()
        {
            // TODO: navigate to story mode
        }

        private async Task OnOnlineModeAsync()
        {
            var currentUser = ServiceFactory.Instance.UserCache.GetAllUsers()
                                .FirstOrDefault(u => u.UserName == Username);

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
                    users
                );
            }

            if (string.IsNullOrWhiteSpace(selectedUser))
            {
                bool createNew = await _dialogService.ShowConfirmAsync(
                    "Create Account",
                    "No account selected. Do you want to create a new account?"
                );

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

            if (_handler.OnlinePlayerLogIn(Username, currentUser))
            {
                await _dialogService.ShowSuccessAsync("Success", $"Logged in successfully as '{Username}'!");

                // ✅ Use NavigateCommand instead of setting CurrentViewModel manually
                NavigateToSideMenuCommand.Execute(null);
            }
            else
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to log in as '{Username}'.");
            }
        }

        private async Task OnQuickLoginAsync()
        {
            var currentUser = ServiceFactory.Instance.UserCache.GetAllUsers()
                                .FirstOrDefault(u => u.UserName == Username);

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
                users
            );

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

            var currentUser = ServiceFactory.Instance.UserCache.GetAllUsers()
                                .FirstOrDefault(u => u.UserName == Username);

            if (currentUser == null)
            {
                return;
            }

            if (_handler.AddOnlineModePlayer(newUser, currentUser))
            {
                Username = newUser;
                await _dialogService.ShowSuccessAsync("Success", $"Account '{newUser}' created! You can now log in.");

                // ✅ Navigate using NavigateCommand
                NavigateToSideMenuCommand.Execute(null);
            }
            else
            {
                await _dialogService.ShowErrorAsync("Error", "Username already taken.");
            }
        }
    }
}