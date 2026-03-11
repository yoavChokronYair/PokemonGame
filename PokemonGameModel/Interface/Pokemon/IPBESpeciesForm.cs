// Layer: Interface — identifies a Pokemon's species and form.
// Implemented by: PartyPokemon, BoxPokemon.

using PokemonGame.Model.Domain.Pokemon;

namespace PokemonGame.Interface.Pokemon
{
    public interface IPBESpeciesForm
    {
        PokemonData Species { get; }
        PokemonFormData Form { get; }
    }
}
