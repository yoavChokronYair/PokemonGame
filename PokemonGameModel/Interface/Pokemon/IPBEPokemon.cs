// Layer: Interface — full Pokemon identity contract for the battle engine.
// OOP: extends IPBESpeciesForm with stats, moves, EVs/IVs, and PBE flags.
// Implemented by: PartyPokemon.

using PokemonGame.Enums.PokemonEnum;
using PokemonGame.Interface.Pokemon;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Core.Model.Pkmn;
using PokemonGame.Services.Enums.PokemonEnum;
using PokemonGame.Enums;

namespace PokemonGame.Interface.Pokemon
{
    public interface IPBEPokemon : IPBESpeciesForm
    {
        // Marks the Pokemon to be ignored by the battle engine (eggs, fainted).
        // Won't be sent out, copied by Illusion, or count as a battler.
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
