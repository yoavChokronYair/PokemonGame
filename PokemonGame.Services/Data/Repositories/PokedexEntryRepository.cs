using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.PokemonData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class PokedexEntryRepository
    {
        private readonly IDbConnectionService _db;
        private Dictionary<int, PokedexEntryData>? _cache;

        internal PokedexEntryRepository(IDbConnectionService db) => _db = db;

        private void EnsureLoaded()
        {
            if (_cache == null)
            {
                var entries = _db.Query<PokedexEntryData>("SELECT * FROM pokedex_data");
                _cache = entries.ToDictionary(e => e.PokedexID);
            }
        }

        // Retrieve full entry for a species
        public PokedexEntryData? GetEntry(int pokedexID)
        {
            EnsureLoaded();
            return _cache!.TryGetValue(pokedexID, out var entry) ? entry : null;
        }

        // Helper to get evolution chain as a list of IDs
        public List<int> GetEvolutionChain(int pokedexID)
        {
            EnsureLoaded();
            // This assumes your strings are stored as "1,2,3"
            var entry = GetEntry(pokedexID);
            if (entry == null || string.IsNullOrEmpty(entry.NextEvolution))
            {
                return new List<int> { pokedexID };
            }

            return entry.NextEvolution.Split(',').Select(int.Parse).ToList();
        }
    }
}