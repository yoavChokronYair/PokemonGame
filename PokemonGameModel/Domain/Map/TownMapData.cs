// Design: Data Transfer Object — struct-like, properties only, no logic.
// Layer: Domain — maps one SQLite row to an easy-to-use C# object.
﻿namespace PokemonGame.Model.Domain.Map
{
    public class TownMapData : WorldData
    {
        public string Name { get; set; } = "";
        public List<WorldRegion> Regions { get; set; } = new();
        public int[]? connections { get; set; } // Left, Up, Right, Down
        public int pathID { get; set; }
    }
    public class TownMapDataList
    {
        public List<TownMapData> Maps { get; set; } = new();
    }
}