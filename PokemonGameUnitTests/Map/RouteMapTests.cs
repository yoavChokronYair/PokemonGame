using PokemonGame.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGameUnitTests.Map
{
    public class RouteMapTests
    {
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        // xUnit will inject this automatically
        public RouteMapTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void CreateRouteTiles_ShouldInitializeTilesCorrectly_AndPrintMap()
        {
            // Arrange
            var routeData = new RouteMapData
            {
                ID = 1,
                Width = 7,
                Height = 5,
                pathID = 0, // Path represented by '-'
                Regions = new List<MapRegion>
            {
                new MapRegion
                {
                    ID = 1,
                    TileType = TileType.Event,
                    StartX = 2,
                    StartY = 1,
                    Width = 3,
                    Height = 3
                }
            }
            };

            var routeList = new RouteMapDataList
            {
                maps = new List<RouteMapData> { routeData }
            };

            // Act
            var routeMap = new RouteMap(routeList);

            // Access private dictionary
            var field = typeof(RouteMap).GetField("routeMapTiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tilesDict = (Dictionary<RouteMapData, Tile[,]>)field.GetValue(routeMap);
            var tiles = tilesDict[routeData];

            // Assert & Output: Print map using '-' for path and '|' for region
            for (int y = 0; y < routeData.Height; y++)
            {
                string line = ""; // <-- build the line
                for (int x = 0; x < routeData.Width; x++)
                {
                    Tile tile = tiles[x, y];
                    line += tile.type == TileType.None ? "-" : "|";
                }
                _output.WriteLine(line); // write the built line
            }
        }
    }
    
}
