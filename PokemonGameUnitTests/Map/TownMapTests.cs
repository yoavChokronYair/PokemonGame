using PokemonGame.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace PokemonGameUnitTests.Map
{
    public class TownMapTest
    {
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        // xUnit will inject this automatically
        public TownMapTest(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TownCreation()
        {
            var pallet = new TownMapData { Name = "PalletTown", connections = new string[] { "ViridianCity", null, null, null } };
            var viridian = new TownMapData { Name = "ViridianCity", connections = new string[] { null, null, "PalletTown", "PewterCity" } };
            var pewter = new TownMapData { Name = "PewterCity", connections = new string[] { null, "ViridianCity", null, null } };

            var list = new TownMapDataList { maps = new List<TownMapData> { pallet, viridian, pewter } };

            var map = new TownMap(list);

            var townMapsField = map.GetType().GetField("townMaps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var townMaps = (TownMapData[,])townMapsField!.GetValue(map)!;

            int rows = townMaps.GetLength(0);
            int cols = townMaps.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                string line = "";
                for (int c = 0; c < cols; c++)
                {
                    line += townMaps[r, c]?.Name?[0] ?? '.';
                    line += " ";
                }
                _output.WriteLine(line);
            }
        }

        [Fact]
        public void TownMap_FillsAllTiles()
        {
            // Arrange: create town data with regions
            var pallet = new TownMapData
            {
                Name = "PalletTown",
                Width = 4,
                Height = 4,
                pathID = 99,
                Regions = new List<MapRegion>
                 {
                    new MapRegion { TileType = TileType.Event,ID = 2, StartX = 2, StartY = 0, Width = 2, Height = 2 },
                    new MapRegion { TileType = TileType.Interactable,ID = 3, StartX = 0, StartY = 2, Width = 2, Height = 2 },
                }
            };

            var list = new TownMapDataList { maps = new List<TownMapData> { pallet } };

            // Act: create TownMap
            var map = new TownMap(list);

            // Access the private dictionary via reflection
            var field = typeof(TownMap).GetField("townMapTiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var townMapTiles = (Dictionary<TownMapData, Tile[,]>)field!.GetValue(map)!;

            var tiles = townMapTiles[pallet];

            // Assert: every tile is filled with either a region ID or pathID
            for (int x = 0; x < pallet.Width; x++)
            {
                for (int y = 0; y < pallet.Height; y++)
                {
                    int tileValue = tiles[x, y].BackgroundID;

                    bool isRegion = pallet.Regions.Exists(r =>
                        x >= r.StartX && x < r.StartX + r.Width &&
                        y >= r.StartY && y < r.StartY + r.Height &&
                        tileValue == r.ID);

                    bool isPathID = tileValue == pallet.pathID;

                    Assert.True(isRegion || isPathID, $"Tile at ({x},{y}) not filled correctly. Value={tileValue}");
                }
            }

            // Optional: print tiles for visual verification
            for (int y = 0; y < pallet.Height; y++)
            {
                string line = "";
                for (int x = 0; x < pallet.Width; x++)
                {
                    line += tiles[x, y].type switch
                    {
                        TileType.None => ".",
                        TileType.Event => "E",
                        TileType.Interactable => "I",
                        _ => "?"
                    };
                    line += tiles[x, y].BackgroundID + " ";
                }
                _output.WriteLine(line);
            }
        }
    }
}

