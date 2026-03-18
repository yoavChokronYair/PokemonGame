using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.PokemonData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class PokemonStatsRepository : SQLiteRepository<int, PokemonStatsData>
    {
        internal PokemonStatsRepository(IDbConnectionService db) : base(db) { }

        // Get both base stats and EV yields for a specific Pokémon
        public List<PokemonStatsData> GetStatsForPokemon(int pokedexID) =>
            _db.Query<PokemonStatsData>(
                "SELECT * FROM pokemon_stats WHERE pokedexID = @pid",
                new { pid = pokedexID }).ToList();

        // Convenience method: Get specifically Base Stats (isEVYield = 0)
        public PokemonStatsData? GetBaseStats(int pokedexID) =>
            _db.QuerySingle<PokemonStatsData>(
                "SELECT * FROM pokemon_stats WHERE pokedexID = @pid AND isEVYield = 0",
                new { pid = pokedexID });

        // Convenience method: Get specifically EV Yields (isEVYield = 1)
        public PokemonStatsData? GetEVYields(int pokedexID) =>
            _db.QuerySingle<PokemonStatsData>(
                "SELECT * FROM pokemon_stats WHERE pokedexID = @pid AND isEVYield = 1",
                new { pid = pokedexID });

        // Update stats for a specific type (0 for base, 1 for EV)
        public void UpdateStats(PokemonStatsData stats)
        {
            _db.Execute(
                @"UPDATE pokemon_stats 
                  SET hp = @HP, attack = @Attack, defense = @Defense, 
                      spAtk = @SpAtk, spDef = @SpDef, speed = @Speed 
                  WHERE pokedexID = @PokedexID AND isEVYield = @IsEVYield",
                new
                {
                    stats.HP,
                    stats.Attack,
                    stats.Defense,
                    stats.SpAtk,
                    stats.SpDef,
                    stats.Speed,
                    stats.PokedexID,
                    IsEVYield = stats.IsEVYield ? 1 : 0 // SQLite boolean handling
                });
        }
    }
}