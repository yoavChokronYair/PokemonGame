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
        private List<MapData> maps;

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
        public MapViewModel(MapDataList mapData, NavigationStore navigationStore, MainWindowViewModel mainWindow)
        {
            
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
           
            enemyDirectionTimer.Start();
        }

        public MapViewModel(List<MapData> maps, NavigationStore navigationStore, MainWindowViewModel mainWindowViewModel)
        {
            this.maps = maps;
            _navigationStore = navigationStore;
            MainWindowViewModel = mainWindowViewModel;
            GameMap map = new GameMap(GameDataManager.Instance.MapData); 
        }

        private void InitializeView()
        {
            PlayerX = 5;
            PlayerY = 5;
            Rows = ViewHeight;
            Columns = ViewWidth;

            InitializeTiles();
        }

        private async Task OnDirectionInput(string input)
        {
           // if (CurrentGameMap.TryMove(input, ref playerX, ref playerY, ref playerDirection))
            //{
                //await Task.Delay(100); // Small delay

              //  await HandleEncounterAsync();
            //}
        }

        private async Task HandleEncounterAsync()
        { 
            
        }

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
       
    }
}
