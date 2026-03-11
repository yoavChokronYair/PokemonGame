using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
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
