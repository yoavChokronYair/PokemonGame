using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;
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


        // ── Condition ────────────────────────────────────────────────────────

        public ICondition<BattleState> TranslateCondition(MoveCondition c) => c.Type switch
        {
            // ── Parameterless ────────────────────────────────────────────────
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

            // ── Parameterized ────────────────────────────────────────────────
            "IsWeatherActive" => new IsWeatherActive(ParseEnum<Weather>(c.Weather!)),
            "IsTerrainActive" => new IsTerrainActive(ParseEnum<TerrainType>(c.Terrain!)),
            "MoveHasTag" => new MoveHasTag(ParseEnum<MoveTag>(c.MoveTag!)),
            "MoveIsCategory" => new MoveIsCategory(ParseEnum<MoveCategory>(c.MoveCategory!)),
            "HasStatus" => new HasStatus(ParseEnum<StatusCondition>(c.Status!)),
            "HasVolatile" => new HasVolatile(ParseEnum<VolatileStatus>(c.VolatileStatus!)),
            "HasType" => new HasType(ParseEnum<PokemonType>(c.PokemonType!)),
            "HPBelow" => new HPBelow(c.HpFraction!.Value),
            "Probability" => new Probability(c.Probability!.Value),

            // ── Combinators ──────────────────────────────────────────────────
            "And" => new And<BattleState>(TranslateCondition(c.Left!), TranslateCondition(c.Right!)),
            "Or" => new Or<BattleState>(TranslateCondition(c.Left!), TranslateCondition(c.Right!)),
            "Not" => new Not<BattleState>(TranslateCondition(c.Inner!)),

            // ── Adapters ─────────────────────────────────────────────────────
            "UserCondition" => new UserCondition(TranslateCondition(c.Inner!)),
            "OpponentCondition" => new OpponentCondition(TranslatePokemonCondition(c.Inner!)),

            // ── Fallback ─────────────────────────────────────────────────────
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

        // ── Effect ───────────────────────────────────────────────────────────

        public IEffect TranslateEffect(MoveEffect e) => e.Type switch
        {
            // ── Item-exclusive ───────────────────────────────────────────────
            "ModifyDamageDealt" => new ModifyDamageDealt(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifySpeedMultiplier" => new ModifySpeedMultiplier(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifyAccuracy" => new ModifyAccuracy(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifyEvasion" => new ModifyEvasion(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifyCritRatio" => new ModifyCritRatio(ResolveTarget(e.Target), e.StatStages ?? 0),
            "ResetNegativeStatStages" => new ResetNegativeStatStages(ResolveTarget(e.Target)),
            "CureVolatile" => new CureVolatile(ResolveTarget(e.Target), ParseEnum<VolatileStatus>(e.Status!)),
            "CureSpecificStatus" => new CureSpecificStatus(ResolveTarget(e.Target), ParseEnum<StatusCondition>(e.Stat!)),

            // ── Combinators ──────────────────────────────────────────────────
            "Sequence" => new Sequence(e.SequenceSteps.Select(TranslateEffect).ToList()),
            "Chance" => new Chance(e.ChanceProbability ?? 0, TranslateEffect(e.ChanceChild!)),
            "Conditional" => new Conditional(
                                 TranslateCondition(e.Condition!),
                                 TranslateEffect(e.OnPass!),
                                 e.OnFail != null ? TranslateEffect(e.OnFail) : null),

            // ── Fallback ─────────────────────────────────────────────────────
            _ => new NoEffect()
        };

        // ── Helpers ──────────────────────────────────────────────────────────

        private static ITarget ResolveTarget(string? target) => target switch
        {
            "Attacker" => new AttackerTarget(),
            "Defender" => new DefenderTarget(),
            _ => new AttackerTarget() // items default to holder
        };

        private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum
        {
            if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
            {
                return result;
            }

            throw new InvalidOperationException($"Cannot parse '{value}' as {typeof(TEnum).Name}");
        }
    }
}