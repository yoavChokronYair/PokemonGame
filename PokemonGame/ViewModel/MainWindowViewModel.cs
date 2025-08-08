using PokemonGameModel.Model.Data;
using PokemonGameModel.Model.Helper;
using PokemonGameModel.Model.Manager;
using PokemonGameModel.ViewModel.BattleMenu;
using PokemonGameModel.ViewModel.Map;
using PokemonGameModel.ViewModel.ViewModelHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGameModel.ViewModel
{
    public class MainWindowViewModel:ViewModelBase
    {
        private readonly NavigationStore _NavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;

        public MainWindowViewModel()
        {
            _NavigationStore = new NavigationStore();
            
            _NavigationStore.CurrentViewModel = new MapViewModel(GameDataManager.Instance.MapData.maps,_NavigationStore,this);
            _NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        }
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
