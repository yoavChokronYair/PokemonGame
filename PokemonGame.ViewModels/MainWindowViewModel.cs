using PokemonGame.Model.Helper;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.Services;
using PokemonGame.Services.Data.DataProvider;
using PokemonGame.ViewModels.ViewModelPage.SignUp;

namespace PokemonGame.ViewModels
{
    public class MainWindowViewModel:ViewModelBase
    {
        private readonly NavigationStore NavigationStore;

        public MainWindowViewModel(NavigationStore navigationStore)
        {
            NavigationStore = navigationStore;
            NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            LogInViewModel logInViewModel = new LogInViewModel(navigationStore);
            NavigationStore.CurrentViewModel = logInViewModel;
        }

        public ViewModelBase CurrentViewModel => NavigationStore.CurrentViewModel;

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
