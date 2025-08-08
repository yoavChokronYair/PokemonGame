using CommunityToolkit.Mvvm.Input;
using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Manager;
using PokemonGameModel.Model.Map;
using PokemonGameModel.ViewModel.ViewModelHelper;
using System;
using System.Collections.Generic;
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

        private PlayerOnMap player; // NEW — handles player position & movement

        // Public Properties
        public MainWindowViewModel MainWindowViewModel { get; }
        public GameMap CurrentGameMap { get; }
        public MapData MapData => player.CurrentMap; // NEW — pulled from PlayerOnMap

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

            // Start player in first map, position (5,5)
            var startingMap = GameMap.Instance._data.Keys.First();
            int startIndex = 5 * startingMap.Width + 5;
            player = new PlayerOnMap(startingMap, startIndex);

            InitializeView();

            enemyDirectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            enemyDirectionTimer.Start();
        }

        private void InitializeView()
        {
            PlayerX = player.CurrentLocation % player.CurrentMap.Width;
            PlayerY = player.CurrentLocation / player.CurrentMap.Width;
            Rows = ViewHeight;
            Columns = ViewWidth;

            InitializeTiles();
        }

        private async Task OnDirectionInput(string input)
        {
            switch (input.ToLower())
            {
                case "left":
                    player.MoveByXY(-1, 0);
                    playerDirection = Direction.Left;
                    break;
                case "right":
                    player.MoveByXY(1, 0);
                    playerDirection = Direction.Right;
                    break;
                case "up":
                    player.MoveByXY(0, -1);
                    playerDirection = Direction.Up;
                    break;
                case "down":
                    player.MoveByXY(0, 1);
                    playerDirection = Direction.Down;
                    break;
            }

            PlayerX = player.CurrentLocation % player.CurrentMap.Width;
            PlayerY = player.CurrentLocation / player.CurrentMap.Width;

            InitializeTiles(); // Refresh the visible area
            await HandleEncounterAsync();
        }

        private async Task HandleEncounterAsync()
        {
            // Placeholder for encounter logic
        }

        private void InitializeTiles()
        {
            TileList.Clear();
            int tilePixelWidth = ScreenWidth / Columns;
            int tilePixelHeight = ScreenHeight / Rows;
            var l = GameMap.Instance.ConvertToColor(MapData);

            for (int i = 0; i < ViewWidth * ViewHeight; i++)
            {
                TileList.Add(new TileViewModel
                {
                    Width = tilePixelWidth,
                    Height = tilePixelHeight,
                    Color = l[i]
                });
            }
        }
    }
}
