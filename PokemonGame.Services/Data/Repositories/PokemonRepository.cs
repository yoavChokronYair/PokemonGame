using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.Data.Repositories
{
    internal class PokemonRepository : DbRepository<int, PokemonGeneral>
    {
        internal PokemonRepository(IDbConnectionService db) : base(db) { }

        // Fetch a single Pokemon's master data by Pokedex ID
        public PokemonGeneral? GetPokemonById(int pokedexID) =>
            _db.QuerySingle<PokemonGeneral>(
                "SELECT * FROM pokemon_general WHERE pokedexID = @pid",
                new { pid = pokedexID });

        // Fetch all Pokemon for your Pokedex UI
        public List<PokemonGeneral> GetAllPokemon() =>
            _db.Query<PokemonGeneral>("SELECT * FROM pokemon_general ORDER BY pokedexID ASC").ToList();

        // Search Pokemon by name
        public PokemonGeneral? GetPokemonByName(string name) =>
            _db.QuerySingle<PokemonGeneral>(
                "SELECT * FROM pokemon_general WHERE name = @name",
                new { name });

        // Get evolution chain information for a specific Pokemon
        public List<PokemonGeneral> GetEvolutionLine(int evoID) =>
            _db.Query<PokemonGeneral>(
                "SELECT * FROM pokemon_general WHERE pokemonEvoID = @evoID",
                new { evoID }).ToList();
    }
}