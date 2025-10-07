using PokemonGame.Enums;
using PokemonGame.Model.Helper;
using PokemonGameModel.Model.Data.MapData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PokemonGameModel.Model.Map
{
    public class TownMap
    {
        private readonly HashSet<(TownMapData, TownMapData)> connectedPairs = new();
        private readonly TownMapData[,] townMaps;
        private readonly TownMapDataList towns;
        private readonly Dictionary<TownMapData, Tile[,]> townMapTiles = new Dictionary<TownMapData, Tile[,]>();
        public TownMap(TownMapDataList towns)
        {
            this.towns = towns;
            this.townMaps = new TownMapData[4, 4];
            ArrayHelper.SetCenter2DArray(townMaps, towns.maps[0]);
            foreach (TownMapData town in towns.maps)
            {
                CreateTownConnections(town);
                townMapTiles.Add(town, CreateTownTiles(town));
            }
        }

        private Tile[,] CreateTownTiles(TownMapData townData)
        {
            Tile[,] mapTiles = new Tile[townData.Width, townData.Height];

            for (int x = 0; x < townData.Width; x++)
            {
                for (int y = 0; y < townData.Height; y++)
                {
                    Tile tile = new Tile();
                    tile.BackgroundID = townData.pathID;
                    tile.type = TileType.None;
                    mapTiles[x, y] = tile;
                }
            }
            // Fill regions with their IDs
            if (townData.Regions != null)
            {
                foreach (var region in townData.Regions)
                {
                    int maxX = region.StartX + region.Width;
                    int maxY = region.StartY + region.Height;

                    for (int x = region.StartX; x < maxX; x++)
                    {
                        for (int y = region.StartY; y < maxY; y++)
                        {
                            Tile tile = new Tile();
                            tile.BackgroundID = region.ID;
                            tile.type = region.TileType;
                            mapTiles[x, y] = tile;
                        }
                    }
                }
            }
            return mapTiles;
        }


        public void CreateTownConnections(TownMapData town)
        {
            if (town.connections == null)
                return;

            var townPos = ArrayHelper.FindIn2DArrayIndex(townMaps, t => t == town);
            if (townPos == null)
                return; // Skip if this town isn't placed yet

            int row = townPos.Value.Row;
            int col = townPos.Value.Col;

            Direction direction = Direction.Left;

            foreach (var neighborName in town.connections)
            {
                if (string.IsNullOrEmpty(neighborName))
                {
                    direction = (Direction)((int)direction + 1);
                    continue;
                }

                TownMapData? neighborMap = (TownMapData?)towns.maps.FirstOrDefault(t => t.Name == neighborName);

                if (connectedPairs.Contains((town, neighborMap)) || connectedPairs.Contains((neighborMap, town)))
                {
                    direction = (Direction)((int)direction + 1);
                    continue;
                }

                int newRow = row, newCol = col;

                switch (direction)
                {
                    case Direction.Left: newCol = col - 1; break;
                    case Direction.Right: newCol = col + 1; break;
                    case Direction.Up: newRow = row - 1; break;
                    case Direction.Down: newRow = row + 1; break;
                }

                if (newRow >= 0 && newRow < townMaps.GetLength(0) &&
                    newCol >= 0 && newCol < townMaps.GetLength(1) &&
                    townMaps[newRow, newCol] == null)
                {
                    townMaps[newRow, newCol] = neighborMap;
                }

                direction = (Direction)((int)direction + 1);
                connectedPairs.Add((town, neighborMap));
            }
        }

    }

    public class Tile
    {
        public int BackgroundID;
        public int? LowerOverlayID; // drawn under player
        public int? UpperOverlayID; // drawn over player
        public TileType type;
    }

}
