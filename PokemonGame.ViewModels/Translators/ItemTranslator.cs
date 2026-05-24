using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;
using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.ViewModels.Translators
{
    public class ItemTranslator : BaseTranslator
    {
        private readonly IItemService _itemService;
        private readonly MoveTranslator _moveTranslator;

        public ItemTranslator()
        {
            _itemService = new LocalItemService();
            _moveTranslator = new MoveTranslator();
        }

        public ItemTranslator(
            IItemService itemService,
            MoveTranslator moveTranslator)
        {
            _itemService = itemService;
            _moveTranslator = moveTranslator;
        }

        // ─────────────────────────────────────────────────────────────
        // Public entry points
        // ─────────────────────────────────────────────────────────────

        public ItemsDomain TranslateById(int itemId)
        {
            var tree = _itemService.GetItemById(itemId)
                ?? throw new InvalidOperationException(
                    $"Item id '{itemId}' not found.");

            return TranslateTree(tree);
        }

        public ItemsDomain Translate(string itemName)
        {
            var tree = _itemService.GetItem(itemName)
                ?? throw new InvalidOperationException(
                    $"Item '{itemName}' not found.");

            return TranslateTree(tree);
        }

        // ─────────────────────────────────────────────────────────────
        // Main translator
        // ─────────────────────────────────────────────────────────────

        private ItemsDomain TranslateTree(ItemTree tree)
        {
            var item = tree.Item;

            IEffect baseEffect = tree.Effect != null
                ? TranslateEffect(tree.Effect)
                : new NoEffect();

            ItemsDomain result;

            if (tree.IsPokeball)
            {
                result = BuildPokeball(item, tree);
            }
            else if (tree.IsTmHm)
            {
                result = BuildTmHm(item, tree);
            }
            else if (tree.IsKeyItem)
            {
                result = BuildKeyItem(item, tree);
            }
            else if (tree.IsHeldItem)
            {
                result = BuildHeldItem(item, tree);
            }
            else
            {
                result = BuildNormalItem(item, baseEffect);
            }

            ApplyBaseItemData(result, item);

            return result;
        }

        private static void ApplyBaseItemData(
            ItemsDomain result,
            ItemData item)
        {
            result.Id = item.Id;
            result.Name = item.Name ?? string.Empty;
            result.Description = item.Description ?? string.Empty;
            result.Price = item.Price;

            // Keep translated subclass type unless DB has a reliable type.
            if (!string.IsNullOrWhiteSpace(item.Item_type) &&
                Enum.TryParse<ItemType>(
                    item.Item_type,
                    ignoreCase: true,
                    out var parsedType))
            {
                // Do not let old DB text convert TM back into Consumable.
                if (result is TmHmState tmHm)
                    result.Type = tmHm.IsHm ? ItemType.Hm : ItemType.Tm;
                else
                    result.Type = parsedType;
            }

            result.UsableInBattle = item.Usable_in_battle == 1;
            result.UsableInField = item.Usable_in_field == 1;
        }

        // ─────────────────────────────────────────────────────────────
        // Builders
        // ─────────────────────────────────────────────────────────────

        private static ItemsDomain BuildNormalItem(
            ItemData item,
            IEffect effect)
        {
            return new ItemsDomain
            {
                Name = item.Name ?? string.Empty,
                Description = item.Description ?? string.Empty,
                Type = ParseItemType(item.Item_type),
                Effect = effect,
                UsableInBattle = item.Usable_in_battle == 1,
                UsableInField = item.Usable_in_field == 1,
                Price = item.Price
            };
        }

        private PokeballState BuildPokeball(
            ItemData item,
            ItemTree tree)
        {
            var ballRow = tree.Pokeball
                ?? throw new InvalidOperationException(
                    $"Item id '{item.Id}' is marked as Pokeball but has no pokeballs row.");

            IEffect caughtEffect = tree.PokeballCaughtEffect != null
                ? TranslateEffect(tree.PokeballCaughtEffect)
                : new NoEffect();

            ICondition<BattleState> condition =
                tree.PokeballCondition != null
                    ? TranslateCondition(tree.PokeballCondition)
                    : new Probability<BattleState>(1.0);

            PokeBallType ballType =
                Enum.TryParse<PokeBallType>(
                    ballRow.Ball_type,
                    ignoreCase: true,
                    out var parsedBallType)
                    ? parsedBallType
                    : PokeBallType.PokeBall;

            return new PokeballState(
                name: item.Name ?? ballType.ToString(),
                caughtEffect: caughtEffect,
                condition: condition,
                multiplier: (float)ballRow.Multiplier,
                description: item.Description ?? string.Empty,
                ballType: ballType);
        }

        private ItemsDomain BuildTmHm(
            ItemData item,
            ItemTree tree)
        {
            var tmHmRow = tree.TmHm
                ?? throw new InvalidOperationException(
                    $"Item id '{item.Id}' is marked as TM/HM but has no tms_hms row.");

            /*
             * Your current MoveTranslator only has:
             *
             *     Translate(string moveName)
             *
             * I did not find:
             *
             *     TranslateById(int moveId)
             *
             * So for now we create a safe placeholder MoveState.
             * Later, the clean fix is to add MoveTranslator.TranslateById(...)
             * and replace BuildPlaceholderMove(...) with that.
             */
            MoveState move = BuildPlaceholderMove(tmHmRow.Move_id);

            bool isHm = tmHmRow.Is_hm == 1;

            return new TmHmState(
                name: item.Name ?? tmHmRow.Machine_id,
                move: move,
                isHm: isHm,
                isReusable: isHm,
                description: item.Description ?? string.Empty);
        }

        private KeyItemState BuildKeyItem(
            ItemData item,
            ItemTree tree)
        {
            var keyRow = tree.KeyItem
                ?? throw new InvalidOperationException(
                    $"Item id '{item.Id}' is marked as KeyItem but has no keyitems row.");

            IEffect usageEffect = tree.Effect != null
                ? TranslateEffect(tree.Effect)
                : new NoEffect();

            return new KeyItemState(
                usageEffect: usageEffect,
                fieldCondition: null,
                registerable: keyRow.Registerable == 1);
        }

        private ItemsDomain BuildHeldItem(
            ItemData item,
            ItemTree tree)
        {
            IEffect effect = tree.HeldItemEffect != null
                ? TranslateEffect(tree.HeldItemEffect)
                : new NoEffect();

            ICondition<BattleState> condition =
                tree.Condition != null
                    ? TranslateCondition(tree.Condition)
                    : new Probability<BattleState>(1.0);

            BattleEventTrigger trigger = BattleEventTrigger.None;

            if (tree.HeldItem != null &&
                !string.IsNullOrWhiteSpace(tree.HeldItem.Trigger) &&
                Enum.TryParse<BattleEventTrigger>(
                    tree.HeldItem.Trigger,
                    ignoreCase: true,
                    out var parsedTrigger))
            {
                trigger = parsedTrigger;
            }

            return new HeldItemState(
                name: item.Name ?? string.Empty,
                condition: condition,
                effect: effect,
                trigger: trigger,
                isConsumable: tree.HeldItem?.Is_one_time_use == 1,
                description: item.Description ?? string.Empty);
        }

        // ─────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────

        private static ItemType ParseItemType(string? itemType)
        {
            if (!string.IsNullOrWhiteSpace(itemType) &&
                Enum.TryParse<ItemType>(
                    itemType,
                    ignoreCase: true,
                    out var parsed))
            {
                return parsed;
            }

            return ItemType.Consumable;
        }

        private static MoveState BuildPlaceholderMove(int moveId)
        {
            return new MoveState(
                attempt: new Attempt(
                    accuracy: new Probability(1.0),
                    onHit: new NoEffect(),
                    onMiss: null,
                    after: null),
                name: $"Move #{moveId}",
                element: PokemonType.Normal,
                category: MoveCategory.Status,
                pp: 1,
                target: MoveTarget.Self,
                priority: 0,
                critStage: 0,
                description: "Move data not loaded yet.");
        }

        // ─────────────────────────────────────────────────────────────
        // Conditions
        // ─────────────────────────────────────────────────────────────

        public ICondition<BattleState> TranslateCondition(MoveCondition c) => c.Type switch
        {
            "IsNewPokemon" => new IsNewPokemon(),
            "WasHitByContact" => new WasHitByContact(),
            "TookDamageThisTurn" => new TookDamageThisTurn(),
            "DidKnockoutOpponent" => new DidKnockoutOpponent(),
            "HasAnyStatus" => new HasAnyStatus(),
            "HasBaseStatChanged" => new HasBaseStatChanged(),
            "IsHoldingItem" => new IsHoldingItem(),
            "IsGrounded" => new IsGrounded(),
            "IsAnyTerrainActive" => new IsAnyTerrainActive(),
            "IsBattleOver" => new IsBattleOver(),
            "IsFainted" => new IsFainted(),
            "IsFullHP" => new IsFullHP(),

            "IsWeatherActive" => new IsWeatherActive(ParseEnum<Weather>(c.Weather!)),
            "IsTerrainActive" => new IsTerrainActive(ParseEnum<TerrainType>(c.Terrain!)),
            "MoveHasTag" => new MoveHasTag(ParseEnum<MoveTag>(c.MoveTag!)),
            "MoveIsCategory" => new MoveIsCategory(ParseEnum<MoveCategory>(c.MoveCategory!)),
            "HasStatus" => new HasStatus(ParseEnum<StatusCondition>(c.Status!)),
            "HasVolatile" => new HasVolatile(ParseEnum<VolatileStatus>(c.VolatileStatus!)),
            "HasType" => new HasType(ParseEnum<PokemonType>(c.PokemonType!)),
            "HPBelow" => new HPBelow(c.HpFraction!.Value),
            "Probability" => new Probability<BattleState>(c.Probability!.Value),

            "And" => new And<BattleState>(
                TranslateCondition(c.Left!),
                TranslateCondition(c.Right!)),

            "Or" => new Or<BattleState>(
                TranslateCondition(c.Left!),
                TranslateCondition(c.Right!)),

            "Not" => new Not<BattleState>(
                TranslateCondition(c.Inner!)),

            "UserCondition" => new UserCondition(
                TranslateCondition(c.Inner!)),

            "OpponentCondition" => new OpponentCondition(
                TranslatePokemonCondition(c.Inner!)),

            _ => _moveTranslator.TranslateCondition(c)
        };

        public ICondition<PokemonState> TranslatePokemonCondition(MoveCondition c) => c.Type switch
        {
            "And" => new And<PokemonState>(
                TranslatePokemonCondition(c.Left!),
                TranslatePokemonCondition(c.Right!)),

            "Or" => new Or<PokemonState>(
                TranslatePokemonCondition(c.Left!),
                TranslatePokemonCondition(c.Right!)),

            "Not" => new Not<PokemonState>(
                TranslatePokemonCondition(c.Inner!)),

            "HasStatus" => new PokemonHasStatus(
                ParseEnum<StatusCondition>(c.Status!)),

            "HasVolatile" => new PokemonHasVolatile(
                ParseEnum<VolatileStatus>(c.VolatileStatus!)),

            "HasType" => new PokemonHasType(
                ParseEnum<PokemonType>(c.PokemonType!)),

            "HPBelow" => new PokemonHPBelow(
                c.HpFraction!.Value),

            "IsFullHP" => new PokemonIsFullHP(),

            "IsFainted" => new PokemonIsFainted(),

            _ => throw new NotSupportedException(
                $"Unknown pokemon condition type: '{c.Type}'")
        };
    }
}