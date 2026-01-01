using PokemonGame.Enums;
using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Services.Data.GameData;

namespace PokemonGame.Core.Model.Pkmn.Interface
{
    public interface IPBEPokemon : IPBESpeciesForm
    {
        //
        // Summary:
        //     This marks the Pokémon to be ignored by the battle engine. The Pokémon will be
        //     treated like an egg or fainted Pokémon. Therefore, it won't be sent out, copied
        //     with Kermalis.PokemonBattleEngine.Data.PBEAbility.Illusion, or count as a battler
        //     if the rest of the team faints.
        bool PBEIgnore { get; }

        GenderType Gender { get; }

        string Nickname { get; }

        bool Shiny { get; }

        byte Level { get; }

        uint EXP { get; }

        bool Pokerus { get; }

        string Item { get; }

        byte Friendship { get; }

        AbilityData Ability { get; }

        NatureType Nature { get; }

        string CaughtBall { get; }

        IPBEStatCollection EffortValues { get; }

        IPBEReadOnlyStatCollection IndividualValues { get; }

        Moveset Moveset { get; }
    }
}
