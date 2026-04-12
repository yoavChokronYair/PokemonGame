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

        public virtual IEffect TranslateEffect(MoveEffect e) => e.Type switch
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

            _ => throw new NotSupportedException($"Unknown effect type: '{e.Type}'")
        };

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

        public virtual ICondition<BattleState> TranslateCondition(MoveCondition c) => c.Type switch
        {
            "Probability" => new Probability(c.Probability ?? 0),
            "HasStatus" => new HasStatus(ParseEnum<StatusCondition>(c.Status!)),
            "HasVolatile" => new HasVolatile(ParseEnum<VolatileStatus>(c.VolatileStatus!)),
            "IsFainted" => new IsFainted(),
            "IsFullHP" => new IsFullHP(),
            "HPBelow" => new HPBelow(c.HpFraction ?? 0),
            "HasType" => new HasType(ParseEnum<PokemonType>(c.PokemonType!)),
            "IsWeatherActive" => new IsWeatherActive(ParseEnum<Weather>(c.Weather!)),
            "And" => new And<BattleState>(TranslateCondition(c.Left!), TranslateCondition(c.Right!)),
            "Or" => new Or<BattleState>(TranslateCondition(c.Left!), TranslateCondition(c.Right!)),
            "Not" => new Not<BattleState>(TranslateCondition(c.Inner!)),
            "UserCondition" => new UserCondition(TranslatePokemonCondition(c.Inner!)),
            "OpponentCondition" => new OpponentCondition(TranslatePokemonCondition(c.Inner!)),
            _ => throw new NotSupportedException($"Unknown battle condition type: '{c.Type}'")
        };

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
