using PokemonGame.ViewModels;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;
using PokemonGame.ViewModels.ViewModelPage.SignUp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PokemonGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NavigationStore _navigationStore;
        private readonly UserStore _userStore;
        public App()
        {
            _navigationStore = new NavigationStore();
            _userStore = new UserStore();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            // Set initial ViewModel
            _navigationStore.CurrentViewModel = CreateLogInViewModel();
            MainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel(_navigationStore)
            };
            MainWindow.Show();
            base.OnStartup(e);
        }
        private SignUpViewModel CreateSignUpViewModel()
        {
            return new SignUpViewModel(_userStore, _navigationStore, CreateLogInViewModel,CreateGameModeChooserViewModel);
        }
        private LogInViewModel CreateLogInViewModel() {
            return new LogInViewModel(_userStore,_navigationStore, CreateSignUpViewModel);
        }
        private GameModeChooserViewModel CreateGameModeChooserViewModel()
        {
            return new GameModeChooserViewModel(_userStore, _navigationStore,new DialogService());
        }
    }
}
