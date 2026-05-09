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
        public string Song { get; set; }

        public (int x, int y) FlyWrapLoc { get; set; }
        public (int x, int y) TownMapLoc { get; set; }

        public List<ConnectedMapDomain> ConnectedMaps { get; set; } = new List<ConnectedMapDomain>();
        public List<WrapDomain> Wraps { get; set; } = new List<WrapDomain>();
        public List<EncounterDomain> Encounters { get; set; } = new List<EncounterDomain>();
        public List<NpcObjectDomain> Npc { get; set; } = new List<NpcObjectDomain>();

        /// <summary>
        /// Collision rectangles loaded from DB (replaces tile-ID magic numbers).
        /// Each entry covers one or more tiles and carries a CollisionType.
        /// </summary>
        public List<CollisionObjectDomain> CollisionObjects { get; set; } = new List<CollisionObjectDomain>();
    }

    /// <summary>
    /// A rectangular region of tiles that share the same collision type.
    /// Coordinates are in tile-space (not pixel-space).
    /// </summary>
    public class CollisionObjectDomain
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public CollisionType CollisionType { get; set; }
    }

    public class WrapDomain
    {
        public MapDomain TargetMap { get; set; }
        public (int x, int y) WrapLoc { get; set; }
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
        public NpcSpriteDomain? Sprite { get; set; }
        public (int x, int y) Location { get; set; }
        public CollisionType CollisionType { get; set; } = CollisionType.Blocked;
        public MovementType MovementType { get; set; }

        public FacingDirection direction { get; set; }
        public FacingDirection DirectionA { get; set; }
        public FacingDirection DirectionB { get; set; }
        public int StepsPerLeg { get; set; }
        public int StepsWalked { get; set; }

        public bool DefaultState { get; set; }
        public bool IsDisappearing { get; set; }
        public int visionRange { get; set; } = 0;
        public VisionType VisionType { get; set; }
    }
}