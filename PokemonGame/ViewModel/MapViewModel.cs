using CommunityToolkit.Mvvm.Input;
using PokemonGame.Enums;
using PokemonGame.Model.BattleSystem.Bot;
using PokemonGame.Model.BattleSystem.Player;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Manager;
using PokemonGame.Model.Map;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel.BattleMenu;
using PokemonGame.ViewModel.ViewModelHelper;
using PokemonGame.Views.UserControls.PokemonBattle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace PokemonGame.ViewModel
{
    public class MapViewModel : ViewModelBase
    {
        public  MainWindowViewModel MainWindowViewModel;
        public ICommand DirectionCommand { get; }

        public GameMap currentGameMap;
        public MapData MapData { get; set; }
        private Direction playerDirection = Direction.Down;

        private int playerX = 0;
        public int PlayerX
        {
            get => playerX;
            set
            {
                if (playerX != value)
                {
                    playerX = value;
                    OnPropertyChanged(nameof(PlayerX));

                }
            }
        }
        private int playerY;
        public int PlayerY
        {
            get => playerY;
            set
            {
                if (playerY != value)
                {
                    playerY = value;
                    OnPropertyChanged(nameof(PlayerY));
                }
            }
        }
        private int rows;
        public int Rows
        {
            get => rows;
            set
            {
                if (rows != value)
                {
                    rows = value;
                    OnPropertyChanged(nameof(Rows));
                }
            }
        }
        private int columns;
        public int Columns
        {
            get => columns;
            set
            {
                if (columns != value)
                {
                    columns = value;
                    OnPropertyChanged(nameof(Columns));
                }
            }
        }
        private ObservableCollection<TileViewModel> tileList;
        public ObservableCollection<TileViewModel> TileList
        {
            get => tileList;
            set
            {
                if (tileList != value)
                {
                    tileList = value;
                    OnPropertyChanged(nameof(TileList));
                }
            }
        }

        private const int ViewWidth = 19;
        private const int ViewHeight = 15;
        private readonly NavigationStore _NavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;
        public MapViewModel(MapData mapData,NavigationStore navigation,MainWindowViewModel mainWindow)
        {
            this.MapData = mapData;
            MainWindowViewModel = mainWindow;
            _NavigationStore = navigation;
            
            DirectionCommand = new RelayCommand<string>(OnDirectionInput);
            TileList = new ObservableCollection<TileViewModel>();
            currentGameMap = new GameMap(mapData);

            PlayerX = 5;
            PlayerY = 5;

            Rows = ViewHeight;
            Columns = ViewWidth;

            InitializeTiles();
            LoadTiles();
        }
        


        private void OnDirectionInput(string input)
        {
            if (currentGameMap.TryMove(input, ref playerX, ref playerY, ref playerDirection))
            {
                LoadTiles();
                if (currentGameMap.GetTileAt(PlayerX, PlayerY) == TileType.Grass)
                {
                    RouteEncounterHelper routeEncounterViewModel = new RouteEncounterHelper(GameDataManager.Instance.RouteData);
                    Encounter encounter = routeEncounterViewModel.GetRandomEncounter(GameDataManager.Instance.RouteData.AllRoutes[0].Name,"Grass");
                    EnemyPokemonGeneration wildPokemon = new EnemyPokemonGeneration(
                        encounter,
                        GameDataManager.Instance.PokemonData.AllPokemons.FirstOrDefault(p => p.Name == encounter.Pokemon)
                    );
                    EnemyPokemonGeneration enemy = new EnemyPokemonGeneration(encounter,GameDataManager.Instance.PokemonData.AllPokemons.FirstOrDefault(m => m.Name == encounter.Pokemon));
                    PlayerPokemonGeneration _playerPokemon = PlayerPokemonManager.Instance._playerPokemonTeam[0];

                    WildPokemonBot wildPokemonBot = new WildPokemonBot(enemy,_playerPokemon);
                    List<PlayerPokemonGeneration> list = new List<PlayerPokemonGeneration>();
                    list.Add(_playerPokemon);
                    PlayerPokemonBot playerPokemon = new PlayerPokemonBot(list, wildPokemon);
                    _NavigationStore.CurrentViewModel = new WildPokemonBattleViewModel(playerPokemon,wildPokemonBot,_NavigationStore,this);
                }
            }
        }

        private void InitializeTiles()
        {
            TileList.Clear();
            int tilePixelWidth = 900 / Columns;
            int tilePixelHeight = 600 / Rows;

            for (int i = 0; i < ViewWidth * ViewHeight; i++)
            {
                TileList.Add(new TileViewModel
                {
                    Width = tilePixelWidth,
                    Height = tilePixelHeight
                });
            }
        }

        private void LoadTiles()
        {
            int tilePixelWidth = 900 / Columns;
            int tilePixelHeight = 600 / Rows;

            var tiles = currentGameMap.GetViewportTiles(PlayerX, PlayerY, ViewWidth, ViewHeight, playerDirection, tilePixelWidth, tilePixelHeight);

            for (int i = 0; i < tiles.Count && i < TileList.Count; i++)
            {
                TileList[i].UpdateFrom(tiles[i]);
            }
        }
    }
}
