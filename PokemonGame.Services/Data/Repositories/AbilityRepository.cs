using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.GameData.PokemonData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class AbilityRepository : DbRepository<int, AbilityData>
    {
        private readonly Dictionary<int, ConditionRow> _conditionCache = new();
        private readonly Dictionary<int, EffectRow> _effectCache = new();
        private readonly Dictionary<int, NumberData> _numberCache = new();

        internal AbilityRepository(IDbConnectionService db) : base(db) { }

        // ─── AbilityData ────────────────────────────────────────────────────────

        public AbilityData? GetAbilityById(int id) =>
            _db.QuerySingle<AbilityData>(
                "SELECT * FROM abilities WHERE id = @id",
                new { id });

        public AbilityData? GetAbilityByName(string name) =>
            _db.QuerySingle<AbilityData>(
                "SELECT * FROM abilities WHERE name = @name",
                new { name });

        public List<AbilityData> GetAllAbilities() =>
            _db.Query<AbilityData>("SELECT * FROM abilities ORDER BY id ASC").ToList();

        public List<AbilityData> GetAbilitiesByTrigger(string trigger) =>
            _db.Query<AbilityData>(
                "SELECT * FROM abilities WHERE trigger = @trigger",
                new { trigger }).ToList();

        public List<AbilityData> GetAbilitiesByEffectId(int effectId) =>
            _db.Query<AbilityData>(
                "SELECT * FROM abilities WHERE effect_id = @effectId",
                new { effectId }).ToList();

        // ─── ConditionRow ────────────────────────────────────────────────────────

        public ConditionRow? GetConditionById(int id) =>
            _db.QuerySingle<ConditionRow>(
                "SELECT * FROM conditions WHERE id = @id",
                new { id });

        public List<ConditionRow> GetAllConditions() =>
            _db.Query<ConditionRow>("SELECT * FROM conditions ORDER BY id ASC").ToList();

        public List<ConditionRow> GetConditionsByType(string type) =>
            _db.Query<ConditionRow>(
                "SELECT * FROM conditions WHERE type = @type",
                new { type }).ToList();

        public List<ConditionRow> GetConditionsByStatus(string status) =>
            _db.Query<ConditionRow>(
                "SELECT * FROM conditions WHERE status = @status",
                new { status }).ToList();

        public List<ConditionRow> GetConditionsByWeather(string weather) =>
            _db.Query<ConditionRow>(
                "SELECT * FROM conditions WHERE weather = @weather",
                new { weather }).ToList();

        /// <summary>
        /// Recursively loads a condition and all its children (left, right, inner).
        /// Uses the internal cache to avoid redundant DB calls.
        /// </summary>
        public ConditionRow? GetConditionTreeById(int id)
        {
            if (_conditionCache.TryGetValue(id, out var cached))
                return cached;

            var row = GetConditionById(id);
            if (row is null) return null;

            _conditionCache[id] = row;

            if (row.LeftConditionId.HasValue)
                row.LeftCondition = GetConditionTreeById(row.LeftConditionId.Value);

            if (row.RightConditionId.HasValue)
                row.RightCondition = GetConditionTreeById(row.RightConditionId.Value);

            if (row.InnerConditionId.HasValue)
                row.InnerCondition = GetConditionTreeById(row.InnerConditionId.Value);

            return row;
        }

        // ─── EffectRow ───────────────────────────────────────────────────────────

        public EffectRow? GetEffectById(int id) =>
            _db.QuerySingle<EffectRow>(
                "SELECT * FROM effects WHERE id = @id",
                new { id });

        public List<EffectRow> GetAllEffects() =>
            _db.Query<EffectRow>("SELECT * FROM effects ORDER BY id ASC").ToList();

        public List<EffectRow> GetEffectsByType(string type) =>
            _db.Query<EffectRow>(
                "SELECT * FROM effects WHERE type = @type",
                new { type }).ToList();

        public List<EffectRow> GetEffectsByTarget(string target) =>
            _db.Query<EffectRow>(
                "SELECT * FROM effects WHERE target = @target",
                new { target }).ToList();

        public List<EffectRow> GetEffectsByStat(string stat) =>
            _db.Query<EffectRow>(
                "SELECT * FROM effects WHERE stat = @stat",
                new { stat }).ToList();

        public List<EffectRow> GetEffectsByConditionId(int conditionId) =>
            _db.Query<EffectRow>(
                "SELECT * FROM effects WHERE condition_id = @conditionId",
                new { conditionId }).ToList();

        /// <summary>
        /// Recursively loads an effect and its child/pass/fail effect chains.
        /// Uses the internal cache to avoid redundant DB calls.
        /// </summary>


        // ─── AbilityTreeData (joined) ─────────────────────────────────────────────

        private const string _abilityTreeSql
            =
            @"SELECT
                a.name           AS AbilityName,
                a.description    AS Description,
                e.type           AS EffectType,
                e.target         AS Target,
                e.stat           AS StatAffected,
                e.stat_stages    AS Stages,
                n.exact_value    AS Value,
                n.range_min      AS MinRange,
                n.range_max      AS MaxRange,
                c.type           AS ConditionTrigger
            FROM abilities a
            LEFT JOIN effects    e ON a.effect_id    = e.id
            LEFT JOIN numbers    n ON e.number_id     = n.id
            LEFT JOIN conditions c ON a.condition_id  = c.id";

        public AbilityTree? GetAbilityTreeById(int id) =>
            _db.QuerySingle<AbilityTree>(
                _abilityTreeSql + " WHERE a.id = @id",
                new { id });

        public AbilityTree? GetAbilityTreeByName(string name) =>
            _db.QuerySingle<AbilityTree>(
                _abilityTreeSql + " WHERE a.name = @name",
                new { name });

        public List<AbilityTree> GetAllAbilityTrees() =>
            _db.Query<AbilityTree>(_abilityTreeSql + " ORDER BY a.id ASC").ToList();

        public AbilityTree? GetRandomAbilityTree() =>
            _db.QuerySingle<AbilityTree>(
                _abilityTreeSql + " WHERE a.id IN (SELECT id FROM abilities ORDER BY RANDOM() LIMIT 1)");
        public NumberData? LoadNumber(int id)
        {
            if (_numberCache.TryGetValue(id, out var cached))
            {
                return cached;
            }

            var row = _db.QuerySingle<NumberData?>(
                @"SELECT id AS Id, type AS Type, exact_value AS ExactValue, 
                     range_min AS RangeMin, range_max AS RangeMax, 
                     left_number_id AS LeftNumberId, right_number_id AS RightNumberId, 
                     target AS Target
              FROM numbers WHERE id = @id", new { id });

            if (row != null)
            {
                _numberCache[id] = row;
            }

            return row;
        }
    }
}