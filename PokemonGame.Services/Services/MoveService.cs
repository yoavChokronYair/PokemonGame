using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public interface IMoveService
    {
        MoveTree? GetMove(string name);
    }

    public class MoveService : IMoveService
    {
        // ── Repositories ─────────────────────────────────────────────────────────
        private readonly MoveRepository _moves;
        private readonly AttemptRepository _attempts;
        private readonly CascadeStepRepository _cascadeSteps;
        private readonly EffectRepository _effects;
        private readonly SequenceStepRepository _sequenceSteps;
        private readonly MultiStatChangeRepository _multiStatChanges;
        private readonly NumberRepository _numbers;
        private readonly WeightedEntryRepository _weightedEntries;
        private readonly ConditionRepository _conditions;

        // ── Cycle guards (reset per GetMove call) ────────────────────────────────
        private readonly HashSet<int> _visitedEffects = new();
        private readonly HashSet<int> _visitedNumbers = new();
        private readonly HashSet<int> _visitedAttempts = new();

        public MoveService()
        {
            var f = ServiceFactory.Instance;
            _moves = f.MoveRepository;
            _attempts = f.AttemptRepository;
            _cascadeSteps = f.CascadeStepRepository;
            _effects = f.EffectRepository;
            _sequenceSteps = f.SequenceStepRepository;
            _multiStatChanges = f.MultiStatChangeRepository;
            _numbers = f.NumberRepository;
            _weightedEntries = f.WeightedEntryRepository;
            _conditions = f.ConditionRepository;
        }

        // ── Public entry point ───────────────────────────────────────────────────

        public MoveTree? GetMove(string name)
        {
            var move = _moves.LoadByName(name);
            if (move == null) return null;

            _visitedEffects.Clear();
            _visitedNumbers.Clear();
            _visitedAttempts.Clear();

            var tree = new MoveTree
            {
                Move = move,
                Priority = move.Priority,
                CritStage = move.CritStage,
                Description = move.Description,
            };

            foreach (var row in _attempts.LoadForMove(move.Id))
                tree.Attempts.Add(BuildAttempt(row));

            return tree;
        }

        // ── Attempt ──────────────────────────────────────────────────────────────

        private MoveAttempt BuildAttempt(AttemptRow row)
        {
            if (_visitedAttempts.Contains(row.Id))
                return new MoveAttempt { Id = row.Id, Type = row.Type };

            _visitedAttempts.Add(row.Id);

            var attempt = new MoveAttempt
            {
                Id = row.Id,
                Type = row.Type,
                AccuracyValue = row.AccuracyValue,
                StopOnMiss = row.StopOnMiss == 1,
                RampageMinTurns = row.RampageMinTurns,
                RampageMaxTurns = row.RampageMaxTurns,
            };

            if (row.OnHitEffectId.HasValue) attempt.OnHit = BuildEffect(row.OnHitEffectId.Value);
            if (row.OnMissEffectId.HasValue) attempt.OnMiss = BuildEffect(row.OnMissEffectId.Value);
            if (row.AfterEffectId.HasValue) attempt.After = BuildEffect(row.AfterEffectId.Value);

            foreach (var step in _cascadeSteps.LoadForAttempt(row.Id))
            {
                var child = _attempts.Load(step.ChildAttemptId);
                if (child != null) attempt.CascadeSteps.Add(BuildAttempt(child));
            }

            if (row.HitsNumberId.HasValue) attempt.HitsNumber = BuildNumber(row.HitsNumberId.Value);
            if (row.ChargeEffectId.HasValue) attempt.ChargeEffect = BuildEffect(row.ChargeEffectId.Value);

            if (row.ReleaseAttemptId.HasValue)
            {
                var release = _attempts.Load(row.ReleaseAttemptId.Value);
                if (release != null) attempt.ReleaseAttempt = BuildAttempt(release);
            }

            if (row.AfterRampageEffectId.HasValue)
                attempt.AfterRampage = BuildEffect(row.AfterRampageEffectId.Value);

            return attempt;
        }

        // ── Effect ───────────────────────────────────────────────────────────────

        private MoveEffect? BuildEffect(int id)
        {
            if (_visitedEffects.Contains(id)) return null;
            _visitedEffects.Add(id);

            var row = _effects.Load(id);
            if (row == null) return null;

            var effect = new MoveEffect
            {
                Id = row.Id,
                Type = row.Type,
                Target = row.Target,
                HealTarget = row.HealTarget,
                ChanceProbability = row.ChanceProbability,
                Stat = row.Stat,
                StatStages = row.StatStages,
                SleepMinTurns = row.SleepMinTurns,
                SleepMaxTurns = row.SleepMaxTurns,
                ConfuseMinTurns = row.ConfuseMinTurns,
                ConfuseMaxTurns = row.ConfuseMaxTurns,
                IsToxic = row.IsToxic == 1,
                Weather = row.Weather,
                WeatherTurns = row.WeatherTurns,
                Screen = row.Screen,
                ScreenTurns = row.ScreenTurns,
                BattleSide = row.BattleSide,
                Hazard = row.Hazard,
                ChargeTurns = row.ChargeTurns,
                Multiplier = row.Multiplier,
                Fraction = row.Fraction,
                VolatileStatus = row.VolatileStatus,
            };

            if (row.NumberId.HasValue) effect.Number = BuildNumber(row.NumberId.Value);
            if (row.ChildEffectId.HasValue) effect.ChanceChild = BuildEffect(row.ChildEffectId.Value);
            if (row.ConditionId.HasValue) effect.Condition = BuildCondition(row.ConditionId.Value);
            if (row.OnPassEffectId.HasValue) effect.OnPass = BuildEffect(row.OnPassEffectId.Value);
            if (row.OnFailEffectId.HasValue) effect.OnFail = BuildEffect(row.OnFailEffectId.Value);

            effect.StatChanges = _multiStatChanges.LoadForEffect(id);

            foreach (var step in _sequenceSteps.LoadForEffect(id))
            {
                var child = BuildEffect(step.ChildEffectId);
                if (child != null) effect.SequenceSteps.Add(child);
            }

            _visitedEffects.Remove(id);
            return effect;
        }

        // ── Number ───────────────────────────────────────────────────────────────

        private MoveNumber? BuildNumber(int id)
        {
            if (_visitedNumbers.Contains(id)) return null;
            _visitedNumbers.Add(id);

            var row = _numbers.Load(id);
            if (row == null) return null;

            var number = new MoveNumber
            {
                Id = row.Id,
                Type = row.Type,
                ExactValue = row.ExactValue,
                RangeMin = row.RangeMin,
                RangeMax = row.RangeMax,
                Target = row.Target,
            };

            if (row.Type == "Weighted")
                number.WeightedEntries = _weightedEntries.LoadForNumber(id);

            if (row.LeftNumberId.HasValue) number.Left = BuildNumber(row.LeftNumberId.Value);
            if (row.RightNumberId.HasValue) number.Right = BuildNumber(row.RightNumberId.Value);

            _visitedNumbers.Remove(id);
            return number;
        }

        // ── Condition ────────────────────────────────────────────────────────────

        private MoveCondition? BuildCondition(int id)
        {
            var row = _conditions.Load(id);
            if (row == null) return null;

            var condition = new MoveCondition
            {
                Id = row.Id,
                Type = row.Type,
                Probability = row.Probability,
                Weather = row.Weather,
                Status = row.Status,
                VolatileStatus = row.VolatileStatus,
                HpFraction = row.HpFraction,
                PokemonType = row.PokemonType,
            };

            if (row.LeftConditionId.HasValue) condition.Left = BuildCondition(row.LeftConditionId.Value);
            if (row.RightConditionId.HasValue) condition.Right = BuildCondition(row.RightConditionId.Value);
            if (row.InnerConditionId.HasValue) condition.Inner = BuildCondition(row.InnerConditionId.Value);

            return condition;
        }
    }
}