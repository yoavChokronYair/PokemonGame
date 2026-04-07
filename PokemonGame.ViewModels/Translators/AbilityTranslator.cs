using PokemonGame.Model.Domain;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model;
using PokemonGame.Model.Model.DesignPatterns;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.GameData.PokemonData;
using PokemonGame.Services.Handler;

namespace PokemonGame.ViewModels.Translators
{
    public class AbilityTranslator
    {
        private readonly IAbilityService _abilityService;
        private readonly MoveTranslator _moveTranslator;

        public AbilityTranslator()
        {
            _abilityService = new AbilityService();
            _moveTranslator = new MoveTranslator();
        }

        public AbilityTranslator(IAbilityService abilityService, MoveTranslator moveTranslator)
        {
            _abilityService = abilityService;
            _moveTranslator = moveTranslator;
        }

        // ── Public entry points ──────────────────────────────────────────────

        public AbilityState Translate(string abilityName)
        {
            var tree = _abilityService.GetAbility(abilityName)
                ?? throw new InvalidOperationException($"Ability '{abilityName}' not found.");

            return BuildAbilityState(tree);
        }

        public AbilityState TranslateById(int id)
        {
            var tree = _abilityService.GetAbilityById(id)
                ?? throw new InvalidOperationException($"Ability with id '{id}' not found.");

            return BuildAbilityState(tree);
        }

        // ── Builder ──────────────────────────────────────────────────────────

        private AbilityState BuildAbilityState(AbilityTree tree)
        {
            var domain = new AbillityDomain
            {
                Name = tree.Name,
                Description = tree.Description,
                Used = false,
            };

            ICondition<BattleState> condition = tree.Condition != null
             ? TranslateCondition(tree.Condition)
             : new Probability<BattleState>(1.0);   // always passes

            IEffect effect = tree.Effect != null
                ? TranslateEffect(tree.Effect)
                : new NoEffect();

            return new AbilityState(domain, effect);
        }

        // ── Condition ────────────────────────────────────────────────────────
        // Handles ability-specific conditions first, then falls back to
        // MoveTranslator for shared types (Probability, HasStatus, And/Or/Not…).

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
            "ContactHit" => new WasHitByContact(),
            // ── Parameterized ────────────────────────────────────────────────
            "WasHitByMoveType" => new WasHitByMoveType(ParseEnum<PokemonType>(c.PokemonType!)),
            "IsWeatherActive" => new IsWeatherActive(ParseEnum<Weather>(c.Weather!)),
            "IsTerrainActive" => new IsTerrainActive(ParseEnum<TerrainType>(c.Terrain!)),
            "MoveHasTag" => new MoveHasTag(ParseEnum<MoveTag>(c.MoveTag!)),
            "MoveIsCategory" => new MoveIsCategory(ParseEnum<MoveCategory>(c.MoveCategory!)),
            "HasStatus" => new HasStatus(ParseEnum<StatusCondition>(c.Status!)),
            "HasVolatile" => new HasVolatile(ParseEnum<VolatileStatus>(c.VolatileStatus!)),
            "HasType" => new HasType(ParseEnum<PokemonType>(c.PokemonType!)),
            "HPBelow" => new HPBelow(c.HpFraction!.Value),
            "Probability" => new Probability(c.Probability!.Value),
            //"HasBeenCrit"

            // ── Combinators ──────────────────────────────────────────────────
            "And" => new And<BattleState>(TranslateCondition(c.Left!), TranslateCondition(c.Right!)),
            "Or" => new Or<BattleState>(TranslateCondition(c.Left!), TranslateCondition(c.Right!)),
            "Not" => new Not<BattleState>(TranslateCondition(c.Inner!)),

            // ── Adapters ─────────────────────────────────────────────────────
            "UserCondition" => new UserCondition(TranslatePokemonCondition(c.Inner!)),
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

            // ── The Safety Net ──
            _ => HandleUnknownCondition(c.Type)
        };

        private ICondition<PokemonState> HandleUnknownCondition(string type)
        {
            // Log the error so you know to fix it in the DB or C# later
            Console.WriteLine($"[WARNING] Condition type '{type}' is not implemented. Defaulting to AlwaysFalse.");

            // Return a condition that simply fails so the game doesn't crash
            return new AlwaysFalseCondition<PokemonState>();
        }

        // ── Effect ───────────────────────────────────────────────────────────
        // Handles ability-specific effects first, then falls back to MoveTranslator.

        public IEffect TranslateEffect(MoveEffect e) => e.Type switch
        {
            // ── Ability-exclusive ────────────────────────────────────────────
            "ModifyDamageDealt" => new ModifyDamageDealt(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "DamageOnAttack" => new DamageOnAttack(ResolveTarget(e.Target), e.Multiplier ?? 0.0),
            "ModifySpeedMultiplier" => new ModifySpeedMultiplier(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifyAccuracy" => new ModifyAccuracy(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifyEvasion" => new ModifyEvasion(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
            "ModifyCritRatio" => new ModifyCritRatio(ResolveTarget(e.Target), e.StatStages ?? 0),
            "ModifyPriority" => new ModifyPriority(ResolveTarget(e.Target), e.ChanceProbability ?? 1.0),
            "ChanceFlinch" => new ChanceFlinch(e.ChanceProbability ?? 0),
            "ResetNegativeStatStages" => new ResetNegativeStatStages(ResolveTarget(e.Target)),
            "LockMove" => new LockMove(ResolveTarget(e.Target)),
            "CureVolatile" => new CureVolatile(ResolveTarget(e.Target), ParseEnum<VolatileStatus>(e.Status!)),
            "CureSpecificStatus" => new CureSpecificStatus(ResolveTarget(e.Target), ParseEnum<StatusCondition>(e.Stat!)),
            "SkipChargeTurn" => new SkipChargeTurn(ResolveTarget(e.Target)),

            // ── Combinators (recursive, ability-aware) ───────────────────────
            "Sequence" => new Sequence(e.SequenceSteps.Select(TranslateEffect).ToList()),
            "Chance" => new Chance(e.ChanceProbability ?? 0, TranslateEffect(e.ChanceChild!)),
            "Conditional" => new Conditional(
                                 TranslateCondition(e.Condition!),
                                 TranslateEffect(e.OnPass!),
                                 e.OnFail != null ? TranslateEffect(e.OnFail) : null),

            // ── Shared with moves — delegate ─────────────────────────────────
            _ => _moveTranslator.TranslateEffect(e)
        };

        // ── Helpers ──────────────────────────────────────────────────────────

        private static ITarget ResolveTarget(string? target) => target switch
        {
            "Attacker" => new AttackerTarget(),
            "Defender" => new DefenderTarget(),
            _ => new AttackerTarget()  // abilities default to self
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