using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services;
using PokemonGame.Services.Data.DataProvider;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using System.Windows.Input;

namespace PokemonGame.ViewModels.ViewModelPage.SignUp
{
    public class LogInViewModel : ViewModelBase
    {
        private readonly NavigationStore NavigationStore;
        private readonly UserStore _userStore;
        public ViewModelBase CurrentViewModel => NavigationStore.CurrentViewModel;

        private readonly LogInService _handler;

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
        public ICommand NavigateToGameModeChooserCommand { get; }   

        public LogInViewModel(UserStore user, NavigationStore navigationStore, Func<SignUpViewModel> createViewModel, Func<GameModeChooserViewModel> createGameChooserViewModel)
        {  
            _userStore = user;
            NavigationStore = navigationStore;
            _handler = new LogInService();
            LoginCommand = new RelayCommand(Login);
            SwitchToSignUpCommand = new NavigateCommand(navigationStore,createViewModel);
            NavigateToGameModeChooserCommand =
            new NavigateCommand(navigationStore, createGameChooserViewModel);
        }

        private void Login()
        {
            if (_handler.Login(Username, Password))
            {
                // Login successful, you can navigate to a different ViewModel here
                Console.WriteLine("Login success");
                _userStore.Username = Username;
                NavigateToGameModeChooserCommand.Execute(null);


            }
            else
            {
                Console.WriteLine("Login failed");
            }
        }

       
    }
}
