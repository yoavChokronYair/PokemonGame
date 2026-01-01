using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services;
using PokemonGame.Services.Data.DataProvider;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;


namespace PokemonGame.ViewModels.ViewModelPage.SignUp
{
    public class SignUpViewModel : ViewModelBase
    {
        private readonly SignUpService _signUpHandler;

        private string _userName = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _statusMessage = string.Empty;

        public SignUpViewModel(NavigationStore navigationStore)
        {

            _signUpHandler = new SignUpService();

            SignUpCommand = new RelayCommand(SignUp);
        }

        // ------------------ PROPERTIES ------------------

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        // ------------------ COMMANDS ------------------

        public ICommand SignUpCommand { get; }

        private void SignUp()
        {
            StatusMessage = "";

            if (string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                StatusMessage = "All fields are required";
                return;
            }

            if (Password != ConfirmPassword)
            {
                StatusMessage = "Passwords do not match";
                return;
            }

            if (Password.Length < 6)
            {
                StatusMessage = "Password must be at least 6 characters";
                return;
            }

            var success = _signUpHandler.CreateUser(UserName, Password);

            if (!success)
            {
                StatusMessage = "Username already exists";
                return;
            }

            StatusMessage = "Account created successfully!";
        }

    }
}
