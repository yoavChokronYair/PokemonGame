// Design: Data Transfer Object — struct-like, properties only, no logic.
// Layer: Domain — maps one SQLite row to an easy-to-use C# object.
﻿using PokemonGame.Enums;

namespace PokemonGame.Model.Domain.Map
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