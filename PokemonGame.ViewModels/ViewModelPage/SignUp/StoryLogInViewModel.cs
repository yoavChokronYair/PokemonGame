using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Services;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.Translators;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.SignUp
{
    public class StoryLogInViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly IStoryPlayerService _service;
        private readonly PlayerLoader _playerLoader;
        private readonly Func<MapViewModel> _createMapViewModel;

        public ObservableCollection<StoryPlayerSummary> Summaries { get; } = new();

        public ICommand SelectPlayerCommand { get; }
        public ICommand NewGameCommand { get; }

        public StoryLogInViewModel(
            NavigationStore navigationStore,
            Func<MapViewModel> createMapViewModel)
        {
            _navigationStore = navigationStore;
            _createMapViewModel = createMapViewModel;
            _service = ServiceFactory.Instance.StoryPlayerService;
            _playerLoader = new PlayerLoader(_service, new MapLoader(new MapService(), UserStore.Instance.Resolver.GetPokemonService()));

            var summaries = _service.GetSummaries(UserStore.Instance.UserID);
            foreach (var s in summaries)
                Summaries.Add(s);

            SelectPlayerCommand = new RelayCommand<StoryPlayerSummary>(OnSelectPlayer);
            NewGameCommand = new RelayCommand(OnNewGame);
        }

        private void OnSelectPlayer(StoryPlayerSummary? summary)
        {
            if (summary is null) return;
            UserStore.Instance.PlayerID = summary.PlayerID;
            _playerLoader.Load();
            _navigationStore.CurrentViewModel = _createMapViewModel();
        }

        private void OnNewGame()
        {
            // TODO
        }
    }
}