using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services;
using PokemonGame.Services.DataProvider;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;
using System.Windows.Input;

namespace PokemonGame.ViewModels.SignUp
{
    public class LogInViewModel : ViewModelBase
    {
        private readonly LogInHandler _handler;

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

        public ICommand LoginCommand { get; }
        public ICommand SwitchToSignUpCommand { get; }

        public LogInViewModel()
        {  
            var provider = new SQLiteConnectionService("C:\\Users\\yoav\\source\\repos\\PokemonGame\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db");

            GameDataProvider handler = new SQLiteDataProvider(provider);

            _handler = new LogInHandler(handler);

            LoginCommand = new RelayCommand(Login);
            SwitchToSignUpCommand = new RelayCommand(SwitchToSignUp);
        }

        private void Login()
        {
            if (_handler.Login(Username, Password))
            {
                // Login successful, you can navigate to a different ViewModel here
                Console.WriteLine("Login success");
            }
            else
            {
                Console.WriteLine("Login failed");
            }
        }

        private void SwitchToSignUp()
        {

        }
    }
}
