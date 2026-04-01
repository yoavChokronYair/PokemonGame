using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.GameData.PokemonData;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public interface IAbilityService
    {
        AbilityTree? GetAbility(string name);
        AbilityTree? GetAbilityById(int id);
    }

    public class AbilityService : IAbilityService
    {
        private readonly AbilityRepository _repo;

        private readonly HashSet<int> _visitedEffects = new();
        private readonly HashSet<int> _visitedConditions = new();
        private readonly HashSet<int> _visitedNumbers = new();

        public AbilityService()
        {
            _repo = ServiceFactory.Instance.AbilityRepository;
        }

        // ── Public entry points ──────────────────────────────────────────────────

        public AbilityTree? GetAbility(string name)
        {
            var ability = _repo.GetAbilityByName(name);
            return ability == null ? null : BuildTree(ability);
        }

        public AbilityTree? GetAbilityById(int id)
        {
            var ability = _repo.GetAbilityById(id);
            return ability == null ? null : BuildTree(ability);
        }

        // ── Tree builder ─────────────────────────────────────────────────────────

        private AbilityTree BuildTree(AbilityData ability)
        {
            _visitedEffects.Clear();
            _visitedConditions.Clear();
            _visitedNumbers.Clear();

            var tree = new AbilityTree
            {
                Ability = ability,
                Name = ability.Name,
                Description = ability.Description,
                Trigger = ability.Trigger,
            };

            if (ability.Effect_id.HasValue)
                tree.Effect = BuildEffect(ability.Effect_id.Value);

            if (ability.Condition_id.HasValue)
                tree.Condition = BuildCondition(ability.Condition_id.Value);

            return tree;
        }

        // ── Effect builder ───────────────────────────────────────────────────────

        private MoveEffect? BuildEffect(int id)
        {
            if (_visitedEffects.Contains(id))
                return null;

            _visitedEffects.Add(id);

            var row = _repo.GetEffectById(id);
            if (row == null)
                return null;

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
            };

            if (row.NumberId.HasValue)
                effect.Number = BuildNumber(row.NumberId.Value);

            if (row.ChildEffectId.HasValue)
                effect.ChanceChild = BuildEffect(row.ChildEffectId.Value);

            if (row.ConditionId.HasValue)
                effect.Condition = BuildCondition(row.ConditionId.Value);

            if (row.OnPassEffectId.HasValue)
                effect.OnPass = BuildEffect(row.OnPassEffectId.Value);

            if (row.OnFailEffectId.HasValue)
                effect.OnFail = BuildEffect(row.OnFailEffectId.Value);

            _visitedEffects.Remove(id);
            return effect;
        }

        // ── Number builder ───────────────────────────────────────────────────────

        private MoveNumber? BuildNumber(int id)
        {
            if (_visitedNumbers.Contains(id))
                return null;

            _visitedNumbers.Add(id);

            var row = _repo.LoadNumber(id);
            if (row == null)
                return null;

            var number = new MoveNumber
            {
                Id = row.Id,
                Type = row.Type,
                ExactValue = row.ExactValue,
                RangeMin = row.RangeMin,
                RangeMax = row.RangeMax,
                Target = row.Target,
            };
            if (row.LeftNumberId.HasValue)
                number.Left = BuildNumber(row.LeftNumberId.Value);

            if (row.RightNumberId.HasValue)
                number.Right = BuildNumber(row.RightNumberId.Value);

            _visitedNumbers.Remove(id);
            return number;
        }

        // ── Condition builder ────────────────────────────────────────────────────

        private MoveCondition? BuildCondition(int id)
        {
            if (_visitedConditions.Contains(id))
                return null;

            _visitedConditions.Add(id);

            var row = _repo.GetConditionById(id);
            if (row == null)
                return null;

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

            if (row.LeftConditionId.HasValue)
                condition.Left = BuildCondition(row.LeftConditionId.Value);

            if (row.RightConditionId.HasValue)
                condition.Right = BuildCondition(row.RightConditionId.Value);

            if (row.InnerConditionId.HasValue)
                condition.Inner = BuildCondition(row.InnerConditionId.Value);

            _visitedConditions.Remove(id);
            return condition;
        }
    }
}