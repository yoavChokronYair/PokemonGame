using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Core.Model.Pkmn.Interface
{
    public interface IPBESpeciesForm
    {
        PokemonData Species { get; }

        PokemonFormData Form { get; }
    }
}