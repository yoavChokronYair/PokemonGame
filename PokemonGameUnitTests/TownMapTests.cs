using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace PokemonGameUnitTests
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
        public void Run()
        {
            var pallet = new TownMapData { Name = "PalletTown", connections = new string[] { "ViridianCity", null, null, null } };
            var viridian = new TownMapData { Name = "ViridianCity", connections = new string[] { null, null, "PalletTown", "PewterCity" } };
            var pewter = new TownMapData { Name = "PewterCity", connections = new string[] { null, "ViridianCity", null, null } };

            var list = new TownMapDataList { maps = new List<TownMapData> { pallet, viridian, pewter } };

            var map = new TownMap(list);

            // Instead of Console.WriteLine, use _output.WriteLine
            int rows = 4;
            int cols = 4;

            for (int r = 0; r < rows; r++)
            {
                string line = "";
                for (int c = 0; c < cols; c++)
                {
                    if (map.GetType().GetField("townMaps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .GetValue(map) is TownMapData[,] townMaps)
                    {
                        if (townMaps[r, c] != null)
                            line += townMaps[r, c].Name![0] + " ";
                        else
                            line += ". ";
                    }
                }
                _output.WriteLine(line);
            }
        }
    }
}
