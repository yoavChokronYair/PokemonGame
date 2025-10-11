using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PokemonGame.Enums;
using PokemonGame.Model.Map;
using PokemonGame.Services.Data.MapData;
using PokemonGameModel.Model.Map;

namespace PokemonGame.ViewModels.Backpack
{
    public class TileViewModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TileType Type { get; set; }
        public int BackgroundID { get; set; }
    }

    public class MiniMapViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TileViewModel> _tiles;
        public ObservableCollection<TileViewModel> Tiles
        {
            get => _tiles;
            set { _tiles = value; OnPropertyChanged(); }
        }

        public MiniMapViewModel()
        {
            Tiles = new ObservableCollection<TileViewModel>();
        }

        /// <summary>
        /// Load the world map automatically using townMaps 2D array
        /// </summary>
        public void LoadWorldMap(WorldMap worldMap, int spacing = 10)
        {
            Tiles.Clear();

            int rows = worldMap.TownMapsRows;
            int cols = worldMap.TownMapsCols;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    WorldData? mapData = worldMap.GetTownMapAt(row, col);
                    if (mapData == null) continue;

                    if (mapData is TownMapData town)
                    {
                        if (!worldMap.townMapTiles.ContainsKey(town)) continue;
                        AddTiles(worldMap.townMapTiles[town], col * spacing, row * spacing);
                    }
                    else if (mapData is RouteMapData route)
                    {
                        if (!worldMap.routeMapTiles.ContainsKey(route)) continue;
                        AddTiles(worldMap.routeMapTiles[route], col * spacing, row * spacing);
                    }
                }
            }
        }

        /// <summary>
        /// Add tiles with optional offset to position them correctly
        /// </summary>
        private void AddTiles(Tile[,] tiles, int offsetX = 0, int offsetY = 0)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tiles.Add(new TileViewModel
                    {
                        X = x + offsetX,
                        Y = y + offsetY,
                        Type = tiles[x, y].type,
                        BackgroundID = tiles[x, y].BackgroundID
                    });
                }
            }
        }

        /// <summary>
        /// For testing: generate a simple connected mini-map
        /// </summary>
        public void LoadTestTilesConnected()
        {
            Tiles.Clear();

            // Town1 (1x1)
            AddTiles(CreateTileArray(1, 1, TileType.Grass, 1), 1, 0);
            // Town2 (1x1)
            AddTiles(CreateTileArray(1, 1, TileType.Grass, 2), 5, 0);
            // Town3 (1x1)
            AddTiles(CreateTileArray(1, 1, TileType.Grass, 3), 1, 3);

            // Route1 horizontal: Town1 -> Town2
            AddTiles(CreateTileArray(3, 1, TileType.None, 9), 2, 0);
            // Route2 vertical: Town1 -> Town3
            AddTiles(CreateTileArray(1, 2, TileType.None, 9), 1, 1);
        }

        /// <summary>
        /// Helper to create a rectangular tile array
        /// </summary>
        private Tile[,] CreateTileArray(int width, int height, TileType type, int backgroundID)
        {
            Tile[,] tiles = new Tile[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    tiles[x, y] = new Tile { type = type, BackgroundID = backgroundID };
            return tiles;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
