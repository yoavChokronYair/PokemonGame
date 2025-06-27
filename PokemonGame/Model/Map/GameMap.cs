using PokemonGame.Enums;
using PokemonGame.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Model.Map
{
    public struct GameMap
    {
        public TileType[][] Tiles { get; set; } // Indexed as Tiles[y][x]
        public static GameMap GenerateGameMapFromRegions(MapData def)
        {
            // Start with an "empty" map
            var tiles = new TileType[def.Height][];
            for (int y = 0; y < def.Height; y++)
            {
                tiles[y] = new TileType[def.Width];
                for (int x = 0; x < def.Width; x++)
                    tiles[y][x] = TileType.Empty;
            }

            // Fill in regions
            foreach (var region in def.Regions)
            {
                if (!Enum.TryParse<TileType>(region.Name, true, out var type))
                    throw new Exception($"Invalid tile type: {region.Name}");

                for (int y = 0; y < region.Height; y++)
                {
                    for (int x = 0; x < region.Width; x++)
                    {
                        int tileX = region.StartX + x;
                        int tileY = region.StartY + y;

                        if (tileX >= 0 && tileX < def.Width && tileY >= 0 && tileY < def.Height)
                        {
                            tiles[tileY][tileX] = type;
                        }
                    }
                }
            }

            return new GameMap { Tiles = tiles };
        }


    }

}
