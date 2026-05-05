using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.SignUp
{
    public class LogInViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly UserStore _userStore;

        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

        private readonly IUserService _handler;

        private string _username = "";
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
        public ICommand LoginCommand { get; }
        public ICommand SwitchToSignUpCommand { get; }
        public ICommand NavigateToGameModeChooserCommand { get; }

        public LogInViewModel(UserStore user, NavigationStore navigationStore, Func<SignUpViewModel> createViewModel, Func<GameModeChooserViewModel> createGameChooserViewModel, IUserService userService)
        {
            _userStore = user;
            _navigationStore = navigationStore;
            _handler = userService;
            LoginCommand = new RelayCommand(Login);
            SwitchToSignUpCommand = new NavigateCommand(navigationStore, createViewModel);
            NavigateToGameModeChooserCommand =
            new NavigateCommand(navigationStore, createGameChooserViewModel);
        }

        private void Login()
        {
            StatusMessage = "";
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "All fields are required";
                return;
            }
            if (_handler.Login(Username, Password))
            {
                // Login successful, you can navigate to a different ViewModel here
                Console.WriteLine("Login success");
                _userStore.Username = Username;
                NavigateToGameModeChooserCommand.Execute(null);


            }
            else
            {
                StatusMessage = "password or username invalid";
            }
        }


    }
}
