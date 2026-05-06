using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;
using PokemonGame.Services.Data.GameData.Move;

namespace PokemonGame.ViewModels.Translators
{
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
                "Pickup" => new Pickup(ResolveTarget(e.Target)),
                "GuaranteedFlee" => new GuaranteedFlee(ResolveTarget(e.Target)),
                "ModifySleepTurns" => new ModifySleepTurns(ResolveTarget(e.Target), e.Multiplier ?? 1.0),
                "Truant" => new Truant(ResolveTarget(e.Target)),
                "SlowStart" => new SlowStart(ResolveTarget(e.Target), e.Multiplier ?? 0.5, e.ChargeTurns ?? 5),
                "DoublePPUsage" => new DoublePPUsage(ResolveTarget(e.Target)),
                "IgnoreAbility" => new IgnoreAbility(ResolveTarget(e.Target)),
                "GenderRivalry" => new GenderRivalry(ResolveTarget(e.Target)),
                "BlockMove" => new BlockMove(ResolveTarget(e.Target)),
                "SuppressWeather" => new SuppressWeather(ResolveTarget(e.Target)),
                "MultitypeChange" => new MultitypeChange(ResolveTarget(e.Target)),
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
                "IsBattleOver" => new IsBattleOver(),
                "IsNewPokemon" => new IsNewPokemon(),

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

                // --- Combinators (Recursive) ---
                "And" => new And<BattleState>(TranslateCondition(c.Left), TranslateCondition(c.Right)),
                "Or" => new Or<BattleState>(TranslateCondition(c.Left), TranslateCondition(c.Right)),
                "Not" => new Not<BattleState>(TranslateCondition(c.Inner)),

                // --- Context Switching (Adapters) ---
                // UserCondition wraps an ICondition<BattleState> (usually checking the Attacker)
                "UserCondition" => new UserCondition(TranslateCondition(c.Inner)),

                // OpponentCondition wraps an ICondition<PokemonState> to check the Defender
                "OpponentCondition" => new OpponentCondition(TranslatePokemonCondition(c.Inner!)),

                _ => new NoCondition()
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
}
