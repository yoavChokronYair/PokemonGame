namespace PokemonGame.Model.Enums
{
    public enum PokemonType
    {
        Normal,
        Fire,
        Water,
        Grass,
        Electric,
        Ice,
        Fighting,
        Poison,
        Ground,
        Flying,
        Psychic,
        Bug,    
        Rock,
        Ghost,
        Dragon,
        Dark,
        Steel,
        Fairy,
        None
    }
    public enum  Gender
    {
        Male,
        Female,
        Genderless
    }
    public enum EvoTriggerType
    {
        LevelUp,
        UseItem,
        Trade,
        Friendship,
        TimeOfDay,
        Location,
        MoveLearned
    }
    public enum GrowthRateType
    {
        Erratic,        // Fastest early, slowest late
        Fast,
        MediumFast,     // Standard cubic: n^3
        MediumSlow,
        Slow,
        Fluctuating     // Slowest early, fastest late
    }

}
