using PokemonGame.Core.Model.Pkmn;
using PokemonGame.Core.Model.Pkmn.Interface;
using PokemonGame.Enums;
using PokemonGame.Services.GameData;
using PokemonGame.Services.GameData.Pokemon;
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
