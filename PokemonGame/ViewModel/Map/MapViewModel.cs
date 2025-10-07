using CommunityToolkit.Mvvm.Input;
using PokemonGame.Enums;
using PokemonGame.Model.BattleSystem.Bot;
using PokemonGame.Model.BattleSystem.Player;
using PokemonGame.Model.Data;
using PokemonGame.Model.Data.NpcData;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Manager;
using PokemonGame.Model.Map;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel.ViewModelHelper;
using PokemonGame.Model.Data.MapData;
using PokemonGame.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static PokemonGame.Model.Map.GameMap;

namespace PokemonGame.ViewModel.Map
{
    public class MapViewModel : ViewModelBase
    {
        // Constants

        int count = 0;
        private const int ViewWidth = 19;
        private const int ViewHeight = 15;
        private const int ScreenWidth = 900;
        private const int ScreenHeight = 600;

        // Fields
        private readonly NavigationStore _navigationStore;
        private Direction playerDirection = Direction.Down;

        // Public Properties
        public MainWindowViewModel MainWindowViewModel { get; }
        public GameMap CurrentGameMap { get; }
        public MapData MapData { get; }

        public ICommand DirectionCommand { get; }


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
        private Direction enemyDirection = Direction.Up;
        public Direction EnemyDirection
        {
            get => enemyDirection;
            set
            {
                if (enemyDirection != value)
                {
                    enemyDirection = value;
                    OnPropertyChanged(nameof(EnemyDirection));
                }
            }
        }

        private readonly DispatcherTimer enemyDirectionTimer;
        // Constructor
        public MapViewModel(MapData mapData, NavigationStore navigationStore, MainWindowViewModel mainWindow)
        {
            MapData = mapData;
            _navigationStore = navigationStore;
            MainWindowViewModel = mainWindow;

            DirectionCommand = new AsyncRelayCommand<string>(OnDirectionInput);

            TileList = new ObservableCollection<TileViewModel>();
            CurrentGameMap = new GameMap(mapData);

            InitializeView();

            // Setup timer to update enemy direction every 5 seconds
            enemyDirectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            enemyDirectionTimer.Tick += EnemyDirectionTimer_Tick;
            enemyDirectionTimer.Start();
        }

        private void InitializeView()
        {
            PlayerX = 5;
            PlayerY = 5;
            Rows = ViewHeight;
            Columns = ViewWidth;

            InitializeTiles();
            LoadTiles();
        }

        private async Task OnDirectionInput(string input)
        {
            if (CurrentGameMap.TryMove(input, ref playerX, ref playerY, ref playerDirection))
            {
                LoadTiles();
                await Task.Delay(100); // Small delay

             //   await HandleEncounterAsync();
            }
        }

        //private async Task HandleEncounterAsync()
        //{
        //    var tile = CurrentGameMap.GetTileAt(PlayerX, PlayerY);
        //    if (tile == TileTypeFirstLayer.Grass)
        //    {
        //       // var routeHelper = new rnghelper (GameDataManager.Instance.RouteData);
        //       // var encounter = routeHelper.GetRandomEncounter(GameDataManager.Instance.RouteData.AllRoutes[0].Name, "Grass");

        //        if (!RandomHelper.ShouldTriggerEncounter(encounter.Rarity))
        //            return;

        //        var enemyPokemon = new EnemyPokemonGeneration(
        //            encounter,
        //            GameDataManager.Instance.PokemonData.AllPokemons.FirstOrDefault(p => p.Name == encounter.Pokemon)
        //        );

        //        var playerPokemon = PlayerPokemonManager.Instance._playerPokemonTeam[0];
        //        var wildBot = new WildPokemonBot(enemyPokemon, playerPokemon);
        //        var playerBot = new PlayerPokemonBot(new List<PlayerPokemonGeneration> { playerPokemon }, wildBot._ActivePokemon);

        //        _navigationStore.CurrentViewModel = new WildPokemonBattleViewModel(playerBot, wildBot, _navigationStore, this);
        //    }
        //    if (tile == TileType.TrainerVision)
        //    {
        //        var routeHelper = new RouteEncounterHelper(GameDataManager.Instance.RouteData);
        //        var encounter = routeHelper.GetRandomEncounter(GameDataManager.Instance.RouteData.AllRoutes[0].Name, "Grass");

        //        if (!RandomHelper.ShouldTriggerEncounter(encounter.Rarity))
        //            return;

        //        var enemyPokemon = new EnemyPokemonGeneration(
        //            encounter,
        //            GameDataManager.Instance.PokemonData.AllPokemons.FirstOrDefault(p => p.Name == encounter.Pokemon)
        //        );

        //        var playerPokemon = PlayerPokemonManager.Instance._playerPokemonTeam[0];
        //        var wildBot = new WildPokemonBot(enemyPokemon, playerPokemon);
        //        var playerBot = new PlayerPokemonBot(new List<PlayerPokemonGeneration> { playerPokemon }, wildBot._ActivePokemon);

        //        _navigationStore.CurrentViewModel = new WildPokemonBattleViewModel(playerBot, wildBot, _navigationStore, this);
        //    }


        ////}

        private void InitializeTiles()
        {
            TileList.Clear();
            int tilePixelWidth = ScreenWidth / Columns;
            int tilePixelHeight = ScreenHeight / Rows;

            for (int i = 0; i < ViewWidth * ViewHeight; i++)
            {
                TileList.Add(new TileViewModel
                {
                    Width = tilePixelWidth,
                    Height = tilePixelHeight
                });
            }
        }
        private void EnemyDirectionTimer_Tick(object sender, EventArgs e)
        {
            if (count % 2 == 0)
            {
                enemyDirection = Direction.Down;
                count++;
            }
            else
            {
                count++;
                enemyDirection = Direction.Up;
            }
            LoadTiles();
        }
        private void LoadTiles()
        {
            int tilePixelWidth = ScreenWidth / Columns;
            int tilePixelHeight = ScreenHeight / Rows;

            var tiles = CurrentGameMap.GetViewportTiles(
                PlayerX,
                PlayerY,
                ViewWidth,
                ViewHeight,
                playerDirection,
                enemyDirection,
                tilePixelWidth,
                tilePixelHeight
            );


            for (int i = 0; i < tiles.Count && i < TileList.Count; i++)
            {
                TileList[i].UpdateFrom(tiles[i]);
            }
        }


    }
}
