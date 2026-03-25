using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.PokemonData;

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

        public PokemonStatsData? GetStatsById(int pokedexID) =>
            _db.QuerySingle<PokemonStatsData>(
                // Table name changed from base_stats to pokemon_stats
                // added WHERE clause for isEVYield = 0 to get base values
                "SELECT * FROM pokemon_stats WHERE pokedexID = @pid AND isEVYield = 0",
                new { pid = pokedexID });

        public string? GetMoveName(int moveId) =>
            _db.QueryScalar<string>(
                "SELECT name FROM moves WHERE id = @mid",
                new { mid = moveId });
        public List<int> GetAllPokedexIds() =>
            _db.QueryScalarList<int>("SELECT pokedexID FROM pokemon_general");

          // 2. Get a random legal move for a specific Pokemon
        public int GetRandomMoveIdForPokemon(int pokedexId) =>
            _db.QueryScalar<int>(
                "SELECT moveID FROM levelup_moves WHERE pokedexID = @pid ORDER BY RANDOM() LIMIT 1",
                new { pid = pokedexId });
    }
}