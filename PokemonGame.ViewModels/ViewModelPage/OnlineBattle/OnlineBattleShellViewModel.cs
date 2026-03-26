using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class OnlineBattleShellViewModel : ViewModelBase
    {
        public SideMenuViewModel SideMenu { get; }

        private readonly NavigationStore _contentNavigationStore;

        public ViewModelBase CurrentContentViewModel => _contentNavigationStore.CurrentViewModel;

        public OnlineBattleShellViewModel(
            NavigationStore contentNavigationStore,
            SideMenuViewModel sideMenu)
        {
            _contentNavigationStore = contentNavigationStore;
            SideMenu = sideMenu;

            // Optional: Listen for changes in CurrentViewModel and notify the view
            _contentNavigationStore.CurrentViewModelChanged += () =>
            {
                OnPropertyChanged(nameof(CurrentContentViewModel));
            };
        }
    }
}
