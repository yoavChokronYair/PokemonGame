namespace PokemonGame.Model.Domain.Map
{
    public class RouteMapData : WorldData
    {
        public int ID { get; set; }
        public List<WorldRegion> Regions { get; set; } = new();
        public List<Encounter> Encounters { get; set; } = new();
        public int[]? TownConnections { get; set; } // first town, second town
        public int pathID { get; set; }
    }
    public class RouteMapDataList
    {
        public List<RouteMapData> Maps { get; set; } = new();
    }
    public class Encounter
    {
        public string Name { get; set; } = "";
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public double Rarity { get; set; }
        public string Environment { get; set; } = "";
    }
}