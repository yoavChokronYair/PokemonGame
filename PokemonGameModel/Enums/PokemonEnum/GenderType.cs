using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Enums.PokemonEnum
{
    public enum GenderType : byte
    {
        /// <summary>The Pokémon is female.</summary>
        Female,
        /// <summary>The Pokémon is genderless.</summary>
        Genderless,
        /// <summary>The Pokémon is male.</summary>
        Male,
        /// <summary>Invalid gender.</summary>
        MAX
    }
}
