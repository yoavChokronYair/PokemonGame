using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Manager;
using PokemonGame.ViewModel.BattleMenu;
using PokemonGame.ViewModel.Map;
using PokemonGame.ViewModel.ViewModelHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.ViewModel
{
    public class MainWindowViewModel:ViewModelBase
    {
        private readonly NavigationStore _NavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;

        public MainWindowViewModel()
        {
            _NavigationStore = new NavigationStore();
            RouteEncounterHelper routeEncounterViewModel = new RouteEncounterHelper(GameDataManager.Instance.RouteData);
            
            _NavigationStore.CurrentViewModel = new MapViewModel(GameDataManager.Instance.MapData.maps[0],_NavigationStore,this);
            _NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        }
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
