

namespace PokemonGame.Model.Enums
{
    public enum CollisionType { None = 0, HM = 1, Unwalkable = 2, WildGrass = 3, Blocked = 4, JumpLeft = 8,
        JumpRight = 5,
        JumpDown = 6,
        JumpUp = 7
    }
    public enum TileType { Normal,Water,Branch,TallGrass,Rock,StrengthAble}
    public enum MapTilesType { Club = 0, Overworld = 1 }
    public enum ConnectionDirection { North, South, East, West }
    public enum FacingDirection { None,Up, Down, Left, Right }
    public enum InspectResultType
    {
        Nothing,
        ItemPickup,
        HmUsed,
        NeedHm,
    }
    public enum MovementType { Stationery, Walking}
    public enum VisionType { Normal, circular}

}
