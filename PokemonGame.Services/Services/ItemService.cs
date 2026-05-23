using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Handler
{
    public class LocalItemService : IItemService
    {
        private readonly ItemRepository _repo;

        private readonly ConditionRepository _conditionRepository;
        private readonly EffectRepository _effectRepository;
        private readonly NumberRepository _numberRepository;

        private readonly PokeballRepository _pokeballRepository;
        private readonly TmHmRepository _tmHmRepository;
        private readonly KeyItemRepository _keyItemRepository;
        private readonly HeldItemRepository _heldItemRepository;

        public LocalItemService()
        {
            var factory = ServiceFactory.Instance;

            _repo = factory.ItemRepository;

            _conditionRepository = factory.ConditionRepository;
            _effectRepository = factory.EffectRepository;
            _numberRepository = factory.NumberRepository;

            _pokeballRepository = factory.PokeballRepository;
            _tmHmRepository = factory.TmHmRepository;
            _keyItemRepository = factory.KeyItemRepository;
            _heldItemRepository = factory.HeldItemRepository;
        }

        internal LocalItemService(
            ItemRepository repo,
            ConditionRepository conditions,
            EffectRepository effects,
            NumberRepository numbers,
            PokeballRepository pokeballs,
            TmHmRepository tmHms,
            KeyItemRepository keyItems,
            HeldItemRepository heldItems)
        {
            _repo = repo;

            _conditionRepository = conditions;
            _effectRepository = effects;
            _numberRepository = numbers;

            _pokeballRepository = pokeballs;
            _tmHmRepository = tmHms;
            _keyItemRepository = keyItems;
            _heldItemRepository = heldItems;
        }

        public ItemTree? GetItem(string name)
        {
            var item = _repo.GetByName(name);

            return item == null
                ? null
                : BuildTree(item);
        }

        public ItemTree? GetItemById(int id)
        {
            var item = _repo.GetById(id);

            return item == null
                ? null
                : BuildTree(item);
        }

        private ItemTree BuildTree(ItemData item)
        {
            var tree = new ItemTree
            {
                Item = item,
                Name = item.Name,
                Description = item.Description,
                Category = item.Category,
                IsConsumable = item.Is_consumable == 1,
            };

            if (item.Effect_id.HasValue)
                tree.Effect = BuildEffect(item.Effect_id.Value);

            if (item.Condition_id.HasValue)
                tree.Condition = BuildCondition(item.Condition_id.Value);

            tree.Pokeball = _pokeballRepository.GetByItemId(item.Id);
            tree.TmHm = _tmHmRepository.GetByItemId(item.Id);
            tree.KeyItem = _keyItemRepository.GetByItemId(item.Id);
            tree.HeldItem = _heldItemRepository.GetByItemId(item.Id);

            if (tree.Pokeball?.Caught_effect_id != null)
                tree.PokeballCaughtEffect = BuildEffect(tree.Pokeball.Caught_effect_id.Value);

            if (tree.Pokeball?.Condition_id != null)
                tree.PokeballCondition = BuildCondition(tree.Pokeball.Condition_id.Value);

            if (tree.KeyItem?.Usage_effect_id != null)
                tree.KeyItemUsageEffect = BuildEffect(tree.KeyItem.Usage_effect_id.Value);

            if (tree.KeyItem?.Condition_id != null)
                tree.KeyItemCondition = BuildCondition(tree.KeyItem.Condition_id.Value);

            if (tree.HeldItem?.Effect_id != null)
                tree.HeldItemEffect = BuildEffect(tree.HeldItem.Effect_id);

            if (tree.HeldItem?.Condition_id != null)
                tree.HeldItemCondition = BuildCondition(tree.HeldItem.Condition_id.Value);

            return tree;
        }

        private MoveEffect? BuildEffect(int id)
        {
            var row = _effectRepository.Load(id);

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
                Multiplier = row.Multiplier,
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

            return effect;
        }

        private MoveNumber? BuildNumber(int id)
        {
            var row = _numberRepository.Load(id);

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

            return number;
        }

        private MoveCondition? BuildCondition(int id)
        {
            var row = _conditionRepository.Load(id);

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

            return condition;
        }
    }
}