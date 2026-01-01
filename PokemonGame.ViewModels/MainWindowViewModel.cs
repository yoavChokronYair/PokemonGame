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
           // LogInViewModel logInViewModel = new LogInViewModel(navigationStore,createViewModel);
            //NavigationStore.CurrentViewModel = logInViewModel;
            navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        public ViewModelBase CurrentViewModel => NavigationStore.CurrentViewModel;

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

    }
}
