using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services;
using PokemonGame.Services.Data.User;
using PokemonGame.Services.DataProvider;
using PokemonGame.ViewModels.ViewModelHelper;
using System;
using System.Windows.Input;

namespace PokemonGame.ViewModels
{
    public class LogInViewModel : ViewModelBase
    {
        private readonly LogInHandler loginHandler;

        public LogInViewModel()
        {
            loginHandler = new LogInHandler(GameDataProvider.Instance);

            LoginCommand = new RelayCommand(Login);
            SwitchToSignUpCommand = new RelayCommand(SwitchToSignUp);
        }

        // ============================
        // PROPERTIES
        // ============================
        private string username = "";
        public string Username
        {
            get => username;
            set
            {
                if (username != value)
                {
                    username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        private string password = "";
        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        private string loginError = "";
        public string LoginError
        {
            get => loginError;
            set
            {
                if (loginError != value)
                {
                    loginError = value;
                    OnPropertyChanged(nameof(LoginError));
                }
            }
        }

        // ============================
        // COMMANDS
        // ============================
        public ICommand LoginCommand { get; }
        public ICommand SwitchToSignUpCommand { get; }

        // ============================
        // METHODS
        // ============================
        private void Login()
        {
            LoginError = "";

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                LoginError = "Username or Password cannot be empty.";
                return;
            }

            bool success = loginHandler.Login(Username, Password);

            if (success)
            {
                // TODO: navigate to main menu or game page
                LoginError = "Login successful!";
            }
            else
            {
                LoginError = "Invalid username or password.";
            }
        }

        private void SwitchToSignUp()
        {
            // TODO: navigate to SignUp page
        }
    }
}
