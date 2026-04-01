using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Move;


namespace PokemonGame.Services.Data.Repositories
{
    internal class MoveRepository : DbRepository<string, MoveData>
    {
        // Secondary caches for sub-tables
        private readonly Dictionary<int, NumberData> _numberCache = new();
        private readonly Dictionary<int, ConditionRow> _conditionCache = new();
        private readonly Dictionary<int, EffectRow> _effectCache = new();
        private readonly Dictionary<int, AttemptRow> _attemptCache = new();
        private readonly Dictionary<int, List<SequenceStepRow>> _sequenceStepCache = new();
        private readonly Dictionary<int, List<CascadeStepRow>> _cascadeStepCache = new();
        private readonly Dictionary<int, List<MoveWeightedEntryData>> _weightedCache = new();
        private readonly Dictionary<int, List<MultiStatChangeRow>> _multiStatCache = new();

        internal MoveRepository(IDbConnectionService db) : base(db) { }

        // ── Move (flat) ───────────────────────────────────────────────────────────

        public MoveData LoadMoveData(string moveName) =>
    GetCached(moveName, () => _db.QuerySingle<MoveData>(
        @"SELECT id, name, element, category, target, pp, priority, 
                 crit_stage, description 
          FROM moves WHERE name = @name",
        new { name = moveName }));

        public List<AttemptRow> LoadAttemptsForMove(int moveId)
        {
            var rows = _db.Query<AttemptRow>(
                @"SELECT id AS Id, move_id AS MoveId, type AS Type, 
                     accuracy_value AS AccuracyValue, on_hit_effect_id AS OnHitEffectId, 
                     on_miss_effect_id AS OnMissEffectId, after_effect_id AS AfterEffectId, 
                     stop_on_miss AS StopOnMiss, hits_number_id AS HitsNumberId, 
                     charge_effect_id AS ChargeEffectId, release_attempt_id AS ReleaseAttemptId, 
                     rampage_min_turns AS RampageMinTurns, rampage_max_turns AS RampageMaxTurns, 
                     after_rampage_effect_id AS AfterRampageEffectId
              FROM attempts WHERE move_id = @id", new { id = moveId }).ToList();

            foreach (var r in rows)
            {
                _attemptCache[r.Id] = r;
            }

            return rows;
        }

        public EffectRow? LoadEffect(int id)
        {
            if (_effectCache.TryGetValue(id, out var cached))
            {
                return cached;
            }

            var row = _db.QuerySingle<EffectRow?>(
                @"SELECT id AS Id, type AS Type, target AS Target, number_id AS NumberId, 
                     heal_target AS HealTarget, chance_probability AS ChanceProbability, 
                     child_effect_id AS ChildEffectId, condition_id AS ConditionId, 
                     on_pass_effect_id AS OnPassEffectId, on_fail_effect_id AS OnFailEffectId, 
                     stat AS Stat, stat_stages AS StatStages, sleep_min_turns AS SleepMinTurns, 
                     sleep_max_turns AS SleepMaxTurns, confuse_min_turns AS ConfuseMinTurns, 
                     confuse_max_turns AS ConfuseMaxTurns, is_toxic AS IsToxic, 
                     weather AS Weather, weather_turns AS WeatherTurns, screen AS Screen, 
                     screen_turns AS ScreenTurns, battle_side AS BattleSide, 
                     hazard AS Hazard, charge_turns AS ChargeTurns 
              FROM effects WHERE id = @id", new { id });

            if (row != null)
            {
                _effectCache[id] = row;
            }

            return row;
        }
        public string? GetMoveName(int moveId) =>
           _db.QueryScalar<string>(
               "SELECT name FROM moves WHERE id = @mid",
               new { mid = moveId });
        public ConditionRow? LoadCondition(int id)
        {
            if (_conditionCache.TryGetValue(id, out var cached))
            {
                return cached;
            }

            var row = _db.QuerySingle<ConditionRow?>(
                @"SELECT id AS Id, type AS Type, probability AS Probability, weather AS Weather, 
                     status AS Status, volatile_status AS VolatileStatus, 
                     hp_fraction AS HpFraction, pokemon_type AS PokemonType, 
                     left_condition_id AS LeftConditionId, right_condition_id AS RightConditionId, 
                     inner_condition_id AS InnerConditionId
              FROM conditions WHERE id = @id", new { id });

            if (row != null)
            {
                _conditionCache[id] = row;
            }

            return row;
        }

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

        public List<MoveData> GetAllMoves() =>
            GetAllCached(
                () => _db.Query<MoveData>("SELECT * FROM moves").ToList(),
                m => m.Name);


        // ── Numbers ───────────────────────────────────────────────────────────────


        public List<MoveWeightedEntryData> LoadWeightedEntries(int numberId)
        {
            if (_weightedCache.TryGetValue(numberId, out var cached))
            {
                return cached;
            }

            var rows = _db.Query<MoveWeightedEntryData>(
                "SELECT * FROM weighted_entries WHERE number_id = @id", new { id = numberId }).ToList();
            _weightedCache[numberId] = rows;
            return rows;
        }

        // ── Conditions ────────────────────────────────────────────────────────────



        // ── Effects ───────────────────────────────────────────────────────────────



        public List<SequenceStepRow> LoadSequenceSteps(int sequenceEffectId)
        {
            if (_sequenceStepCache.TryGetValue(sequenceEffectId, out var cached))
            {
                return cached;
            }

            var rows = _db.Query<SequenceStepRow>(
                "SELECT * FROM sequence_steps WHERE sequence_effect_id = @id ORDER BY step_order",
                new { id = sequenceEffectId }).ToList();
            _sequenceStepCache[sequenceEffectId] = rows;
            return rows;
        }

        public List<MultiStatChangeRow> LoadMultiStatChanges(int effectId)
        {
            if (_multiStatCache.TryGetValue(effectId, out var cached))
            {
                return cached;
            }

            var rows = _db.Query<MultiStatChangeRow>(
                "SELECT * FROM multi_stat_changes WHERE effect_id = @id", new { id = effectId }).ToList();
            _multiStatCache[effectId] = rows;
            return rows;
        }

        // ── Attempts ─────────────────────────────────────────────────────────────



        public AttemptRow? LoadAttempt(int id)
        {
            if (_attemptCache.TryGetValue(id, out var cached))
            {
                return cached;
            }

            var row = _db.QuerySingle<AttemptRow?>(
                "SELECT * FROM attempts WHERE id = @id", new { id });
            if (row != null)
            {
                _attemptCache[id] = row;
            }

            return row;
        }

        public List<CascadeStepRow> LoadCascadeSteps(int cascadeAttemptId)
        {
            if (_cascadeStepCache.TryGetValue(cascadeAttemptId, out var cached))
            {
                return cached;
            }

            var rows = _db.Query<CascadeStepRow>(
                "SELECT * FROM cascade_steps WHERE cascade_attempt_id = @id ORDER BY step_order",
                new { id = cascadeAttemptId }).ToList();
            _cascadeStepCache[cascadeAttemptId] = rows;
            return rows;
        }
    }
}