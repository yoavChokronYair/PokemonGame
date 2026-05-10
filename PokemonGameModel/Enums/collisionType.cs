

namespace PokemonGame.Model.Enums
{
    public enum CollisionType { None = 0, HM = 1,WildGrass = 3, Blocked = 4, JumpLeft = 8,
        JumpRight = 5,
        JumpDown = 6,
        JumpUp = 7
    }
    public enum TileType { Ground,Water,Objects,Above,Cave}
    public enum ConnectionDirection { North, South, East, West }
    public enum FacingDirection { None,Up, Down, Left, Right }
    public enum InspectResultType
    {
        Nothing,
        NpcDialogue,
        ItemPickup,
        HmUsed,
        NeedHm,
    }
    public enum MovementType { Stationary, Walking}
    public enum VisionType { Normal, circular}

}
