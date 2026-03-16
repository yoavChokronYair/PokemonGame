using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.Data.Repositories.SQLite
{
    // Inherits base cache for PokemonData (primary type, keyed by int).
    // All other Pokemon sub-types share the same int key so each gets its own dictionary.
    internal class SQLitePokemonRepository : SQLiteRepository<int, PokemonData>
    {
        private readonly Dictionary<int, PokemonFormData> _formCache = new();
        private readonly Dictionary<int, BaseStatsData> _baseStatsCache = new();
        private readonly Dictionary<int, EvolutionData> _evolutionCache = new();
        private readonly Dictionary<int, EggMoveData> _eggMoveCache = new();
        private readonly Dictionary<int, LevelUpMoveData> _levelUpCache = new();

        internal SQLitePokemonRepository(ISQLiteConnectionService db) : base(db) { }

        public PokemonData LoadPokemonData(int id) =>
            GetCached(id, () => _db.QuerySingle<PokemonData>("SELECT * FROM Pokemon WHERE PokemonID = @id", new { id }));

        public List<PokemonData> GetAllPokemon() =>
            GetAllCached(() => _db.Query<PokemonData>("SELECT * FROM Pokemon").ToList(), p => p.PokemonID);

        public PokemonFormData LoadFormData(int id) =>
            GetOrSet(_formCache, id, () => _db.QuerySingle<PokemonFormData>("SELECT * FROM PokemonForm WHERE PokemonID = @id", new { id }));

        public List<PokemonFormData> GetAllFormData() =>
            GetOrSetAll(_formCache, () => _db.Query<PokemonFormData>("SELECT * FROM PokemonForm").ToList(), f => f.PokemonID);

        public BaseStatsData LoadBaseStatsData(int id) =>
            GetOrSet(_baseStatsCache, id, () => _db.QuerySingle<BaseStatsData>("SELECT * FROM BaseStats WHERE PokemonID = @id", new { id }));

        public List<BaseStatsData> GetAllBaseStats() =>
            GetOrSetAll(_baseStatsCache, () => _db.Query<BaseStatsData>("SELECT * FROM BaseStats").ToList(), s => s.PokemonID);

        public EvolutionData LoadEvolutionData(int id) =>
            GetOrSet(_evolutionCache, id, () => _db.QuerySingle<EvolutionData>("SELECT * FROM Evolution WHERE PokemonID = @id", new { id }));

        public List<EvolutionData> GetAllEvolution() =>
            GetOrSetAll(_evolutionCache, () => _db.Query<EvolutionData>("SELECT * FROM Evolution").ToList(), e => e.PokemonID);

        public EggMoveData LoadEggMovesData(int id) =>
            GetOrSet(_eggMoveCache, id, () => _db.QuerySingle<EggMoveData>("SELECT * FROM EggMove WHERE PokemonID = @id", new { id }));

        public List<EggMoveData> GetAllEggMoves() =>
            GetOrSetAll(_eggMoveCache, () => _db.Query<EggMoveData>("SELECT * FROM EggMove").ToList(), e => e.PokemonID);

        public LevelUpMoveData LoadLevelUpMovesData(int id) =>
            GetOrSet(_levelUpCache, id, () => _db.QuerySingle<LevelUpMoveData>("SELECT * FROM LevelUpMove WHERE PokemonID = @id", new { id }));

        public List<LevelUpMoveData> GetAllLevelUpMoves() =>
            GetOrSetAll(_levelUpCache, () => _db.Query<LevelUpMoveData>("SELECT * FROM LevelUpMove").ToList(), l => l.PokemonID);

        // Small helpers so the extra dictionaries read the same as the base class methods
        private static T GetOrSet<T>(Dictionary<int, T> cache, int key, Func<T> fetch) where T : class
        {
            if (cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var value = fetch();
            cache[key] = value;
            return value;
        }

        private static List<T> GetOrSetAll<T>(Dictionary<int, T> cache, Func<List<T>> fetch, Func<T, int> keySelector)
        {
            if (cache.Count > 0)
            {
                return new List<T>(cache.Values);
            }

            var all = fetch();
            foreach (var item in all)
            {
                cache[keySelector(item)] = item;
            }

            return all;
        }
    }
}