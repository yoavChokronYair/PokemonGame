using PokemonGame.Enums;
using System;

namespace PokemonGame.Model
{
    public class GameMap
    {
        public Tile[,] Tiles { get; private set; }

        public GameMap(string mapData)
        {
            LoadFromString(mapData);
        }

        private void LoadFromString(string mapData)
        {
            var lines = mapData.Trim().Split('\n');
            int rows = lines.Length;
            int cols = lines[0].Trim().Split(',').Length;

            Tiles = new Tile[rows, cols];

            for (int y = 0; y < rows; y++)
            {
                var parts = lines[y].Trim().Split(',');

                for (int x = 0; x < cols; x++)
                {
                    if (int.TryParse(parts[x], out int tileInt) &&
                        Enum.IsDefined(typeof(TileType), tileInt))
                    {
                        Tiles[y, x] = new Tile((TileType)tileInt);
                    }
                    else
                    {
                        Tiles[y, x] = new Tile(TileType.Grass); // Default/fallback
                    }
                }
            }
        }
    }
}



