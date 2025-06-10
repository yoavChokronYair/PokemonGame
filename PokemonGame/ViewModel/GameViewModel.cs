using PokemonGame.Enums;
using PokemonGame.Model;
using PokemonGame.Model.Data;
using PokemonGame.Model.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PokemonGame.ViewModel
{
    public class GameViewModel
    {
        public MapGenaration Map { get; private set; }
        private int currentLeftColIndex = 5;
        private int currentTopRowIndex = 5;

        public ObservableCollection<ObservableCollection<ImageSource>> UiTiles { get; set; }

        public Dictionary<TileType, ImageSource> TileImages = new Dictionary<TileType, ImageSource>
        {
            { TileType.Grass, new BitmapImage(new Uri("pack://application:,,,/images/TallGrass.png")) },
            { TileType.Building, new BitmapImage(new Uri("pack://application:,,,/images/road.png")) },
            { TileType.Fence, new BitmapImage(new Uri("pack://application:,,,/images/road.png")) },
            { TileType.Path, new BitmapImage(new Uri("pack://application:,,,/images/road.png")) },
            { TileType.Tree, new BitmapImage(new Uri("pack://application:,,,/images/road.png")) },
        };


        public GameViewModel(MapData mapData)
        {
            Map = new MapGenaration(mapData);
            UiTiles = new ObservableCollection<ObservableCollection<ImageSource>>();

            int rows = mapData.height - 5;
            int cols = mapData.width - 5;
           
            for (int i = 0; i < rows-5; i++)
            {
                var row = new ObservableCollection<ImageSource>();
                for (int j = 0; j < cols-5; j++)
                {
                    var tile = Map.mapTiles[currentTopRowIndex + i, j];
                    row.Add(TileImages[tile]);
                }
                UiTiles.Add(row);
            }
        }

        public void ShiftTilesDown()
        {
            if (currentTopRowIndex <= 0)
                return;

            currentTopRowIndex--;

            // Remove bottom row
            if (UiTiles.Count > 0)
                UiTiles.RemoveAt(UiTiles.Count - 1);

            // Add a new row at the top
            var newRow = new ObservableCollection<ImageSource>();
            int visibleCols = UiTiles[0].Count;

            for (int j = 0; j < visibleCols; j++)
            {
                int colIndex = currentLeftColIndex + j;
                var tile = Map.mapTiles[currentTopRowIndex, colIndex];
                newRow.Add(TileImages[tile]);
            }

            UiTiles.Insert(0, newRow);
        }

        public void ShiftTilesLeft()
        {
            int mapWidth = Map.mapTiles.GetLength(1);
            if (currentLeftColIndex + UiTiles[0].Count >= mapWidth)
                return;

            currentLeftColIndex++;

            foreach (var row in UiTiles)
            {
                row.RemoveAt(0); // Remove left-most tile
                int rowIndex = UiTiles.IndexOf(row);
                var tile = Map.mapTiles[currentTopRowIndex + rowIndex, currentLeftColIndex + row.Count - 1];
                row.Add(TileImages[tile]);
            }
        }

        public void ShiftTilesRight()
        {
            if (currentLeftColIndex <= 0)
                return;

            currentLeftColIndex--;

            foreach (var row in UiTiles)
            {
                row.RemoveAt(row.Count - 1); // Remove right-most tile
                int rowIndex = UiTiles.IndexOf(row);
                var tile = Map.mapTiles[currentTopRowIndex + rowIndex, currentLeftColIndex];
                row.Insert(0, TileImages[tile]);
            }
        }
        public void ShiftTilesUp()
        {
            int mapHeight = Map.mapTiles.GetLength(0);
            int visibleRows = UiTiles.Count;

            if (currentTopRowIndex + visibleRows >= mapHeight)
                return;

            currentTopRowIndex++;

            // Remove top row
            if (UiTiles.Count > 0)
                UiTiles.RemoveAt(0);

            // Add a new row at the bottom
            var newRow = new ObservableCollection<ImageSource>();
            int visibleCols = UiTiles[0].Count;

            for (int j = 0; j < visibleCols; j++)
            {
                int colIndex = currentLeftColIndex + j;
                var tile = Map.mapTiles[currentTopRowIndex + visibleRows - 1, colIndex];
                newRow.Add(TileImages[tile]);
            }

            UiTiles.Add(newRow);
        }
    }

}
