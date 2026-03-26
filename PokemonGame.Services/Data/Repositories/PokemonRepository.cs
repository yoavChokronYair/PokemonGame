using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.Data.Repositories
{
    internal class PokemonRepository : DbRepository<int, PokemonGeneral>
    {
        internal PokemonRepository(IDbConnectionService db) : base(db) { }

        public PokemonGeneral? GetPokemonById(int pokedexID) =>
            _db.QuerySingle<PokemonGeneral>(
                "SELECT * FROM pokemon_general WHERE pokedexID = @pid",
                new { pid = pokedexID });

        public List<PokemonGeneral> GetAllPokemon() =>
            _db.Query<PokemonGeneral>("SELECT * FROM pokemon_general ORDER BY pokedexID ASC").ToList();

        public PokemonGeneral? GetPokemonByName(string name) =>
            _db.QuerySingle<PokemonGeneral>(
                "SELECT * FROM pokemon_general WHERE name = @name",
                new { name });

        public List<PokemonGeneral> GetEvolutionLine(int evoID) =>
            _db.Query<PokemonGeneral>(
                "SELECT * FROM pokemon_general WHERE pokemonEvoID = @evoID",
                new { evoID }).ToList();

        // ── Battle loading ────────────────────────────────────────────────────
        public List<int> GetAllPokedexIds() =>
            _db.QueryScalarList<int>("SELECT pokedexID FROM pokemon_general");

    }
}