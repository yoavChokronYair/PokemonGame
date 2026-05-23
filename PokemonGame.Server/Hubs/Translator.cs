using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Server.Hubs
{
    public class AbilityTranslator : BaseTranslator
    {
        private readonly IAbilityService _abilityService;

        public AbilityTranslator()
        {
            _abilityService = new LocalAbilityService();
        }

        public AbilityTranslator(IAbilityService abilityService)
        {
            _abilityService = abilityService;
        }

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

        private AbilityState BuildAbilityState(AbilityTree tree)
        {
            ICondition<BattleState> condition = tree.Condition != null
                ? TranslateCondition(tree.Condition)
                : new Probability<BattleState>(1.0);

            IEffect effect = tree.Effect != null
                ? TranslateEffect(tree.Effect)
                : new NoEffect();

            return new AbilityState(tree.Name, effect, tree.Description);
        }
    }
    public abstract class BaseTranslator
    {
        // ── Effect ───────────────────────────────────────────────────────────

        public virtual IEffect TranslateEffect(MoveEffect e)
        {
            if (e == null) return new NoEffect();
            return e.Type switch
            {
                "Sequence" => new Sequence(e.SequenceSteps.Select(TranslateEffect).ToList()),
                "Chance" => new Chance(e.ChanceProbability ?? 0, TranslateEffect(e.ChanceChild!)),
                "Conditional" => new Conditional(
                                     TranslateCondition(e.Condition!),
                                     TranslateEffect(e.OnPass!),
                                     e.OnFail != null ? TranslateEffect(e.OnFail) : null),

                "FormulaDamage" => new FormulaDamage(ResolveTarget(e.Target), TranslateNumber(e.Number!)),
                "DirectDamage" => new DirectDamage(ResolveTarget(e.Target), TranslateNumber(e.Number!)),
                "CrashDamage" => new CrashDamage(ResolveTarget(e.Target), TranslateNumber(e.Number!)),
                "Recoil" => new Recoil(ResolveTarget(e.Target), TranslateNumber(e.Number!)),
                "Drain" => new Drain(ResolveTarget(e.Target), ResolveTarget(e.HealTarget), TranslateNumber(e.Number!)),
                "RestoreHP" => new RestoreHP(ResolveTarget(e.Target), TranslateNumber(e.Number!)),
                "OHKO" => new OHKO(ResolveTarget(e.Target)),
                "Faint" => new Faint(ResolveTarget(e.Target)),
                "PowerUpMove" => new PowerUpMove(e.Multiplier ?? 1.0),
                "Paralyze" => new Paralyze(ResolveTarget(e.Target)),
                "Burn" => new Burn(ResolveTarget(e.Target)),
                "Poison" => new Poison(ResolveTarget(e.Target), e.IsToxic),
                "Sleep" => new Sleep(ResolveTarget(e.Target), e.SleepMinTurns ?? 1, e.SleepMaxTurns ?? 3),
                "Freeze" => new Freeze(ResolveTarget(e.Target)),
                "Confuse" => new Confuse(ResolveTarget(e.Target), e.ConfuseMinTurns ?? 1, e.ConfuseMaxTurns ?? 4),
                "Flinch" => new Flinch(ResolveTarget(e.Target)),

                "StatChange" => new StatChange(ResolveTarget(e.Target), ParseEnum<Stat>(e.Stat!), e.StatStages ?? 0),
                "MultiStatChange" => new MultiStatChange(
                                         ResolveTarget(e.Target),
                                         e.StatChanges.Select(s => (ParseEnum<Stat>(s.Stat), s.Stages)).ToList()),
                "ResetStats" => new ResetStats(ResolveTarget(e.Target)),

                "SetWeather" => new SetWeather(ParseEnum<Weather>(e.Weather!), e.WeatherTurns ?? 5),
                "SetScreen" => new SetScreen(
                                    ParseEnum<BattleSide>(e.BattleSide!),
                                    ParseEnum<Screen>(e.Screen!),
                                    e.ScreenTurns ?? 5),
                "SetHazard" => new SetHazard(
                                    ParseEnum<BattleSide>(e.BattleSide!),
                                    ParseEnum<Hazard>(e.Hazard!)),

                "ForceSwitch" => new ForceSwitch(ResolveTarget(e.Target)),
                "CureStatus" => new CureStatus(ResolveTarget(e.Target)),
                "CopyLastMove" => new CopyLastMove(ResolveTarget(e.Target)),
                "StoreAndRelease" => new StoreAndRelease(ResolveTarget(e.Target), e.ChargeTurns ?? 2),
                "NoEffect" => new NoEffect(),
                "ModifyDamageDealt" => new ModifyDamageDealt(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
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
                "DamageOnAttack" => new DamageOnAttack(ResolveTarget(e.Target), e.Multiplier ?? 0.0),
                "Immune" => new Immune(ResolveTarget(e.Target), e.Stat != null ? ParseEnum<StatusCondition>(e.Stat) : null),
                "BlockCritical" => new BlockCritical(ResolveTarget(e.Target)),
                "PreventStatReduction" => new PreventStatReduction(ResolveTarget(e.Target), e.Stat != null ? ParseEnum<Stat>(e.Stat) : null),
                "BlockSecondaryEffects" => new BlockSecondaryEffects(ResolveTarget(e.Target)),
                "BlockRecoil" => new BlockRecoil(ResolveTarget(e.Target)),
                "BlockIndirectDamage" => new BlockIndirectDamage(ResolveTarget(e.Target)),
                "Endure" => new Endure(ResolveTarget(e.Target)),
                "SuperEffectiveOnly" => new SuperEffectiveOnly(ResolveTarget(e.Target)),
                "ModifyStatStages" => new ModifyStatStages(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
                "IgnoreStatChanges" => new IgnoreStatChanges(ResolveTarget(e.Target)),
                "MaxMultiStrike" => new MaxMultiStrike(ResolveTarget(e.Target)),
                "NormalizeType" => new NormalizeType(ResolveTarget(e.Target)),
                "PreventFlee" => new PreventFlee(ResolveTarget(e.Target)),
                "PreventSwitch" => new PreventSwitch(ResolveTarget(e.Target)),
                "PreventItemTheft" => new PreventItemTheft(ResolveTarget(e.Target)),
                "WeatherTransform" => new WeatherTransform(ResolveTarget(e.Target)),
                "GuaranteedFlee" => new GuaranteedFlee(ResolveTarget(e.Target)),
                "ModifySleepTurns" => new ModifySleepTurns(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
                "Truant" => new Truant(ResolveTarget(e.Target)),
                "SlowStart" => new SlowStart(ResolveTarget(e.Target), e.Multiplier ?? 0.5, e.ChargeTurns ?? 5),
                "DoublePPUsage" => new DoublePPUsage(ResolveTarget(e.Target)),
                "IgnoreAbility" => new IgnoreAbility(ResolveTarget(e.Target)),
                "GenderRivalry" => new GenderRivalry(ResolveTarget(e.Target)),
                "BlockMove" => new BlockMove(ResolveTarget(e.Target)),
                "SuppressWeather" => new SuppressWeather(ResolveTarget(e.Target)),
                "MoveLastPriority" => new MoveLastPriority(ResolveTarget(e.Target)),
                "TypeChange" => new TypeChange(ResolveTarget(e.Target)),
                "CopyAbility" => new CopyAbility(ResolveTarget(e.Target)),
                "PassStatus" => new PassStatus(ResolveTarget(e.Target)),
                "DamageRedirect" => new DamageRedirect(ResolveTarget(e.Target)),
                "ModifyChance" => new ModifyChance(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
                "InspectOpponent" => new InspectOpponent(ResolveTarget(e.Target)),
                _ => HandleUnknownEffect(e.Type)
            };
        }
        private IEffect HandleUnknownEffect(string? type)
        {
            // Log the error to your console so you know what's missing
            Console.WriteLine($"[Warning] Unknown effect type encountered: '{type ?? "NULL"}'. Defaulting to NoEffect.");

            // Return a safe fallback so the game keeps running
            return new NoEffect();
        }

        // ── Number ───────────────────────────────────────────────────────────

        public INumber TranslateNumber(MoveNumber n) => n.Type switch
        {
            "Exactly" => new Exactly(n.ExactValue ?? 0),
            "Between" => new Between(n.RangeMin ?? 0, n.RangeMax ?? 0),
            "Weighted" => new Weighted(n.WeightedEntries.Select(e => (e.Value, e.Weight)).ToList()),
            "Product" => new Product(TranslateNumber(n.Left!), TranslateNumber(n.Right!)),
            "Sum" => new Sum(TranslateNumber(n.Left!), TranslateNumber(n.Right!)),
            "Quotient" => new Quotient(TranslateNumber(n.Left!), TranslateNumber(n.Right!)),
            "MaxHP" => new MaxHP(ResolveTarget(n.Target)),
            "CurrentHP" => new CurrentHP(ResolveTarget(n.Target)),
            "Level" => new Level(ResolveTarget(n.Target)),
            "LastDamageDealt" => new LastDamageDealt(ResolveTarget(n.Target)),
            _ => throw new NotSupportedException($"Unknown number type: '{n.Type}'")
        };

        // ── Condition ────────────────────────────────────────────────────────

        public virtual ICondition<BattleState> TranslateCondition(MoveCondition? c)
        {
            // If no condition is defined in the DB, the check always passes.
            if (c == null) return new Probability(1);

            return c.Type switch
            {
                // --- Core Logic & Environment ---
                "Probability" => new Probability(c.Probability ?? 0),
                "IsWeatherActive" => new IsWeatherActive(ParseEnum<Weather>(c.Weather!)),
                "IsTerrainActive" => new IsTerrainActive(ParseEnum<TerrainType>(c.Terrain!)),
                "IsAnyTerrainActive" => new IsAnyTerrainActive(),


                // --- Move Context Conditions ---
                "WasHitByContact" => new WasHitByContact(),
                "WasHitByMoveType" => new WasHitByMoveType(ParseEnum<PokemonType>(c.PokemonType!)),
                "MoveHasTag" => new MoveHasTag(ParseEnum<MoveTag>(c.MoveTag!)),
                "MoveIsCategory" => new MoveIsCategory(ParseEnum<MoveCategory>(c.MoveCategory!)),
                "ContactHit" => new MoveIsCategory(ParseEnum<MoveCategory>(c.MoveCategory!)),

                // --- Attacker State Conditions (ICondition<BattleState>) ---
                "HasStatus" => new HasStatus(ParseEnum<StatusCondition>(c.Status!)),
                "HasAnyStatus" => new HasAnyStatus(),
                "HasVolatile" => new HasVolatile(ParseEnum<VolatileStatus>(c.VolatileStatus!)),
                "IsFainted" => new IsFainted(),
                "IsFullHP" => new IsFullHP(),
                "HPBelow" => new HPBelow(c.HpFraction ?? 0),
                "HasType" => new HasType(ParseEnum<PokemonType>(c.PokemonType!)),
                "IsHoldingItem" => new IsHoldingItem(),
                "TookDamageThisTurn" => new TookDamageThisTurn(),
                "DidKnockoutOpponent" => new DidKnockoutOpponent(),
                "HasBaseStatChanged" => new HasBaseStatChanged(),
                "IsGrounded" => new IsGrounded(),
                "IsBattleOver" => new IsBattleOver(),
                "IsNewPokemon" => new IsNewPokemon(),

                // --- Combinators (Recursive) ---
                "And" => new And<BattleState>(TranslateCondition(c.Left), TranslateCondition(c.Right)),
                "Or" => new Or<BattleState>(TranslateCondition(c.Left), TranslateCondition(c.Right)),
                "Not" => new Not<BattleState>(TranslateCondition(c.Inner)),

                // --- Context Switching (Adapters) ---
                // UserCondition wraps an ICondition<BattleState> (usually checking the Attacker)
                "UserCondition" => new UserCondition(TranslateCondition(c.Inner)),

                // OpponentCondition wraps an ICondition<PokemonState> to check the Defender
                "OpponentCondition" => new OpponentCondition(TranslatePokemonCondition(c.Inner!)),

                _ => throw new NotSupportedException($"Unknown condition type: '{c.Type}'")
            };
        }

        public virtual ICondition<PokemonState> TranslatePokemonCondition(MoveCondition c) => c.Type switch
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
            "Probability" => new ProbabilityPokemon(c.Probability ?? 0),
            _ => throw new NotSupportedException($"Unknown pokemon condition type: '{c.Type}'")
        };

        // ── Helpers ──────────────────────────────────────────────────────────

        protected virtual ITarget ResolveTarget(string? target) => target switch
        {
            "Attacker" => new AttackerTarget(),
            "Defender" => new DefenderTarget(),
            _ => new DefenderTarget()
        };

        protected static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum
        {
            if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
                return result;

            throw new InvalidOperationException($"Cannot parse '{value}' as {typeof(TEnum).Name}");
        }
    }
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
            _ => throw new NotSupportedException($"Unknown effect type: '{e.Type}'")
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
    public class MoveTranslator : BaseTranslator
    {
        private readonly IMoveService _moveService;

        public MoveTranslator()
        {
            _moveService = new LocalMoveService();
        }
        public MoveTranslator(IMoveService moveService)
        {
            _moveService = moveService;
        }
        // ── Public entry point ───────────────────────────────────────────────

        public IMove Translate(string moveName)
        {
            var tree = _moveService.GetMove(moveName)
                ?? throw new InvalidOperationException($"Move '{moveName}' not found.");

            if (tree.Attempts.Count == 0)
                throw new InvalidOperationException($"Move '{moveName}' has no attempts.");

            var rootAttemptData = tree.Attempts[0];
            IAttempt translatedAttempt = TranslateAttempt(rootAttemptData);
            var move = tree.Move;

            var moveState = new MoveState(
                attempt: translatedAttempt,
                name: move.Name,
                element: ParseEnum<PokemonType>(move.Element),
                category: ParseEnum<MoveCategory>(move.Category),
                pp: move.PP,
                target: ParseEnum<MoveTarget>(move.Target),
                priority: move.Priority,
                critStage: move.CritStage,
                description: move.Description
            );

            // ← wrap in decorators before returning
            return ApplyDecorators(moveState, tree.Decorators);
        }


        public IAttempt TranslateAttempt(MoveAttempt a) => a.Type switch
        {
            "Attempt" => new Attempt(
                accuracy: new Probability(a.AccuracyValue ?? 1.0),
                onHit: a.OnHit != null ? TranslateEffect(a.OnHit) : null,
                onMiss: a.OnMiss != null ? TranslateEffect(a.OnMiss) : null,
                after: a.After != null ? TranslateEffect(a.After) : null),

            "Cascade" => new Cascade(
                attempts: a.CascadeSteps.Select(TranslateAttempt).ToList(),
                stopOnMiss: a.StopOnMiss),

            "Combo" => new Combo(
                accuracy: new Probability(a.AccuracyValue ?? 1.0),
                hits: TranslateNumber(a.HitsNumber!),
                onEachHit: a.OnHit != null ? TranslateEffect(a.OnHit) : new NoEffect(),
                onEachMiss: a.OnMiss != null ? TranslateEffect(a.OnMiss) : null,
                after: a.After != null ? TranslateEffect(a.After) : null),

            "Charge" => new Charge(
                chargeEffect: a.ChargeEffect != null ? TranslateEffect(a.ChargeEffect) : new NoEffect(),
                releaseAttempt: TranslateAttempt(a.ReleaseAttempt!)),

            "Rampage" => new Rampage(
                attack: TranslateAttempt(a.CascadeSteps[0]),
                afterRampage: a.AfterRampage != null ? TranslateEffect(a.AfterRampage) : new NoEffect(),
                minTurns: a.RampageMinTurns ?? 2,
                maxTurns: a.RampageMaxTurns ?? 3),

            _ => throw new NotSupportedException($"Unknown attempt type: '{a.Type}'")
        };

        public IMove ApplyDecorators(IMove move, IReadOnlyList<MoveDecorator> decorators)
        {
            foreach (var d in decorators)
            {
                move = d.Type switch
                {
                    "Precondition" => new WithPrecondition(
                        TranslateCondition(d.Condition!),
                        move,
                        d.FailMessage),

                    "Applicability" => new WithApplicability(
                        TranslatePokemonCondition(d.PokemonCondition!),
                        move,
                        d.FailMessage),

                    "Disable" => new WithDisable(
                        move,
                        d.LockTurns ?? 0),

                    "TypeOverride" => new WithTypeOverride(
                        move,
                        ParseEnum<PokemonType>(d.OverrideType!)),

                    "FollowUp" => new WithFollowUp(
                        move,
                        TranslateEffect(d.FollowUpEffect!)),

                    _ => move
                };
            }
            return move;
        }

    }
    public class TeamTranslator
    {
        private readonly IPokemonService _pokemonService;
        private readonly MoveTranslator _moveTranslator;
        private readonly AbilityTranslator _abilityTranslator;
        private readonly ItemTranslator _itemTranslator;
        private readonly TeamCreationManager _teamCreator;

        public TeamTranslator()
        {
            _moveTranslator = new MoveTranslator();
            _abilityTranslator = new AbilityTranslator();
            _itemTranslator = new ItemTranslator();
            _pokemonService = new LocalPokemonService();
            _teamCreator = new TeamCreationManager();
        }

        public TeamTranslator(IPokemonService pokemonService, MoveTranslator moveTranslator,
                              AbilityTranslator abilityTranslator, ItemTranslator itemTranslator)
        {
            _pokemonService = pokemonService;
            _moveTranslator = moveTranslator;
            _abilityTranslator = abilityTranslator;
            _itemTranslator = itemTranslator;
            _teamCreator = new TeamCreationManager();
        }

        public PokemonTeam LoadTeamByID(int battlePlayerId)
        {
            var results = _pokemonService.LoadTeamResults(battlePlayerId);

            if (results == null || results.Count == 0)
                throw new InvalidOperationException($"No team found for Player ID {battlePlayerId}.");

            var roster = results.Select(ToCreationData).ToList();
            return _teamCreator.BuildTeam(roster);
        }

        public PokemonState TranslateToDomain(PokemonLoadResult result) =>
            _teamCreator.BuildPokemon(ToCreationData(result));

        // ── Mapping ──────────────────────────────────────────────────────────
        private PokemonCreationData ToCreationData(PokemonLoadResult result)
        {
            var b = result.Battler;
            var g = result.General;
            var s = result.Stats;

            return new PokemonCreationData
            {
                Name = g.Name ?? "MissingNo",
                PokedexId = g.PokedexID,
                Type1 = g.Type1 ?? "Normal",
                Type2 = g.Type2,
                Level = b.Level,
                Nature = b.Nature ?? "Serious",

                BaseHp = s.HP,
                BaseAtk = s.Attack,
                BaseDef = s.Defense,
                BaseSpAtk = s.SpAtk,
                BaseSpDef = s.SpDef,
                BaseSpeed = s.Speed,

                IvHp = b.Iv_hp,
                IvAtk = b.Iv_atk,
                IvDef = b.Iv_def,
                IvSpAtk = b.Iv_spAtk,
                IvSpDef = b.Iv_spDef,
                IvSpeed = b.Iv_speed,

                EvHp = b.Ev_hp,
                EvAtk = b.Ev_atk,
                EvDef = b.Ev_def,
                EvSpAtk = b.Ev_spAtk,
                EvSpDef = b.Ev_spDef,
                EvSpeed = b.Ev_speed,

                Moves = result.MoveNames
                        .Where(m => !string.IsNullOrEmpty(m))
                        .Select(_moveTranslator.Translate) // Direct reference to the method
                        .ToList(),
                Ability = _abilityTranslator.TranslateById(b.AbilityID),
                HeldItem = null,
            };
        }
    }
}

