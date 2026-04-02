using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Move;


namespace PokemonGame.Services.Data.Repositories
{
    // MoveRepository.cs
    internal class MoveRepository : DbRepository<string, MoveData>
    {
        internal MoveRepository(IDbConnectionService db) : base(db) { }

        public MoveData? LoadByName(string name) =>
            GetCached(name, () => _db.QuerySingle<MoveData>(
                @"SELECT id, name, element, category, target, pp, priority, 
                     crit_stage, description 
              FROM moves WHERE name = @name",
                new { name }));

        public string? GetName(int id) =>
            _db.QueryScalar<string>("SELECT name FROM moves WHERE id = @id", new { id });

        public List<MoveData> GetAll() =>
            GetAllCached(
                () => _db.Query<MoveData>("SELECT * FROM moves").ToList(),
                m => m.Name);
    }
    // AttemptRepository.cs
    internal class AttemptRepository : DbRepository<int, AttemptRow>
    {
        private readonly Dictionary<int, List<CascadeStepRow>> _cascadeCache = new();

        internal AttemptRepository(IDbConnectionService db) : base(db) { }

        public List<AttemptRow> LoadForMove(int moveId)
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



            return rows;
        }

        public AttemptRow? Load(int id) =>
            GetCached(id, () => _db.QuerySingle<AttemptRow?>(
                "SELECT * FROM attempts WHERE id = @id", new { id }));


    }
    // CascadeStepRepository.cs
    internal class CascadeStepRepository : DbRepository<int, List<CascadeStepRow>>
    {
        internal CascadeStepRepository(IDbConnectionService db) : base(db) { }

        public List<CascadeStepRow> LoadForAttempt(int attemptId) =>
            GetCached(attemptId, () => _db.Query<CascadeStepRow>(
                "SELECT * FROM cascade_steps WHERE cascade_attempt_id = @id ORDER BY step_order",
                new { id = attemptId }).ToList());
    }
    // WeightedEntryRepository.cs
    internal class WeightedEntryRepository : DbRepository<int, List<MoveWeightedEntryData>>
    {
        internal WeightedEntryRepository(IDbConnectionService db) : base(db) { }

        public List<MoveWeightedEntryData> LoadForNumber(int numberId) =>
            GetCached(numberId, () => _db.Query<MoveWeightedEntryData>(
                "SELECT * FROM weighted_entries WHERE number_id = @id",
                new { id = numberId }).ToList());
    }
    // SequenceStepRepository.cs
    internal class SequenceStepRepository : DbRepository<int, List<SequenceStepRow>>
    {
        internal SequenceStepRepository(IDbConnectionService db) : base(db) { }

        public List<SequenceStepRow> LoadForEffect(int effectId) =>
            GetCached(effectId, () => _db.Query<SequenceStepRow>(
                "SELECT * FROM sequence_steps WHERE sequence_effect_id = @id ORDER BY step_order",
                new { id = effectId }).ToList());
    }
    internal class EffectRepository : DbRepository<int, EffectRow>
    {
        internal EffectRepository(IDbConnectionService db) : base(db) { }

        public EffectRow? Load(int id) =>
            GetCached(id, () => _db.QuerySingle<EffectRow?>(
                @"SELECT id AS Id, type AS Type, target AS Target, number_id AS NumberId, 
                     heal_target AS HealTarget, chance_probability AS ChanceProbability, 
                     child_effect_id AS ChildEffectId, condition_id AS ConditionId, 
                     on_pass_effect_id AS OnPassEffectId, on_fail_effect_id AS OnFailEffectId, 
                     stat AS Stat, stat_stages AS StatStages, sleep_min_turns AS SleepMinTurns, 
                     sleep_max_turns AS SleepMaxTurns, confuse_min_turns AS ConfuseMinTurns, 
                     confuse_max_turns AS ConfuseMaxTurns, is_toxic AS IsToxic, 
                     weather AS Weather, weather_turns AS WeatherTurns, screen AS Screen, 
                     screen_turns AS ScreenTurns, battle_side AS BattleSide, 
                     hazard AS Hazard, charge_turns AS ChargeTurns,
                     multiplier AS Multiplier, status AS Status
              FROM effects WHERE id = @id", new { id }));
    }
    internal class NumberRepository : DbRepository<int, NumberData>
    {
        internal NumberRepository(IDbConnectionService db) : base(db) { }

        public NumberData? Load(int id) =>
            GetCached(id, () => _db.QuerySingle<NumberData?>(
                @"SELECT id AS Id, type AS Type, exact_value AS ExactValue, 
                     range_min AS RangeMin, range_max AS RangeMax, 
                     left_number_id AS LeftNumberId, right_number_id AS RightNumberId, 
                     target AS Target
              FROM numbers WHERE id = @id", new { id }));
    }
    // MultiStatChangeRepository.cs
    internal class MultiStatChangeRepository : DbRepository<int, List<MultiStatChangeRow>>
    {
        internal MultiStatChangeRepository(IDbConnectionService db) : base(db) { }

        public List<MultiStatChangeRow> LoadForEffect(int effectId) =>
            GetCached(effectId, () => _db.Query<MultiStatChangeRow>(
                "SELECT * FROM multi_stat_changes WHERE effect_id = @id",
                new { id = effectId }).ToList());
    }
    internal class ConditionRepository : DbRepository<int, ConditionRow>
    {
        internal ConditionRepository(IDbConnectionService db) : base(db) { }

        public ConditionRow? Load(int id) =>
            GetCached(id, () => _db.QuerySingle<ConditionRow?>(
                @"SELECT id AS Id, type AS Type, probability AS Probability, weather AS Weather, 
                     status AS Status, volatile_status AS VolatileStatus, 
                     hp_fraction AS HpFraction, pokemon_type AS PokemonType, 
                     left_condition_id AS LeftConditionId, right_condition_id AS RightConditionId, 
                     inner_condition_id AS InnerConditionId
              FROM conditions WHERE id = @id", new { id }));
    }
}