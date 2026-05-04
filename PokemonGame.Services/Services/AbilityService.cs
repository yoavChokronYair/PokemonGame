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
        private readonly ConditionRepository _conditionRepository;
        private readonly EffectRepository _effectRepository;
        private readonly NumberRepository _numberRepository;


        public AbilityService()
        {
            _repo = ServiceFactory.Instance.AbilityRepository;
            _conditionRepository = ServiceFactory.Instance.ConditionRepository;
            _effectRepository = ServiceFactory.Instance.EffectRepository;
            _numberRepository = ServiceFactory.Instance.NumberRepository;
        }
        internal AbilityService(AbilityRepository repo, ConditionRepository conditions,
                      EffectRepository effects, NumberRepository numbers)
        {
            _repo = repo;
            _conditionRepository = conditions;
            _effectRepository = effects;
            _numberRepository = numbers;
        }
        public AbilityService(ServiceFactory factory)
        {
            _repo = factory.AbilityRepository;
            _conditionRepository = factory.ConditionRepository;
            _effectRepository = factory.EffectRepository;
            _numberRepository = factory.NumberRepository;
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


            var tree = new AbilityTree
            {
                Ability = ability,
                Name = ability.Name,
                Description = ability.Description,
                Trigger = ability.Trigger,
            };

            if (ability.Effect_id.HasValue)
            {
                tree.Effect = BuildEffect(ability.Effect_id.Value);
            }

            if (ability.Condition_id.HasValue)
            {
                tree.Condition = BuildCondition(ability.Condition_id.Value);
            }

            return tree;
        }

        // ── Effect builder ───────────────────────────────────────────────────────

        private MoveEffect? BuildEffect(int id)
        {
            var row = _effectRepository.Load(id);
            if (row == null)
            {
                return null;
            }

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
            };

            if (row.NumberId.HasValue)
            {
                effect.Number = BuildNumber(row.NumberId.Value);
            }

            if (row.ChildEffectId.HasValue)
            {
                effect.ChanceChild = BuildEffect(row.ChildEffectId.Value);
            }

            if (row.ConditionId.HasValue)
            {
                effect.Condition = BuildCondition(row.ConditionId.Value);
            }

            if (row.OnPassEffectId.HasValue)
            {
                effect.OnPass = BuildEffect(row.OnPassEffectId.Value);
            }

            if (row.OnFailEffectId.HasValue)
            {
                effect.OnFail = BuildEffect(row.OnFailEffectId.Value);
            }

            return effect;
        }

        // ── Number builder ───────────────────────────────────────────────────────

        private MoveNumber? BuildNumber(int id)
        {
            var row = _numberRepository.Load(id);
            if (row == null)
            {
                return null;
            }

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
            {
                number.Left = BuildNumber(row.LeftNumberId.Value);
            }

            if (row.RightNumberId.HasValue)
            {
                number.Right = BuildNumber(row.RightNumberId.Value);
            }

            return number;
        }

        // ── Condition builder ────────────────────────────────────────────────────

        private MoveCondition? BuildCondition(int id)
        {


            var row = _conditionRepository.Load(id);
            if (row == null)
            {
                return null;
            }

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
            {
                condition.Left = BuildCondition(row.LeftConditionId.Value);
            }

            if (row.RightConditionId.HasValue)
            {
                condition.Right = BuildCondition(row.RightConditionId.Value);
            }

            if (row.InnerConditionId.HasValue)
            {
                condition.Inner = BuildCondition(row.InnerConditionId.Value);
            }

            return condition;
        }
    }
}