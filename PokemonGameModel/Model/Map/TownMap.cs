using PokemonGame.Enums;
using PokemonGame.Model.Helper;
using PokemonGameModel.Model.Data.MapData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokemonGameModel.Model.Map
{
    public class TownMap
    {
        private readonly HashSet<(TownMapData, TownMapData)> connectedPairs = new();
        private readonly TownMapData[,] townMaps;
        public TownMap(TownMapDataList towns)
        {
            this.townMaps = new TownMapData[4, 4];
            ArrayHelper.SetCenter2DArray(townMaps, towns.maps[0]);
            foreach (TownMapData town in towns.maps)
            {
                CreateTownConnections(town);
            }
        }
        public void CreateTownConnections(TownMapData town)
        {
            Direction direction = Direction.Left;
            var townPos = ArrayHelper.FindIn2DArrayIndex(townMaps, t => t == town);
            int row = townPos.Value.Row;
            int col = townPos.Value.Col;
            foreach (var neighbor in town.connections)
            {
                
                TownMapData? neighborMap = GetMapFromName(neighbor);
                // Check both directions (A→B or B→A)
                if (neighborMap == null)
                {
                    continue;
                }

                if (connectedPairs.Contains((neighborMap, town)) || connectedPairs.Contains((town, neighborMap)))
                {
                    continue;
                }

                int newRow = row;
                int newCol = col;

                switch (direction)
                {
                    case Direction.Left:
                        newCol = col - 1;
                        break;
                    case Direction.Right:
                        newCol = col + 1;
                        break;
                    case Direction.Up:
                        newRow = row - 1;
                        break;
                    case Direction.Down:
                        newRow = row + 1;
                        break;
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
        private TownMapData? GetMapFromName(string name)
        {
            return ArrayHelper.FindIn2DArray(townMaps,t => t.Name == name);
        }
        public void PrintTownMap()
        {
            int rows = townMaps.GetLength(0);
            int cols = townMaps.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (townMaps[r, c] != null)
                        Console.Write($"{townMaps[r, c].Name![0]} "); // print first letter
                    else
                        Console.Write(". ");
                }
                Console.WriteLine();
            }
        }


    }
}
