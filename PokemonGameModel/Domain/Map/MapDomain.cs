using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Map
{
    public class MapDomain
    {
        
        public string Name { get; set; }
        public List<TileDomain> BackgroundBlocks { get; set; }
        public List<TileDomain> Blocks { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TileDomain DefultBlockID { get; set; }
        public string Song { get; set; }

        public int[,] FlyWrapLoc = new int[1, 1];
        public int[,] TownMapLoc = new int[1, 1];
        public MapTilesType TilesType { get; set; }
        public List<ConnectedMapDomain> ConnectedMaps { get; set; } = new();//one per side 
        public List<WrapDomain> Wraps { get; set; } = new(); // for fly/town map/etc
    }
    public class WrapDomain
    {
        public MapDomain TargetMap { get; set; }
        public int WrapLoc { get; set; }
        public int SpawnRow { get; set; }
    }
    public class ConnectedMapDomain
    {
        public MapDomain ConnectedMap { get; set; }
        public ConnectionDirection ConnectionDirection { get; set; }
        public int Margin { get; set; }
    }
}
