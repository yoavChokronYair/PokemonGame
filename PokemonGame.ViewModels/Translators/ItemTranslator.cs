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

        public ItemTranslator(IItemService itemService, MoveTranslator moveTranslator)
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
                ?? throw new InvalidOperationException($"Item id '{itemId}' not found.");

            return TranslateTree(tree);
        }

        public ItemsDomain Translate(string itemName)
        {
            var tree = _itemService.GetItem(itemName)
                ?? throw new InvalidOperationException($"Item '{itemName}' not found.");

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

            ICondition<BattleState>? baseCondition = tree.Condition != null
                ? TranslateCondition(tree.Condition)
                : null;

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

        private static void ApplyBaseItemData(ItemsDomain result, ItemData item)
        {
            result.Id = item.Id;
            result.Name = item.Name ?? string.Empty;
            result.Description = item.Description ?? string.Empty;
            result.Price = item.Price;
            result.UsableInBattle = item.Usable_in_battle == 1;
            result.UsableInField = item.Usable_in_field == 1;
        }

        // ─────────────────────────────────────────────────────────────
        // Builders
        // ─────────────────────────────────────────────────────────────

        private static ItemsDomain BuildNormalItem(ItemData item, IEffect effect)
        {
            return new ItemsDomain
            {
                Type = ParseItemType(item.Item_type),
                Effect = effect
            };
        }

        private PokeballState BuildPokeball(ItemData item, ItemTree tree)
        {
            var ballRow = tree.Pokeball
                ?? throw new InvalidOperationException($"Item id '{item.Id}' is marked as Pokeball but has no pokeballs row.");

            IEffect caughtEffect = tree.PokeballCaughtEffect != null
                ? TranslateEffect(tree.PokeballCaughtEffect)
                : new NoEffect();

            ICondition<BattleState> condition = tree.PokeballCondition != null
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
                description: item.Description ?? string.Empty)
            {
                BallType = ballType
            };
        }

        private ItemsDomain BuildTmHm(ItemData item, ItemTree tree)
        {
            var tmHmRow = tree.TmHm
                ?? throw new InvalidOperationException($"Item id '{item.Id}' is marked as TM/HM but has no tms_hms row.");

            // Your current TmHmData has Move_id, not MoveName.
            // So for now we create the TM/HM item cleanly with move = null.
            // Teaching the move can later use tmHmRow.Move_id directly.
            MoveState? move = null;

            return new TmHmState(
                name: item.Name ?? tmHmRow.Machine_id,
                move: move!,
                isHm: tmHmRow.Is_hm == 1,
                description: item.Description ?? string.Empty);
        }

        private KeyItemState BuildKeyItem(ItemData item, ItemTree tree)
        {
            var keyRow = tree.KeyItem
                ?? throw new InvalidOperationException($"Item id '{item.Id}' is marked as KeyItem but has no keyitems row.");

            ICondition<BattleState>? condition = tree.KeyItemCondition != null
                ? TranslateCondition(tree.KeyItemCondition)
                : null;

            return new KeyItemState(
                usageEffect: null!,
                condition: condition,
                registerable: keyRow.Registerable == 1);
        }

        private ItemsDomain BuildHeldItem(ItemData item, ItemTree tree)
        {
            IEffect effect = tree.HeldItemEffect != null
                ? TranslateEffect(tree.HeldItemEffect)
                : new NoEffect();

            return new ItemsDomain
            {
                Type = ItemType.HeldItem,
                Effect = effect
            };
        }

        private static ItemType ParseItemType(string? itemType)
        {
            if (!string.IsNullOrWhiteSpace(itemType) &&
                Enum.TryParse<ItemType>(itemType, ignoreCase: true, out var parsed))
            {
                return parsed;
            }

            return ItemType.Consumable;
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
            "Probability" => new Probability(c.Probability!.Value),

            "And" => new And<BattleState>(TranslateCondition(c.Left!), TranslateCondition(c.Right!)),
            "Or" => new Or<BattleState>(TranslateCondition(c.Left!), TranslateCondition(c.Right!)),
            "Not" => new Not<BattleState>(TranslateCondition(c.Inner!)),

            "UserCondition" => new UserCondition(TranslateCondition(c.Inner!)),
            "OpponentCondition" => new OpponentCondition(TranslatePokemonCondition(c.Inner!)),

            _ => _moveTranslator.TranslateCondition(c)
        };

        public ICondition<PokemonState> TranslatePokemonCondition(MoveCondition c) => c.Type switch
        {
            "And" => new And<PokemonState>(TranslatePokemonCondition(c.Left!), TranslatePokemonCondition(c.Right!)),
            "Or" => new Or<PokemonState>(TranslatePokemonCondition(c.Left!), TranslatePokemonCondition(c.Right!)),
            "Not" => new Not<PokemonState>(TranslatePokemonCondition(c.Inner!)),

            "HasStatus" => new PokemonHasStatus(ParseEnum<StatusCondition>(c.Status!)),
            "HasVolatile" => new PokemonHasVolatile(ParseEnum<VolatileStatus>(c.VolatileStatus!)),
            "HasType" => new PokemonHasType(ParseEnum<PokemonType>(c.PokemonType!)),
            "HPBelow" => new PokemonHPBelow(c.HpFraction!.Value),
            "IsFullHP" => new PokemonIsFullHP(),
            "IsFainted" => new PokemonIsFainted(),

            _ => throw new NotSupportedException($"Unknown pokemon condition type: '{c.Type}'")
        };

        // ─────────────────────────────────────────────────────────────
        // Effects
        // ─────────────────────────────────────────────────────────────

        public override IEffect TranslateEffect(MoveEffect e) => e.Type switch
        {
            "ModifyDamageDealt" => new ModifyDamageDealt(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifySpeedMultiplier" => new ModifySpeedMultiplier(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifyAccuracy" => new ModifyAccuracy(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifyEvasion" => new ModifyEvasion(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifyCritRatio" => new ModifyCritRatio(ResolveTarget(e.Target), e.StatStages ?? 0),
            "ResetNegativeStatStages" => new ResetNegativeStatStages(ResolveTarget(e.Target)),
            "CureVolatile" => new CureVolatile(ResolveTarget(e.Target), ParseEnum<VolatileStatus>(e.Status!)),
            "CureSpecificStatus" => new CureSpecificStatus(ResolveTarget(e.Target), ParseEnum<StatusCondition>(e.Stat!)),

            "Sequence" => new Sequence(e.SequenceSteps.Select(TranslateEffect).ToList()),
            "Chance" => new Chance(e.ChanceProbability ?? 0, TranslateEffect(e.ChanceChild!)),
            "Conditional" => new Conditional(
                TranslateCondition(e.Condition!),
                TranslateEffect(e.OnPass!),
                e.OnFail != null ? TranslateEffect(e.OnFail) : null),

            _ => base.TranslateEffect(e)
        };

        private static ITarget ResolveTarget(string? target) => target switch
        {
            "Attacker" => new AttackerTarget(),
            "Defender" => new DefenderTarget(),
            _ => new AttackerTarget()
        };

        private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum
        {
            if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
                return result;

            throw new InvalidOperationException($"Cannot parse '{value}' as {typeof(TEnum).Name}");
        }
    }
}