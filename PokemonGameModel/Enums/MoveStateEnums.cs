// Move classification enums extracted from MoveDomain.cs.
// Do not redefine these in any other file.
// Used by: MoveDomain, CreateMoves, BattleCalculatorHelper

namespace PokemonGame.Model.Enums
{
    public enum MoveCategory { Physical, Special, Status }
    public enum MoveTarget { Opponent, Self, Both, AllOpponents, AllAllies }
    public enum MoveTag
    {
        Punching,      // Trigger for Iron Fist
        Biting,        // Trigger for Strong Jaw
        Slicing,       // Trigger for Sharpness
        Sound,         // Trigger for Soundproof / Punk Rock
        Powder,        // Trigger for Overcoat / Safety Goggles
        Ballistics,    // Trigger for Bulletproof (Bombs/Balls)
        Pulse,         // Trigger for Mega Launcher
        Contact,       // Trigger for Static / Rough Skin / Rocky Helmet
        Wind           // Trigger for Wind Power / Wind Rider
    }
}
