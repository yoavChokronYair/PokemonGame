using PokemonGame.ViewModels.Map;
using PokemonGame.Model.Helper;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels
{
    public class MainWindowViewModel:ViewModelBase
    {
        private readonly NavigationStore _NavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;

        public MainWindowViewModel()
        {
            _NavigationStore = new NavigationStore();
            
           // _NavigationStore.CurrentViewModel = new MapViewModel(GameDataManager.Instance.MapData.maps[0],_NavigationStore,this);
            _NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
