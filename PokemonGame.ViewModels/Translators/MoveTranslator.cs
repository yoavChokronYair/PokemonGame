using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Handler;

namespace PokemonGame.ViewModels.Translators
{
    // Sits in the ViewModel layer — knows about both the service's MoveTree
    // and the model's domain objects, translating between the two.
    // Neither the model nor the services know this class exists.
    public class MoveTranslator
    {
        private readonly IMoveService _moveService;

        public MoveTranslator()
        {
            _moveService = new MoveService();
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

        // ── Attempt ──────────────────────────────────────────────────────────
        public IAttempt TranslateAttemptForMove(string moveName)
        {
            var tree = _moveService.GetMove(moveName)
                ?? throw new InvalidOperationException($"Move '{moveName}' not found.");

            if (tree.Attempts.Count == 0)
            {
                throw new InvalidOperationException($"Move '{moveName}' has no attempts.");
            }

            return TranslateAttempt(tree.Attempts[0]);
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

        // ── Effect ───────────────────────────────────────────────────────────

        public IEffect TranslateEffect(MoveEffect e) => e.Type switch
        {
            "Sequence" => new Sequence(e.SequenceSteps.Select(TranslateEffect).ToList()),

            "Chance" => new Chance(
                               e.ChanceProbability ?? 0,
                               TranslateEffect(e.ChanceChild!)),

            "Conditional" => new Conditional(
                               TranslateCondition(e.Condition!),
                               TranslateEffect(e.OnPass!),
                               e.OnFail != null ? TranslateEffect(e.OnFail) : null),

            "FormulaDamage" => new FormulaDamage(ResolveTarget(e.Target), TranslateNumber(e.Number!)),
            "DirectDamage" => new DirectDamage(ResolveTarget(e.Target), TranslateNumber(e.Number!)),
            "CrashDamage" => new CrashDamage(ResolveTarget(e.Target), TranslateNumber(e.Number!)),
            "Recoil" => new Recoil(ResolveTarget(e.Target), TranslateNumber(e.Number!)),
            "Drain" => new Drain(
                                   ResolveTarget(e.Target),
                                   ResolveTarget(e.HealTarget),
                                   TranslateNumber(e.Number!)),
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

            "StatChange" => new StatChange(
                                   ResolveTarget(e.Target),
                                   ParseEnum<Stat>(e.Stat!),
                                   e.StatStages ?? 0),

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
        // In MoveTranslator — add after Translate()

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

                    _ =>  move
                };
            }
            return move;
        }
        // ── Condition ────────────────────────────────────────────────────────

        public ICondition<BattleState> TranslateCondition(MoveCondition c) => c.Type switch
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

        public ICondition<PokemonState> TranslatePokemonCondition(MoveCondition c) => c.Type switch
        {

            "And" => new And<PokemonState>(TranslatePokemonCondition(c.Left!), TranslatePokemonCondition(c.Right!)),
            "Or" => new Or<PokemonState>(TranslatePokemonCondition(c.Left!), TranslatePokemonCondition(c.Right!)),
            "Not" => new Not<PokemonState>(TranslatePokemonCondition(c.Inner!)),
            _ => throw new NotSupportedException($"Unknown pokemon condition type: '{c.Type}'")
        };

        // ── Helpers ──────────────────────────────────────────────────────────

        private static ITarget ResolveTarget(string? target) => target switch
        {
            "Attacker" => new AttackerTarget(),
            "Defender" => new DefenderTarget(),
            _ => new DefenderTarget()   // safe default
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