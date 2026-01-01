using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Enums.MovesEnum
{
    public enum MoveTargetType : byte
    {
        All,                   // Every battler (Ex. Perish Song)
        AllFoes,               // Every foe (Ex. Stealth Rock)
        AllFoesSurrounding,    // All foes surrounding (Ex. Growl)
        AllSurrounding,        // All battlers surrounding (Ex. Earthquake)
        AllTeam,               // User's entire team (Ex. Light Screen)
        RandomFoeSurrounding,  // Randomly picks a surrounding foe (Ex. Outrage)
        Self,                  // Self (Ex. Growth)
        SelfOrAllySurrounding, // Self or adjacent ally (Ex. Acupressure)
        SingleAllySurrounding, // Adjacent ally (Ex. Helping Hand)
        SingleFoeSurrounding,  // Single foe surrounding (Ex. Me First)
        SingleNotSelf,         // Single battler except itself (Ex. Dark Pulse)
        SingleSurrounding,     // Single battler surrounding (Ex. Tackle)
        Varies                 // Possible targets vary (Ex. Curse)
    }
}

