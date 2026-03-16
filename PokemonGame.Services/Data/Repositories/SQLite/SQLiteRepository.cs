using PokemonGame.Services.Data.ConnectionsService;

namespace PokemonGame.Services.Data.Repositories.SQLite
{
    internal abstract class SQLiteRepository<TKey, TValue> where TValue : class
    {
        protected readonly ISQLiteConnectionService _db;
        private readonly Dictionary<TKey, TValue> _cache = new();

        protected SQLiteRepository(ISQLiteConnectionService db) => _db = db;

        protected TValue? GetCached(TKey key, Func<TValue?> fetch)
        {
            if (_cache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var value = fetch();
            if (value != null)
            {
                _cache[key] = value;
            }

            return value;
        }

        protected List<TValue> GetAllCached(Func<List<TValue>> fetch, Func<TValue, TKey> keySelector)
        {
            if (_cache.Count > 0)
            {
                return new List<TValue>(_cache.Values);
            }

            var all = fetch();
            foreach (var item in all)
            {
                _cache[keySelector(item)] = item;
            }

            return all;
        }

        protected TValue StoreAndReturn(TKey key, Func<TValue> fetch)
        {
            var value = fetch();
            _cache[key] = value;
            return value;
        }

        protected bool ExistsCached(TKey key, Func<bool> fetch)
            => _cache.ContainsKey(key) || fetch();
    }
}
