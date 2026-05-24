namespace PokemonGame.Model.Enums
{
    public enum CollisionType
    {
        None = 0,
        HM = 1,
        CutTree = 2,
        WildGrass = 3,
        Blocked = 4,
        JumpRight = 5,
        JumpDown = 6,
        JumpUp = 7,
        JumpLeft = 8
    }

    public enum TileType
    {
        Ground,
        Water,
        Objects,
        Above,
        Cave,
        Grass
    }

    public enum EncounterMethod
    {
        Grass,
        Surf,
        Fishing,
        Cave
    }

    public enum ConnectionDirection
    {
        North,
        South,
        East,
        West
    }

    public enum FacingDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public enum InspectResultType
    {
        Nothing,
        NpcDialogue,
        ItemPickup,
        HmUsed,
        NeedHm,
    }

    public enum MovementType
    {
        Stationary,
        Walking,
        Wander,
        Random
    }

    public enum VisionType
    {
        Normal,
        Circular
    }
}