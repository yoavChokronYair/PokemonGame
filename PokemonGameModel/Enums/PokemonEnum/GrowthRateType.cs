using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Services.Enums.PokemonEnum
{
    public enum GrowthRateType:byte
    {
        Erratic = 1,
        Fast = 4,
        Fluctuating = 2,
        MediumFast = 0,
        MediumSlow = 3,
        Slow = 5,
        MAX = 6 // 6 & 7 in-game are clones of MediumFast, but no Pokémon uses 6 or 7
    }
}
