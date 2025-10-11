using PokemonGame.Model.Map;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace PokemonGameUnitTests.Map
{
    public class WorldMapTests
    {
        private readonly ITestOutputHelper _output;

        public WorldMapTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestWorldMapWithRoutesOnly()
        {
            // Sample towns
            var town1 = new TownMapData { Name = "Town1", Width = 1, Height = 1, pathID = 1 };
            var town2 = new TownMapData { Name = "Town2", Width = 1, Height = 1, pathID = 2 };
            var town3 = new TownMapData { Name = "Town3", Width = 1, Height = 1, pathID = 3 };
            var towns = new TownMapDataList { maps = new List<TownMapData> { town1, town2, town3 } };

            // Sample routes
            var route1 = new RouteMapData { ID = 1, Width = 3, Height = 1, pathID = 9 }; // town1 -> town2
            var route2 = new RouteMapData { ID = 2, Width = 1, Height = 2, pathID = 9 }; // town1 -> town3
            var routes = new RouteMapDataList { maps = new List<RouteMapData> { route1, route2 } };

            // Create the world map
            var worldMap = new WorldMap(towns, routes);

            int worldWidth = 7;
            int worldHeight = 5;
            char[,] worldGrid = new char[worldWidth, worldHeight];

            // Fill empty space
            for (int x = 0; x < worldWidth; x++)
                for (int j = 0; j < worldHeight; j++)
                    worldGrid[x, j] = ' ';

            // Place towns
            var townPositions = new Dictionary<TownMapData, (int X, int Y)>
            {
                { town1, (1, 0) },
                { town2, (5, 0) },
                { town3, (1, 3) }
            };

            foreach (var kvp in townPositions)
                worldGrid[kvp.Value.X, kvp.Value.Y] = 't';

            // Draw route1 horizontally between town1 and town2
            var route1Tiles = worldMap.routeMapTiles[route1];
            int startX = townPositions[town1].X + 1;
            int y = townPositions[town1].Y;
            for (int x = 0; x < route1.Width; x++)
            {
                int worldX = startX + x;
                if (worldX < worldWidth)
                    worldGrid[worldX, y] = '-';
            }

            // Draw route2 vertically between town1 and town3
            var route2Tiles = worldMap.routeMapTiles[route2];
            int startY = townPositions[town1].Y + 1;
            int xPos = townPositions[town1].X;
            for (int yPos = 0; yPos < route2.Height; yPos++)
            {
                int worldY = startY + yPos;
                if (worldY < worldHeight)
                    worldGrid[xPos, worldY] = '|';
            }

            // Print the grid
            for (int row = 0; row < worldHeight; row++)
            {
                string line = "";
                for (int col = 0; col < worldWidth; col++)
                    line += worldGrid[col, row];
                _output.WriteLine(line);
            }
            PrintTileArrayByCommand(worldMap, "r1");
            PrintTileArrayByCommand(worldMap, "r2");
            PrintTileArrayByCommand(worldMap, "t1");
            PrintTileArrayByCommand(worldMap, "t2");
        }

        private void PrintTileArrayByCommand(WorldMap worldMap, string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            command = command.ToLower();

            if (command.StartsWith("r")) // Route
            {
                if (int.TryParse(command.Substring(1), out int routeId))
                {
                    var route = worldMap.routeMapTiles.Keys.FirstOrDefault(r => r.ID == routeId);
                    if (route != null)
                    {
                        var tiles = worldMap.routeMapTiles[route];
                        _output.WriteLine($"--- Route {routeId} ({route.Width}x{route.Height}) ---");
                        PrintTiles(tiles);
                        return;
                    }
                }
            }

            if (command.StartsWith("t")) // Town
            {
                if (int.TryParse(command.Substring(1), out int townIndex))
                {
                    var towns = worldMap.townMapTiles.Keys.ToList();
                    if (townIndex - 1 >= 0 && townIndex - 1 < towns.Count)
                    {
                        var town = towns[townIndex - 1];
                        var tiles = worldMap.townMapTiles[town];
                        _output.WriteLine($"--- Town {town.Name} ({town.Width}x{town.Height}) ---");
                        PrintTiles(tiles);
                        return;
                    }
                }
            }

            _output.WriteLine($"Unknown command: {command}");
        }

        private void PrintTiles(Tile[,] tiles)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                string line = "";
                for (int x = 0; x < width; x++)
                {
                    line += tiles[x, y].BackgroundID.ToString().PadLeft(2, '0') + " ";
                }
                _output.WriteLine(line);
            }
        }

    }
}

