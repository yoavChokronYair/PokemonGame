using System;
using System.Collections.Generic;

namespace PokemonGame.Services.DataProvider
{
    public abstract class GameDataProvider
    {
        public static GameDataProvider Instance { get; private set; } = null!;

        protected GameDataProvider()
        {
            Instance = this;
        }

        #region --- Generic Registries ---

        // Single-record loaders: keyed by (TValue type, TKey type, column name)
        private readonly Dictionary<(Type valueType, Type keyType, string keyColumn), object> _loaders
            = new();

        // "GetAll" loaders: keyed by TValue type
        private readonly Dictionary<Type, object> _allLoaders = new();

        // Cache: keyed by (TValue type, key value, column name)
        private readonly Dictionary<(Type valueType, object key, string keyColumn), object> _cache
            = new();

        #endregion

        #region --- Registration Methods ---

        /// <summary>
        /// Registers a single-record loader for a given data type and key column.
        /// </summary>
        protected void Register<TKey, TValue>(string keyColumn, Func<TKey, TValue> loader)
        {
            _loaders[(typeof(TValue), typeof(TKey), keyColumn)] = loader;
        }

        /// <summary>
        /// Registers a bulk loader (GetAll) for a given data type.
        /// </summary>
        protected void RegisterAll<TValue>(Func<List<TValue>> loader)
        {
            _allLoaders[typeof(TValue)] = loader;
        }
        protected void RegisterAllLoader<TValue>(Func<List<TValue>> loader)
        {
            _allLoaders[typeof(TValue)] = loader;
        }

        #endregion

        #region --- Generic Getters ---

        /// <summary>
        /// Gets a single data object by key (e.g., Pokémon by ID, Move by Name).
        /// Uses cache if enabled.
        /// </summary>
        public TValue Get<TValue, TKey>(TKey key, string keyColumn, bool useCache = true)
        {
            var cacheKey = (typeof(TValue), (object)key!, keyColumn);

            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return (TValue)cached;

            if (!_loaders.TryGetValue((typeof(TValue), typeof(TKey), keyColumn), out var loaderObj))
                throw new InvalidOperationException($"No loader registered for {typeof(TValue).Name} with key column '{keyColumn}'");

            var loader = (Func<TKey, TValue>)loaderObj;
            var value = loader(key);

            if (useCache)
                _cache[cacheKey] = value;

            return value;
        }

        /// <summary>
        /// Gets all entries of a registered data type.
        /// </summary>
        public List<TValue> GetAll<TValue>()
        {
            if (_allLoaders.TryGetValue(typeof(TValue), out var loaderObj))
            {
                var loader = (Func<List<TValue>>)loaderObj;
                return loader();
            }

            throw new InvalidOperationException($"No 'GetAll' loader registered for {typeof(TValue).Name}");
        }

        /// <summary>
        /// Clears all cached entries.
        /// </summary>
        public void ClearCache() => _cache.Clear();

        #endregion
    }
}
