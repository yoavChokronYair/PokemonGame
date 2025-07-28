namespace PokemonGameModel.Enums
{
    public enum StatusType
    {
        // Primary status conditions (only one can be active at a time)
        None,
        Burn,       // BRN
        Freeze,     // FRZ
        Paralysis,  // PAR
        Poison,     // PSN
        Sleep,      // SLP
        
        //ToDo:replace to a different place
        // Volatile status conditions (can stack with primary)
        Confusion,
        Infatuation,
        Flinch,
        Curse,          // Ghost-type curse effect
        Nightmare,      // Takes damage while asleep
        LeechSeed,
        Bound,          // Trapped (e.g., by Wrap, Whirlpool)
        Taunt,
        Torment,
        Disable,
        Encore,
        HealBlock,
        Yawn,           // Will fall asleep next turn
        PerishSong,     // Will faint in 3 turns
        Embargo,
        AquaRing,       // Heals each turn
        Ingrain,        // Heals each turn and prevents switching
        SmackDown,      // Loses Flying-type immunity
        Telekinesis,    // Becomes easier to hit
        MagnetRise,     // Gains Ground immunity
        Substitute,
        FocusEnergy     // Increased critical hit chance
    }

}
