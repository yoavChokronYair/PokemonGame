using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper.Service;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PokemonGame.Services.Data.DataProvider;

namespace PokemonGame.ViewModels.SignUp
{
    public class GameModeChooserViewModel : ObservableObject
    {
        private readonly GameModeChooserService _handler;
        private readonly IDialogService _dialogService;

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

        public GameModeChooserViewModel(string username, IDialogService dialogService)
        {
            _dialogService = dialogService;

            // Safe null check for username
            var userData = string.IsNullOrWhiteSpace(username)
                ? null
                : GameDataProvider.Instance.GetAllUsers().FirstOrDefault(u => u.UserName == username);

            _handler = new GameModeChooserService(GameDataProvider.Instance, userData);

            StoryModeCommand = new RelayCommand(OnStoryMode);
            OnlineModeCommand = new AsyncRelayCommand(OnOnlineModeAsync);
            QuickLoginCommand = new AsyncRelayCommand(OnQuickLoginAsync);
            CreateAccountCommand = new AsyncRelayCommand(OnCreateAccountAsync);
        }

        private void OnStoryMode()
        {
            // TODO: navigate to story mode
        }

        private async Task OnOnlineModeAsync()
        {
            // 1️⃣ Try quick login first if users exist
            var users = _handler.GetAllOnlinePlayers().Select(u => u.Name).ToList();
            string? selectedUser = null;

            if (users.Count > 0)
            {
                selectedUser = await _dialogService.ShowSelection(
                    "Quick Login",
                    "Select your username: \n*cancel to create a new account",
                    users
                );
            }

            // 2️⃣ If no selection or no users, ask for a new username
            if (string.IsNullOrWhiteSpace(selectedUser))
            {
                bool createNew = await _dialogService.ShowConfirm(
                    "Create Account",
                    "No account selected. Do you want to create a new account?"
                );

                if (!createNew)
                    return;

                // Ask for new username
                selectedUser = await _dialogService.ShowInput("Create Account", "Enter a username:");
                if (string.IsNullOrWhiteSpace(selectedUser))
                    return; // user cancelled

                bool created = _handler.AddOnlineModePayer(selectedUser);
                if (!created)
                {
                    await _dialogService.ShowError("Error", "Username already exists. Try another one.");
                    return;
                }

                await _dialogService.ShowSuccess("Success", $"Account '{selectedUser}' created successfully!");
            }

            // 3️⃣ Attempt login with selected or newly created username
            Username = selectedUser;
            if (_handler.OnlinePlayerLogIn(Username))
            {
                await _dialogService.ShowSuccess("Success", $"Logged in successfully as '{Username}'!");
            }
            else
            {
                await _dialogService.ShowError("Error", $"Failed to log in as '{Username}'.");
            }
        }

        private async Task OnQuickLoginAsync()
        {
            var users = GameDataProvider.Instance.GetAllUsers().Select(u => u.UserName).ToList();
            string? selectedUser = await _dialogService.ShowSelection(
                "Select Account",
                "Choose your username:",
                users
            );

            if (string.IsNullOrWhiteSpace(selectedUser))
                return; // user cancelled

            Username = selectedUser;
            await OnOnlineModeAsync();

        }

        private async Task OnCreateAccountAsync()
        {
            string? newUser = await _dialogService.ShowInput("Create Account", "Choose a username:");
            if (string.IsNullOrWhiteSpace(newUser))
                return;

            if (_handler.AddOnlineModePayer(newUser))
            {
                Username = newUser;
                await _dialogService.ShowSuccess("Success", $"Account '{newUser}' created! You can now log in.");
            }
            else
            {
                await _dialogService.ShowError("Error", "Username already taken.");
            }
        }
    }
}
