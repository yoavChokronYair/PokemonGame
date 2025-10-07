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
    public static class TownMapTest
    {
        public static void Run()
        {
            // === Create simple test towns ===
            var pallet = new TownMapData
            {
                Name = "PalletTown",
                connections = new string[] { "ViridianCity", null, null, null } // Left connection only
            };

            var viridian = new TownMapData
            {
                Name = "ViridianCity",
                connections = new string[] { null, null, "PalletTown", "PewterCity" } // Right = PalletTown, Down = PewterCity
            };

            var pewter = new TownMapData
            {
                Name = "PewterCity",
                connections = new string[] { null, "ViridianCity", null, null } // Up = ViridianCity
            };

            var list = new TownMapDataList
            {
                maps = new List<TownMapData> { pallet, viridian, pewter }
            };

            // === Create and print map ===
            var map = new TownMap(list);
            map.PrintTownMap(); // <-- You’ll add this inside TownMap class
        }
    }
}
