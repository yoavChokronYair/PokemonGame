// Layer: Interface — contract definition only, no logic or implementations here.
﻿using PokemonGame.Services.Enums.PokemonEnum;
using System.Collections.Generic;

namespace PokemonGame.Interface
{
    public interface ITypeable
    {
        List<PokemonType> Types { get; } 
    }
}
