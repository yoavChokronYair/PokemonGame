using PokemonGame.Model.Helper;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.Services.DataProvider;
using PokemonGame.Services;

namespace PokemonGame.ViewModels
{
    public class MainWindowViewModel:ViewModelBase
    {
        private readonly NavigationStore _NavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;

        public MainWindowViewModel()
        {
            SQLiteDataProvider handler = new SQLiteDataProvider(new SQLiteConnectionService("C:\\Users\\yoav\\Documents\\PokemonGameDB.db"));
            
            GameDataProvider gameDataProvider = handler;
        }

    }
}
