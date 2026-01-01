using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Enums.MovesEnum
{
    public enum MovesCategoryType:byte
    {
        Status,
        /// <summary>The move deals physical damage using the Attack and Defense stats.</summary>
        Physical,
        /// <summary>The move deals special damage using the Special Attack and Special Defense stats.</summary>
        Special,
        /// <summary>Invalid category.</summary>
        MAX

    }
}
