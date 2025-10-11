using PokemonGame.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Services.Data.MapData
{
    public class WorldData
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public class WorldRegion
        {
            public TileType TileType { get; set; }
            public int ID { get; set; }
            public int StartX { get; set; }
            public int StartY { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
    
}
