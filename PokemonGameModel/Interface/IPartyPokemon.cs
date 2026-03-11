// Layer: Interface — contract definition only, no logic or implementations here.
﻿using PokemonGame.Core.Model.Pkmn;
using PokemonGame.Enums;
using PokemonGame.Interface.Pokemon;
using PokemonGame.Services.Enums.PokemonEnum;
using System.Collections.Generic;

namespace PokemonGame.Interface
{
    public interface IPartyPokemon : IPBEPokemon, IPBESpeciesForm
    {
        ushort HP { get; }

        StatusType Status1 { get; }

        byte SleepTurns { get; }

        new Moveset Moveset { get; }
    }
}
