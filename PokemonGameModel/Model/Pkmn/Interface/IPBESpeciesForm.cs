using PokemonGame.Model.Domain.Pokemon;

namespace PokemonGame.Core.Model.Pkmn.Interface
{
    public interface IPBESpeciesForm
    {
        PokemonData Species { get; }

        PokemonFormData Form { get; }
    }
}