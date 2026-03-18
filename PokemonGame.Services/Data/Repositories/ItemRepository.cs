using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class ItemRepository
    {
        private readonly IDbConnectionService _db;
        private Dictionary<int, ItemData>? _cache;

        internal ItemRepository(IDbConnectionService db) => _db = db;

        private void EnsureLoaded()
        {
            if (_cache == null)
            {
                // Queries all items once and stores them for O(1) lookups
                var items = _db.Query<ItemData>("SELECT * FROM items");
                _cache = items.ToDictionary(i => i.Id);
            }
        }

        public ItemData? GetItem(int itemID)
        {
            EnsureLoaded();
            return _cache!.TryGetValue(itemID, out var item) ? item : null;
        }

        public List<ItemData> GetAllItems()
        {
            EnsureLoaded();
            return _cache!.Values.ToList();
        }

        // Useful for Shop UIs to filter by category
        public List<ItemData> GetItemsByCategory(string category)
        {
            EnsureLoaded();
            return _cache!.Values.Where(i => i.Category == category).ToList();
        }
    }
}