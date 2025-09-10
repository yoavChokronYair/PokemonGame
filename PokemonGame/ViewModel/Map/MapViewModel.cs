using CommunityToolkit.Mvvm.Input;
using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Manager;
using PokemonGameModel.Model.Map;
using PokemonGameModel.ViewModel.ViewModelHelper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace PokemonGameModel.ViewModel.Map
{
    public class MapViewModel : ViewModelBase
    {
        // Constants
        private const int ViewWidth = 19;
        private const int ViewHeight = 15;
        private const int ScreenWidth = 900;
        private const int ScreenHeight = 600;

        // Fields
        private readonly NavigationStore _navigationStore;
        private readonly DispatcherTimer enemyDirectionTimer;

        private Direction playerDirection = Direction.Down;
        private Direction enemyDirection = Direction.Up;

        private PlayerOnMap player;

        // Public Properties
        public MainWindowViewModel MainWindowViewModel { get; }
        public GameMap CurrentGameMap { get; }

        public ICommand DirectionCommand { get; }

        private int playerX;
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

        // Constructor
        public MapViewModel(MapDataList mapData, NavigationStore navigationStore, MainWindowViewModel mainWindow)
        {
            _navigationStore = navigationStore;
            MainWindowViewModel = mainWindow;

            DirectionCommand = new AsyncRelayCommand<string>(OnDirectionInput);

            TileList = new ObservableCollection<TileViewModel>();
            CurrentGameMap = GameMap.GetInstance(mapData);

            // Start player in world coordinates (5,5)
            player = new PlayerOnMap(5, 5);

            InitializeView();

            enemyDirectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            enemyDirectionTimer.Start();
        }

        private void InitializeView()
        {
            PlayerX = player.WorldX;
            PlayerY = player.WorldY;
            Rows = ViewHeight;
            Columns = ViewWidth;

            InitializeTiles();
        }

        private async Task OnDirectionInput(string input)
        {
            switch (input.ToLower())
            {
                case "left":
                    playerDirection = Direction.Left;
                    player.Move(-1, 0,playerDirection);
                    break;
                case "right":
                    playerDirection = Direction.Right;
                    player.Move(1, 0, playerDirection);
                    break;
                case "up":
                    playerDirection = Direction.Up;
                    player.Move(0, -1, playerDirection);
                    break;
                case "down":
                    playerDirection = Direction.Down;
                    player.Move(0, 1, playerDirection);
                    break;
            }

            PlayerX = player.WorldX;
            PlayerY = player.WorldY;

            InitializeTiles(); // Refresh visible area
            await HandleEncounterAsync();
        }

        private async Task HandleEncounterAsync()
        {
            // TODO: encounter logic
        }

        private void InitializeTiles()
        {
            TileList.Clear();
            int tilePixelWidth = ScreenWidth / Columns;
            int tilePixelHeight = ScreenHeight / Rows;

            int halfWidth = ViewWidth / 2;
            int halfHeight = ViewHeight / 2;

            int startX = Math.Max(0, player.WorldX - halfWidth);
            int startY = Math.Max(0, player.WorldY - halfHeight);

            for (int y = 0; y < ViewHeight; y++)
            {
                for (int x = 0; x < ViewWidth; x++)
                {
                    int worldX = startX + x;
                    int worldY = startY + y;

                    string color = "Black"; // fallback
                    if (worldX >= 0 && worldX < CurrentGameMap.WorldWidth &&
                        worldY >= 0 && worldY < CurrentGameMap.WorldHeight)
                    {
                        int index = worldY * CurrentGameMap.WorldWidth + worldX;
                        var tile = CurrentGameMap.WorldTiles[index];

                        if (tile.Item2 == TileTypeSecondLayer.player)
                            color = "Red";
                        else
                        {
                            switch (tile.Item1)
                            {
                                case TileTypeFirstLayer.Empty: color = "White"; break;
                                case TileTypeFirstLayer.Path: color = "Gray"; break;
                                case TileTypeFirstLayer.Grass: color = "Green"; break;
                                case TileTypeFirstLayer.Water: color = "Blue"; break;
                                case TileTypeFirstLayer.Black: color = "Black"; break;
                                case TileTypeFirstLayer.Trainer: color = "Yellow"; break;
                                default: color = "Magenta"; break;
                            }
                        }
                    }

                    TileList.Add(new TileViewModel
                    {
                        Width = tilePixelWidth,
                        Height = tilePixelHeight,
                        Color = color
                    });
                }
            }
        }
    }
}
