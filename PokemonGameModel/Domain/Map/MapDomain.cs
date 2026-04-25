using PokemonGame.Model.Domain.Pokemon;
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
        public List<EncounterDomain> Encounters { get; set; } = new();
    }
    public class WrapDomain
    {
        public MapDomain TargetMap { get; set; }
        public (int x,int y) WrapLoc { get; set; }
        public (int row, int col) SpawnLoc { get; set; } 
    }
    public class ConnectedMapDomain
    {
        public MapDomain ConnectedMap { get; set; }
        public ConnectionDirection ConnectionDirection { get; set; }
        public int Margin { get; set; }
    }
    public class EncounterDomain
    {
        public PokemonState Pokemon { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public int CatchChance { get; set; }
        public int Rate { get; set; }
        public (Stat stat, int amount)? evYield { get; set; }
        public int BaseExpYield { get; set; }
        public int BaseFriendshipYield { get; set; }
        public int CatchRate { get; set; }
        public int femaleRatio { get; set; }
    }
}
