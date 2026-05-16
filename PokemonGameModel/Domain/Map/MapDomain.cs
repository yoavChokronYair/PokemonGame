using PokemonGame.Model.Domain.Npc;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Map
{
    // ══════════════════════════════════════════════════════════════════════════
    // CANONICAL COORDINATE CONVENTION (fixes Bug #6 medium — undefined semantics)
    //
    //   Tile-space  : (x = tileCol,   y = tileRow)   — matches DB X/Y columns
    //   Square-space: (row = squareRow, col = squareCol)
    //
    // NpcObjectDomain.Location   → tile-space  (x=col, y=row)
    // WrapDomain.WrapLoc         → SQUARE-space (x=squareCol, y=squareRow)
    //   (stored as WrapX/WrapY in DB which are square coords, NOT tile coords)
    // WrapDomain.SpawnLoc        → square-space (row, col) — feeds SquareToTile
    // ConnectedMapDomain.Margin  → SQUARE units (Bug #8 fix — was used as tile
    //   offset in MapState.FindNeighbor, corrected there)
    // ══════════════════════════════════════════════════════════════════════════

    public class MapDomain
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Song { get; set; }

        public (int x, int y) FlyWrapLoc { get; set; }
        public (int x, int y) TownMapLoc { get; set; }

        /// <summary>Visual background tiles — sparse, use X/Y to place.</summary>
        public List<TileDomain> BackgroundBlocks { get; set; } = new List<TileDomain>();

        /// <summary>Visual object/foreground tiles — sparse, use X/Y to place.</summary>
        public List<TileDomain> Blocks { get; set; } = new List<TileDomain>();

        /// <summary>Collision rectangles from Tiled object layers.</summary>
        public List<CollisionObjectDomain> CollisionObjects { get; set; } = new List<CollisionObjectDomain>();

        public List<ConnectedMapDomain> ConnectedMaps { get; set; } = new List<ConnectedMapDomain>();
        public List<WrapDomain> Wraps { get; set; } = new List<WrapDomain>();
        public List<EncounterDomain> Encounters { get; set; } = new List<EncounterDomain>();
        public List<NpcObjectDomain> Npc { get; set; } = new List<NpcObjectDomain>();
    }

    public class CollisionObjectDomain
    {
        /// <summary>Tile-space column.</summary>
        public int X { get; set; }
        /// <summary>Tile-space row.</summary>
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
        public GrowthRateType GrowthRate { get; set; }
    }

    public class NpcObjectDomain
    {
        public NpcDomain NpcInfo { get; set; }

        public (int x, int y) Location { get; set; }

        public CollisionType CollisionType { get; set; } = CollisionType.Blocked;
        public MovementType MovementType { get; set; }

        public FacingDirection Direction { get; set; }
        public FacingDirection? DirectionA { get; set; }
        public FacingDirection? DirectionB { get; set; }

        public int? StepsPerLeg { get; set; }

        public bool DefaultState { get; set; }
        public bool IsDisappearing { get; set; }

        public int VisionRange { get; set; } = 0;
        public VisionType VisionType { get; set; }
    }
}