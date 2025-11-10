using PokemonGame.Services.Enums.PokemonEnum;
namespace PokemonGame.Services.Data.Pokemon
{
    public sealed class PokemonData 
    {
        private int pokemonID;
        private string speciesName;

        public int PokemonID { get => pokemonID; set => pokemonID = value; }
        public string SpeciesName { get => speciesName; set => speciesName = value; }
    }
}
