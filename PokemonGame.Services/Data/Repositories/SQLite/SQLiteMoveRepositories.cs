using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Move;


namespace PokemonGame.Services.Data.Repositories.SQLite
{
    internal class SQLiteMoveRepository : SQLiteRepository<string, MoveData>
    {
        // Secondary caches for sub-tables
        private readonly Dictionary<int, MoveNumberData> _numberCache = new();
        private readonly Dictionary<int, ConditionRow> _conditionCache = new();
        private readonly Dictionary<int, EffectRow> _effectCache = new();
        private readonly Dictionary<int, AttemptRow> _attemptCache = new();
        private readonly Dictionary<int, List<SequenceStepRow>> _sequenceStepCache = new();
        private readonly Dictionary<int, List<CascadeStepRow>> _cascadeStepCache = new();
        private readonly Dictionary<int, List<MoveWeightedEntryData>> _weightedCache = new();
        private readonly Dictionary<int, List<MultiStatChangeRow>> _multiStatCache = new();

        internal SQLiteMoveRepository(ISQLiteConnectionService db) : base(db) { }

        // ── Move (flat) ───────────────────────────────────────────────────────────

        public MoveData LoadMoveData(string moveName) =>
            GetCached(moveName, () => _db.QuerySingle<MoveData>(
                "SELECT * FROM moves WHERE name = @name", new { name = moveName }));

        public List<MoveData> GetAllMoves() =>
            GetAllCached(
                () => _db.Query<MoveData>("SELECT * FROM moves").ToList(),
                m => m.Name);

        // ── Numbers ───────────────────────────────────────────────────────────────

        public MoveNumberData? LoadNumber(int id)
        {
            if (_numberCache.TryGetValue(id, out var cached)) return cached;
            var row = _db.QuerySingle<MoveNumberData?>(
                "SELECT * FROM numbers WHERE id = @id", new { id });
            if (row != null) _numberCache[id] = row;
            return row;
        }

        public List<MoveWeightedEntryData> LoadWeightedEntries(int numberId)
        {
            if (_weightedCache.TryGetValue(numberId, out var cached)) return cached;
            var rows = _db.Query<MoveWeightedEntryData>(
                "SELECT * FROM weighted_entries WHERE number_id = @id", new { id = numberId }).ToList();
            _weightedCache[numberId] = rows;
            return rows;
        }

        // ── Conditions ────────────────────────────────────────────────────────────

        public ConditionRow? LoadCondition(int id)
        {
            if (_conditionCache.TryGetValue(id, out var cached)) return cached;
            var row = _db.QuerySingle<ConditionRow?>(
                "SELECT * FROM conditions WHERE id = @id", new { id });
            if (row != null) _conditionCache[id] = row;
            return row;
        }

        // ── Effects ───────────────────────────────────────────────────────────────

        public EffectRow? LoadEffect(int id)
        {
            if (_effectCache.TryGetValue(id, out var cached)) return cached;
            var row = _db.QuerySingle<EffectRow?>(
                "SELECT * FROM effects WHERE id = @id", new { id });
            if (row != null) _effectCache[id] = row;
            return row;
        }

        public List<SequenceStepRow> LoadSequenceSteps(int sequenceEffectId)
        {
            if (_sequenceStepCache.TryGetValue(sequenceEffectId, out var cached)) return cached;
            var rows = _db.Query<SequenceStepRow>(
                "SELECT * FROM sequence_steps WHERE sequence_effect_id = @id ORDER BY step_order",
                new { id = sequenceEffectId }).ToList();
            _sequenceStepCache[sequenceEffectId] = rows;
            return rows;
        }

        public List<MultiStatChangeRow> LoadMultiStatChanges(int effectId)
        {
            if (_multiStatCache.TryGetValue(effectId, out var cached)) return cached;
            var rows = _db.Query<MultiStatChangeRow>(
                "SELECT * FROM multi_stat_changes WHERE effect_id = @id", new { id = effectId }).ToList();
            _multiStatCache[effectId] = rows;
            return rows;
        }

        // ── Attempts ─────────────────────────────────────────────────────────────

        public List<AttemptRow> LoadAttemptsForMove(int moveId)
        {
            var rows = _db.Query<AttemptRow>(
                "SELECT * FROM attempts WHERE move_id = @id", new { id = moveId }).ToList();
            foreach (var r in rows) _attemptCache[r.Id] = r;
            return rows;
        }

        public AttemptRow? LoadAttempt(int id)
        {
            if (_attemptCache.TryGetValue(id, out var cached)) return cached;
            var row = _db.QuerySingle<AttemptRow?>(
                "SELECT * FROM attempts WHERE id = @id", new { id });
            if (row != null) _attemptCache[id] = row;
            return row;
        }

        public List<CascadeStepRow> LoadCascadeSteps(int cascadeAttemptId)
        {
            if (_cascadeStepCache.TryGetValue(cascadeAttemptId, out var cached)) return cached;
            var rows = _db.Query<CascadeStepRow>(
                "SELECT * FROM cascade_steps WHERE cascade_attempt_id = @id ORDER BY step_order",
                new { id = cascadeAttemptId }).ToList();
            _cascadeStepCache[cascadeAttemptId] = rows;
            return rows;
        }
    }
}