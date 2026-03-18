using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.PokemonData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class AbilityRepository
    {
        private readonly IDbConnectionService _db;
        private Dictionary<int, AbilityData>? _cache;

        internal AbilityRepository(IDbConnectionService db) => _db = db;

        // Ensures the database is read into memory only once
        private void EnsureLoaded()
        {
            if (_cache == null)
            {
                var allAbilities = _db.Query<AbilityData>("SELECT * FROM abilities");
                _cache = allAbilities.ToDictionary(a => a.Id);
            }
        }

        // Get an ability by its ID from memory
        public AbilityData? GetAbility(int abilityID)
        {
            EnsureLoaded();
            return _cache!.TryGetValue(abilityID, out var ability) ? ability : null;
        }

        // Get all abilities (useful for admin tools or UI pickers)
        public List<AbilityData> GetAllAbilities()
        {
            EnsureLoaded();
            return _cache!.Values.ToList();
        }
    }
}