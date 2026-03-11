using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;

        public MainWindowViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
            // LogInViewModel logInViewModel = new LogInViewModel(navigationStore,createViewModel);
            //NavigationStore.CurrentViewModel = logInViewModel;
            navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

    }
}
