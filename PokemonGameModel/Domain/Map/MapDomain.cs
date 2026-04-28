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

        public (int x, int y) FlyWrapLoc { get; set; }
        public (int x, int y) TownMapLoc { get; set; }
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

    public class NpcObjectDomain
    {
        public NpcDomain NpcInfo { get; set; }
        public NpcSpriteDomain? Sprite { get; set; }  // ← add
        public (int x, int y) Location { get; set; }
        public CollisionType CollisionType { get; set; } = CollisionType.Unwalkable;
        public MovementType MovementType { get; set; }

        // ── Facing ──────────────────────────────────────────────────────────────
        public FacingDirection direction { get; set; }      // current facing (also used for vision)

        // ── Walking ─────────────────────────────────────────────────────────────
        public FacingDirection DirectionA { get; set; }     // first leg  e.g. Up
        public FacingDirection DirectionB { get; set; }     // second leg e.g. Down
        public int StepsPerLeg { get; set; }                // steps before flipping
        public int StepsWalked { get; set; }                // internal counter — don't set manually

        // ── Other ────────────────────────────────────────────────────────────────
        public bool DefaultState { get; set; }
        public bool IsDisappearing { get; set; }
        public int visionRange { get; set; } = 0;
        public VisionType VisionType { get; set; }
    }
}
