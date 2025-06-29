using CommunityToolkit.Mvvm.Input;
using PokemonGame.Enums;
using PokemonGame.Model.Data;
using PokemonGame.Model.Manager;
using PokemonGame.Model.Map;
using PokemonGame.ViewModel.BattleMenu;
using PokemonGame.ViewModel.ViewModelHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PokemonGame.ViewModel
{
    public class MapViewModel:ViewModelBase
    {
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
        private GameMap currentGameMap;
        private MapData currentMapData;
        private Direction playerDirection = Direction.Down;
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

        public MapViewModel(MapData mapData)
        {
            TileList = new ObservableCollection<TileViewModel>();
            
            GameMap gameMap = GameMap.GenerateGameMapFromRegions(mapData);
            currentGameMap = gameMap;
            currentMapData = mapData;
            DirectionCommand = new RelayCommand<string>(OnDirectionInput);
            playerX = 5; // Or center of map
            playerY = 5;
            LoadMap(gameMap, playerX, playerY);

        }
        private void OnDirectionInput(string direction)
        {
            int newX = playerX;
            int newY = playerY;
            Direction newDirection = playerDirection;

            switch (direction)
            {
                case "W":
                case "Up":
                    newDirection = Direction.Up;
                    newY--;
                    break;
                case "S":
                case "Down":
                    newDirection = Direction.Down;
                    newY++;
                    break;
                case "A":
                case "Left":
                    newDirection = Direction.Left;
                    newX--;
                    break;
                case "D":
                case "Right":
                    newDirection = Direction.Right;
                    newX++;
                    break;
                default:
                    return;
            }

            // Check if moving to a different map
            if (newX < 0 && currentMapData.LeftMap != null)
            {
                SwitchToNewMap(currentMapData.LeftMap, currentGameMap.Tiles[0].Length - 1, newY, newDirection);
            }
            else if (newX >= currentMapData.Width && currentMapData.RightMap != null)
            {
                SwitchToNewMap(currentMapData.RightMap, 0, newY, newDirection);
            }
            else if (newY < 0 && currentMapData.UpMap != null)
            {
                SwitchToNewMap(currentMapData.UpMap, newX, currentGameMap.Tiles.Length - 1, newDirection);
            }
           
            else if (newX >= 0 && newX < currentMapData.Width && newY >= 0 && newY < currentMapData.Height)
            {
                playerX = newX;
                playerY = newY;
                playerDirection = newDirection;
                LoadMap(currentGameMap, playerX, playerY);
            }
            if (TileType.Grass == GetTileAt(newX, newY))
            {
                Console.WriteLine("pokemon");
            }
        }
        private bool isFirstLoad = true;
        private void SwitchToNewMap(string newMapName, int newX, int newY, Direction newDirection)
        {
            var newMapData = GameDataManager.Instance.MapData.maps.FirstOrDefault(m => m.Name == newMapName);
            if (newMapData != null)
            {
                currentMapData = newMapData;
                currentGameMap = GameMap.GenerateGameMapFromRegions(newMapData);
                playerX = newX;
                playerY = newY;
                playerDirection = newDirection;
                LoadMap(currentGameMap, playerX, playerY);
            }
        }

        private void LoadMap(GameMap gameMap, int centerX, int centerY)
        {
            const int viewWidth = 19;
            const int viewHeight = 15;

            currentGameMap = gameMap;
            Rows = viewHeight;
            Columns = viewWidth;

            int tilePixelWidth = 900 / Columns;
            int tilePixelHeight = 600 / Rows;

            int mapRows = gameMap.Tiles.Length;
            int mapCols = gameMap.Tiles[0].Length;
            
            int halfWidth = viewWidth / 2;
            int halfHeight = viewHeight / 2;

            if (isFirstLoad)
            {
                TileList.Clear(); // One-time initialization
                for (int row = 0; row < viewHeight; row++)
                {
                    for (int col = 0; col < viewWidth; col++)
                    {
                        TileList.Add(new TileViewModel
                        {
                            Width = tilePixelWidth,
                            Height = tilePixelHeight
                        });
                    }
                }
                isFirstLoad = false;
            }

            for (int row = 0; row < viewHeight; row++)
            {
                for (int col = 0; col < viewWidth; col++)
                {
                    int mapY = centerY - halfHeight + row;
                    int mapX = centerX - halfWidth + col;

                    int index = row * viewWidth + col;
                    var tile = TileList[index];

                    tile.Width = tilePixelWidth;
                    tile.Height = tilePixelHeight;

                    if (mapX >= 0 && mapX < mapCols && mapY >= 0 && mapY < mapRows)
                    {
                        if (mapX == centerX && mapY == centerY)
                        {
                            tile.Color = Brushes.Red;
                            tile.X1 = tilePixelWidth / 2;
                            tile.Y1 = tilePixelHeight / 2;

                            switch (playerDirection)
                            {
                                case Direction.Up:
                                    tile.X2 = tile.X1;
                                    tile.Y2 = 0;
                                    break;
                                case Direction.Down:
                                    tile.X2 = tile.X1;
                                    tile.Y2 = tilePixelHeight;
                                    break;
                                case Direction.Left:
                                    tile.X2 = 0;
                                    tile.Y2 = tile.Y1;
                                    break;
                                case Direction.Right:
                                    tile.X2 = tilePixelWidth;
                                    tile.Y2 = tile.Y1;
                                    break;
                            }
                        }
                        else
                        {
                            tile.Color = GetBrushForTile(gameMap.Tiles[mapY][mapX]);
                            tile.X1 = tile.X2 = tile.Y1 = tile.Y2 = 0;
                        }
                    }
                    else
                    {
                        GameMap neighborMap = currentGameMap;
                        MapData neighborData = null;
                        string neighborMapName = null;
                        int neighborX = -1;
                        int neighborY = -1;

                        if (mapX < 0 && currentMapData.LeftMap != null)
                        {
                            neighborMapName = currentMapData.LeftMap;
                            neighborX = currentGameMap.Tiles[0].Length + mapX;
                            neighborY = mapY;
                        }
                        else if (mapX >= currentGameMap.Tiles[0].Length && currentMapData.RightMap != null)
                        {
                            neighborMapName = currentMapData.RightMap;
                            neighborX = mapX - currentGameMap.Tiles[0].Length;
                            neighborY = mapY;
                        }
                        else if (mapY < 0 && currentMapData.UpMap != null)
                        {
                            neighborMapName = currentMapData.UpMap;
                            neighborY = currentGameMap.Tiles.Length + mapY;
                            neighborX = mapX;
                        }
                        if (!string.IsNullOrEmpty(neighborMapName))
                        {
                            neighborData = GameDataManager.Instance.MapData.maps
                                            .FirstOrDefault(m => m.Name == neighborMapName);

                            if (neighborData != null)
                            {
                                neighborMap = GameMap.GenerateGameMapFromRegions(neighborData); // or some dictionary like GameMapDictionary[neighborData.Name]
                            }
                        }
                        if (neighborMap.Name != currentGameMap.Name && 
                            neighborX >= 0 && neighborX < neighborMap.Tiles[0].Length &&
                            neighborY >= 0 && neighborY < neighborMap.Tiles.Length)
                        {
                            tile.Color = GetBrushForTile(neighborMap.Tiles[neighborY][neighborX]);
                        }
                        else
                        {
                            tile.Color = Brushes.Black;
                        }

                        tile.X1 = tile.X2 = tile.Y1 = tile.Y2 = 0;

                    }
                }
            }
        }
        private TileType GetTileAt(int x, int y)
        {
            if (x >= 0 && x < currentMapData.Width && y >= 0 && y < currentMapData.Height)
                return currentGameMap.Tiles[y][x];
            return TileType.Empty;
        }
        private Brush GetBrushForTile(TileType type)
        {
            switch (type)
            {
                case TileType.Path:
                    return Brushes.Gray;
                case TileType.Grass:
                    return Brushes.Green;
                case TileType.Water:
                    return Brushes.Blue;
                case TileType.Empty:
                    return Brushes.White;
                default:
                    return Brushes.Red; // Unknown tile type
            }

        }
    }
}
