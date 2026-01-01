using PokemonGame.Model.Helper;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.Services;
using PokemonGame.Services.Data.DataProvider;

namespace PokemonGame.ViewModels
{
    public class MainWindowViewModel:ViewModelBase
    {
        private readonly NavigationStore _NavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;

        public MainWindowViewModel()
        { 
        }

    }
}
