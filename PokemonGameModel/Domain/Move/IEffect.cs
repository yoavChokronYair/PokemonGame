using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Domain.Move
{
    //TODO:create all things needed for here
    internal interface IEffect
    {
        public void Apply(BattleDomain battle);
    }

    #region Combinators

     internal class Sequence : IEffect
    {
        private readonly List<IEffect> effects;

        public Sequence(List<IEffect> effects)
        {
            this.effects = effects;
        }

        // Convenience constructor for inline usage: new Sequence(effect1, effect2, ...)
        public Sequence(params IEffect[] effects)
        {
            this.effects = new List<IEffect>(effects);
        }

        public void Apply(BattleDomain battle)
        {
            foreach (var effect in effects)
                effect.Apply(battle);
        }
    }

    internal class Conditional : IEffect
    {
        private readonly ICondition<BattleDomain> condition;
        private readonly IEffect onPass;
        private readonly IEffect? onFail;

        public Conditional(
            ICondition<BattleDomain> condition,
            IEffect onPass,
            IEffect? onFail = null)
        {
            this.condition = condition;
            this.onPass = onPass;
            this.onFail = onFail;
        }

        public void Apply(BattleDomain battle)
        {
            if (condition.Check(battle))
                onPass.Apply(battle);
            else
                onFail?.Apply(battle);
        }
    }

    // Applies effect with a percentage chance — e.g. 30% burn on Fire Blast
    internal class Chance : IEffect
    {
        private readonly double probability; // 0.0 to 1.0
        private readonly IEffect effect;
        private static readonly Random rng = new();

        public Chance(double probability, IEffect effect)
        {
            this.probability = probability;
            this.effect = effect;
        }

        public void Apply(BattleDomain battle)
        {
            if (rng.NextDouble() < probability)
                effect.Apply(battle);
        }
    }

    #endregion

    // Does nothing — useful as a default/null-safe placeholder
    internal class NoEffect : IEffect
    {
        public void Apply(BattleDomain battle) { }
    }

    // ── Damage ────────────────────────────────────────────────────────────────

    // Damage computed from a formula (handles type, stat, STAB, etc. externally)
    internal class FormulaDamage : IEffect
    {
        private readonly ITarget target;
        private readonly INumber power;

        public FormulaDamage(ITarget target, INumber power)
        {
            this.target = target;
            this.power = power;
        }

        public void Apply(BattleDomain battle)
        {
            var defender = target.Resolve(battle);
            int amount = (int)power.Evaluate(battle);
            defender.TakeDamage(amount);
            battle.ActiveUser.RegisterDamageDealt(amount); // track for Drain/Recoil
            battle.LastDamageDealt = amount;               // track on battle for Counter
        }
    }

    // Fixed damage ignoring stats — e.g. Seismic Toss, Dragon Rage
    internal class DirectDamage : IEffect
    {
        private readonly ITarget target;
        private readonly INumber amount;

        public DirectDamage(ITarget target, INumber amount)
        {
            this.target = target;
            this.amount = amount;
        }

        public void Apply(BattleDomain battle)
        {
            var defender = target.Resolve(battle);
            int amount = (int)this.amount.Evaluate(battle);
            defender.TakeDamage(amount);
            battle.ActiveUser.RegisterDamageDealt(amount);
            battle.LastDamageDealt = amount;
        }
    }

    // One-hit KO — Fissure, Horn Drill, Guillotine
    internal class OHKO : IEffect
    {
        private readonly ITarget target;

        public OHKO(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
        {
            var battler = target.Resolve(battle);
            battler.TakeDamage(battler.CurrentHP);
        }
    }

    // Damage that also heals the user — Drain Punch, Giga Drain
    internal class Drain : IEffect
    {
        private readonly ITarget damageTarget;
        private readonly ITarget healTarget;
        private readonly INumber drainAmount; // e.g. new Quotient(lastDamage, new Exactly(2))

        public Drain(ITarget damageTarget, ITarget healTarget, INumber drainAmount)
        {
            this.damageTarget = damageTarget;
            this.healTarget = healTarget;
            this.drainAmount = drainAmount;
        }

        public void Apply(BattleDomain battle)
        {
            var victim = damageTarget.Resolve(battle);
            var user = healTarget.Resolve(battle);
            int amount = (int)drainAmount.Evaluate(battle);
            victim.TakeDamage(amount);
            user.RestoreHP(amount);
        }
    }

    // Self-damage on miss or recoil — High Jump Kick crash, Jump Kick
    internal class CrashDamage : IEffect
    {
        private readonly ITarget target;
        private readonly INumber amount;

        public CrashDamage(ITarget target, INumber amount)
        {
            this.target = target;
            this.amount = amount;
        }

        public void Apply(BattleDomain battle)
        {
            var battler = target.Resolve(battle);
            battler.TakeDamage((int)amount.Evaluate(battle));
        }
    }

    // Recoil damage to the user — Double-Edge, Flare Blitz
    internal class Recoil : IEffect
    {
        private readonly ITarget target;
        private readonly INumber amount; // e.g. Quotient(LastDamageDealt, Exactly(3))

        public Recoil(ITarget target, INumber amount)
        {
            this.target = target;
            this.amount = amount;
        }

        public void Apply(BattleDomain battle)
        {
            var battler = target.Resolve(battle);
            battler.TakeDamage((int)amount.Evaluate(battle));
        }
    }

    // ── HP ────────────────────────────────────────────────────────────────────

    internal class RestoreHP : IEffect
    {
        private readonly ITarget target;
        private readonly INumber amount;

        public RestoreHP(ITarget target, INumber amount)
        {
            this.target = target;
            this.amount = amount;
        }

        public void Apply(BattleDomain battle)
        {
            var battler = target.Resolve(battle);
            battler.RestoreHP((int)amount.Evaluate(battle));
        }
    }

    internal class Faint : IEffect
    {
        private readonly ITarget target;

        public Faint(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
        {
            var battler = target.Resolve(battle);
            battler.TakeDamage(battler.CurrentHP);
        }
    }

    // ── Status Conditions ─────────────────────────────────────────────────────

    internal class Paralyze : IEffect
    {
        private readonly ITarget target;

        public Paralyze(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).ApplyStatus(StatusCondition.Paralysis);
    }

    internal class Burn : IEffect
    {
        private readonly ITarget target;

        public Burn(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).ApplyStatus(StatusCondition.Burn);
    }

    internal class Poison : IEffect
    {
        private readonly ITarget target;
        private readonly bool toxic; // toxic = badly poisoned (damage grows each turn)

        public Poison(ITarget target, bool toxic = false)
        {
            this.target = target;
            this.toxic = toxic;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).ApplyStatus(toxic ? StatusCondition.Toxic : StatusCondition.Poison);
    }

    internal class Sleep : IEffect
    {
        private readonly ITarget target;
        private readonly Between turns; // random sleep duration

        public Sleep(ITarget target, int minTurns = 1, int maxTurns = 3)
        {
            this.target = target;
            this.turns = new Between(minTurns, maxTurns);
        }

        public void Apply(BattleDomain battle)
        {
            var battler = target.Resolve(battle);
            int duration = (int)turns.Evaluate(battle);
            battler.ApplyStatus(StatusCondition.Sleep, duration);
        }
    }

    internal class Freeze : IEffect
    {
        private readonly ITarget target;

        public Freeze(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).ApplyStatus(StatusCondition.Freeze);
    }

    internal class Confuse : IEffect
    {
        private readonly ITarget target;
        private readonly Between turns;

        public Confuse(ITarget target, int minTurns = 1, int maxTurns = 4)
        {
            this.target = target;
            this.turns = new Between(minTurns, maxTurns);
        }

        public void Apply(BattleDomain battle)
        {
            int duration = (int)turns.Evaluate(battle);
            target.Resolve(battle).ApplyVolatileStatus(VolatileStatus.Confusion, duration);
        }
    }

    internal class Flinch : IEffect
    {
        private readonly ITarget target;

        public Flinch(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).ApplyVolatileStatus(VolatileStatus.Flinch);
    }

    // ── Stat Changes ──────────────────────────────────────────────────────────

    internal class StatChange : IEffect
    {
        private readonly ITarget target;
        private readonly Stat stat;
        private readonly int stages; // positive = buff, negative = debuff

        public StatChange(ITarget target, Stat stat, int stages)
        {
            this.target = target;
            this.stat = stat;
            this.stages = stages;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).ChangeStatStage(stat, stages);
    }

    // Changes multiple stats at once — e.g. Swords Dance (+2 Atk), Shell Smash
    internal class MultiStatChange : IEffect
    {
        private readonly ITarget target;
        private readonly List<(Stat stat, int stages)> changes;

        public MultiStatChange(ITarget target, List<(Stat stat, int stages)> changes)
        {
            this.target = target;
            this.changes = changes;
        }

        public void Apply(BattleDomain battle)
        {
            var battler = target.Resolve(battle);
            foreach (var (stat, stages) in changes)
                battler.ChangeStatStage(stat, stages);
        }
    }

    internal class ResetStats : IEffect
    {
        private readonly ITarget target;

        public ResetStats(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).ResetStatStages();
    }

    // ── Field / Battle-wide ───────────────────────────────────────────────────

    // Hazards — Stealth Rock, Spikes, Toxic Spikes
    internal class SetHazard : IEffect
    {
        private readonly BattleSide side;
        private readonly Hazard hazard;

        public SetHazard(BattleSide side, Hazard hazard)
        {
            this.side = side;
            this.hazard = hazard;
        }

        public void Apply(BattleDomain battle)
            => battle.GetSide(side).AddHazard(hazard);
    }

    // Screens — Reflect, Light Screen, Aurora Veil
    internal class SetScreen : IEffect
    {
        private readonly BattleSide side;
        private readonly Screen screen;
        private readonly int turns;

        public SetScreen(BattleSide side, Screen screen, int turns = 5)
        {
            this.side = side;
            this.screen = screen;
            this.turns = turns;
        }

        public void Apply(BattleDomain battle)
            => battle.GetSide(side).ActivateScreen(screen, turns);
    }

    // Weather — Sunny Day, Rain Dance, Sandstorm, Hail
    internal class SetWeather : IEffect
    {
        private readonly Weather weather;
        private readonly int turns;

        public SetWeather(Weather weather, int turns = 5)
        {
            this.weather = weather;
            this.turns = turns;
        }

        public void Apply(BattleDomain battle)
            => battle.SetWeather(weather, turns);
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    // Forces the target to switch out — Roar, Whirlwind, Dragon Tail
    internal class ForceSwitch : IEffect
    {
        private readonly ITarget target;

        public ForceSwitch(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).ForceSwitch(battle);
    }

    // Cure status condition — Aromatherapy, Heal Bell, Lum Berry
    internal class CureStatus : IEffect
    {
        private readonly ITarget target;

        public CureStatus(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).ClearStatus();
    }

    // Copy opponent's last used move — Mimic, Copycat
    internal class CopyLastMove : IEffect
    {
        private readonly ITarget copyFrom;

        public CopyLastMove(ITarget copyFrom)
        {
            this.copyFrom = copyFrom;
        }

        public void Apply(BattleDomain battle)
        {
            var source = copyFrom.Resolve(battle);
            battle.ActiveUser.CopyMove(source.LastUsedMove);
        }
    }

    // Store damage this turn to release next turn — Bide
    internal class StoreAndRelease : IEffect
    {
        private readonly ITarget target;
        private readonly int chargeTurns;

        public StoreAndRelease(ITarget target, int chargeTurns = 2)
        {
            this.target = target;
            this.chargeTurns = chargeTurns;
        }

        public void Apply(BattleDomain battle)
            => target.Resolve(battle).StartBide(chargeTurns);
    }
}