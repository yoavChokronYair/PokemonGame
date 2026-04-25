using PokemonGame.Model.Domain.Npc;
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
        public List<NpcObjectDomain> Npc { get; set; } = new();
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
    public class HiddenItemsDomain
    {
        public string Name { get; set; }
        public CollisionType CollisionType { get; set; } = CollisionType.Unwalkable;
        public string Description { get; set; }
        public (int x, int y) Location { get; set; }
        public bool DefaultState { get; set; }
        public bool IsPickedUp { get; set; } = false;

        public bool IsVisible => DefaultState && !IsPickedUp;
        public bool IsBlocking => IsVisible; // blocks movement only while still there

    }
    public class NpcObjectDomain
    {
        public NpcDomain NpcInfo { get; set; }
        public (int x, int y) Location { get; set; }
        public CollisionType CollisionType { get; set; } = CollisionType.Unwalkable;
        public MovementType movementType { get; set; }
        public FacingDirection direction { get; set; }
        public bool DefaultState { get; set; }
        public bool IsDisappearing { get; set; }
        public int visionRange { get; set; } = 0; // for trainer npcs, how far they can see the player to trigger battle
        public VisionType VisionType { get; set; }
    }
}
