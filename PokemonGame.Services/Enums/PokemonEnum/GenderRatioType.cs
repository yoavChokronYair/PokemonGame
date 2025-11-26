using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Enums.PokemonEnum
{
    public enum GenderRatioType:byte
    {
        /// <summary>The species is 87.5% male, 12.5% female.</summary>
        M7_F1 = 0x1F,
        /// <summary>The species is 75% male, 25% female.</summary>
        M3_F1 = 0x3F,
        /// <summary>The species is 50% male, 50% female.</summary>
        M1_F1 = 0x7F,
        /// <summary>The species is 25% male, 75% female.</summary>
        M1_F3 = 0xBF,
        /// <summary>The species is 0% male, 100% female.</summary>
        M0_F1 = 0xFE,
        /// <summary>The species is genderless.</summary>
        M0_F0 = 0xFF,
        /// <summary>The species is 100% male, 0% female.</summary>
        M1_F0 = 0x00
    }
}
