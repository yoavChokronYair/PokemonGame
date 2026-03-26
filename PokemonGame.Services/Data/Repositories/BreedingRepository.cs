using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.PokemonData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class BreedingRepository
    {
        private readonly IDbConnectionService _db;
        private Dictionary<int, BreedingData>? _cache;

        internal BreedingRepository(IDbConnectionService db) => _db = db;

        private void EnsureLoaded()
        {
            if (_cache == null)
            {
                var data = _db.Query<BreedingData>("SELECT * FROM breeding");
                _cache = data.ToDictionary(b => b.PokedexID);
            }
        }

        // Get breeding stats for a specific species
        public BreedingData? GetBreedingStats(int pokedexID)
        {
            EnsureLoaded();
            return _cache!.TryGetValue(pokedexID, out var data) ? data : null;
        }

        // Check if two Pokemon share an egg group (Essential for Daycare logic)
        public bool AreCompatible(int p1PokedexID, int p2PokedexID)
        {
            EnsureLoaded();
            var b1 = GetBreedingStats(p1PokedexID);
            var b2 = GetBreedingStats(p2PokedexID);

            if (b1 == null || b2 == null || b1.EggGroup == null || b2.EggGroup == null)
            {
                return false;
            }

            return b1.EggGroup == b2.EggGroup;
        }
    }
}