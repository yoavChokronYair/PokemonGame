// Design: Command pattern — each IEffect is a self-contained battle action.
// Design: Composite pattern — Sequence and Conditional compose multiple effects.
// Covers: damage (formula, direct, OHKO), drain, recoil, HP restore, status, stat changes,
//         field effects (weather/screens/hazards), utility (ForceSwitch, CureStatus, CopyMove).
// Layer: Domain/Move — concrete effect implementations.
// IEffect interface lives in Interface/Move/IEffect.cs.
// NOTE: Chance uses RandomHelper.NextBool — no inline new Random() in this file.

using PokemonGame.Enums.Battle;
using PokemonGame.Interface.Move;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Domain.Move
{
    #region Combinators

    internal class Sequence : IEffect
    {
        private readonly List<IEffect> effects;
        public Sequence(List<IEffect> effects) { this.effects = effects; }
        public Sequence(params IEffect[] effects) { this.effects = new List<IEffect>(effects); }
        public void Apply(BattleState battle) { foreach (var effect in effects) effect.Apply(battle); }
    }

    internal class Conditional : IEffect
    {
        private readonly ICondition<BattleState> condition;
        private readonly IEffect onPass;
        private readonly IEffect? onFail;

        public Conditional(ICondition<BattleState> condition, IEffect onPass, IEffect? onFail = null)
        {
            this.condition = condition;
            this.onPass = onPass;
            this.onFail = onFail;
        }

        public void Apply(BattleState battle)
        {
            if (condition.Check(battle)) onPass.Apply(battle);
            else onFail?.Apply(battle);
        }
    }

    // Applies effect with a percentage chance — e.g. 30% burn on Fire Blast.
    // Uses RandomHelper.NextBool — no new Random() here.
    internal class Chance : IEffect
    {
        private readonly double probability;
        private readonly IEffect effect;

        public Chance(double probability, IEffect effect)
        {
            this.probability = probability;
            this.effect = effect;
        }

        public void Apply(BattleState battle)
        {
            if (RandomHelper.NextBool(probability))
                effect.Apply(battle);
        }
    }

    #endregion

    internal class NoEffect : IEffect
    {
        public void Apply(BattleState battle) { }
    }

    // ── Damage ────────────────────────────────────────────────────────────────

    internal class FormulaDamage : IEffect
    {
        private readonly ITarget target;
        private readonly INumber power;

        public FormulaDamage(ITarget target, INumber power) { this.target = target; this.power = power; }

        public void Apply(BattleState battle)
        {
            var defender = target.Resolve(battle);
            int amount = (int)power.Evaluate(battle);
            defender.TakeDamage(amount);
            battle.Attacker.RegisterDamageDealt(amount);
            battle.LastDamageDealt = amount;
        }
    }

    // Fixed damage ignoring stats — e.g. Seismic Toss, Dragon Rage.
    internal class DirectDamage : IEffect
    {
        private readonly ITarget target;
        private readonly INumber amount;

        public DirectDamage(ITarget target, INumber amount) { this.target = target; this.amount = amount; }

        public void Apply(BattleState battle)
        {
            var defender = target.Resolve(battle);
            int amt = (int)this.amount.Evaluate(battle);
            defender.TakeDamage(amt);
            battle.Attacker.RegisterDamageDealt(amt);
            battle.LastDamageDealt = amt;
        }
    }

    // One-hit KO — Fissure, Horn Drill, Guillotine.
    internal class OHKO : IEffect
    {
        private readonly ITarget target;
        public OHKO(ITarget target) { this.target = target; }
        public void Apply(BattleState battle)
        {
            var battler = target.Resolve(battle);
            battler.TakeDamage(battler.CurrentHP);
        }
    }

    // Damage that also heals the user — Drain Punch, Giga Drain.
    internal class Drain : IEffect
    {
        private readonly ITarget damageTarget;
        private readonly ITarget healTarget;
        private readonly INumber drainAmount;

        public Drain(ITarget damageTarget, ITarget healTarget, INumber drainAmount)
        {
            this.damageTarget = damageTarget;
            this.healTarget = healTarget;
            this.drainAmount = drainAmount;
        }

        public void Apply(BattleState battle)
        {
            var victim = damageTarget.Resolve(battle);
            var user = healTarget.Resolve(battle);
            int amount = (int)drainAmount.Evaluate(battle);
            victim.TakeDamage(amount);
            user.RestoreHP(amount);
        }
    }

    // Self-damage on miss — High Jump Kick crash, Jump Kick.
    internal class CrashDamage : IEffect
    {
        private readonly ITarget target;
        private readonly INumber amount;
        public CrashDamage(ITarget target, INumber amount) { this.target = target; this.amount = amount; }
        public void Apply(BattleState battle) => target.Resolve(battle).TakeDamage((int)amount.Evaluate(battle));
    }

    // Recoil damage to the user — Double-Edge, Flare Blitz.
    internal class Recoil : IEffect
    {
        private readonly ITarget target;
        private readonly INumber amount;
        public Recoil(ITarget target, INumber amount) { this.target = target; this.amount = amount; }
        public void Apply(BattleState battle) => target.Resolve(battle).TakeDamage((int)amount.Evaluate(battle));
    }

    // ── HP ────────────────────────────────────────────────────────────────────

    internal class RestoreHP : IEffect
    {
        private readonly ITarget target;
        private readonly INumber amount;
        public RestoreHP(ITarget target, INumber amount) { this.target = target; this.amount = amount; }
        public void Apply(BattleState battle) => target.Resolve(battle).RestoreHP((int)amount.Evaluate(battle));
    }

    internal class Faint : IEffect
    {
        private readonly ITarget target;
        public Faint(ITarget target) { this.target = target; }
        public void Apply(BattleState battle)
        {
            var battler = target.Resolve(battle);
            battler.TakeDamage(battler.CurrentHP);
        }
    }

    // ── Status Conditions ─────────────────────────────────────────────────────

    internal class Paralyze : IEffect
    {
        private readonly ITarget target;
        public Paralyze(ITarget target) { this.target = target; }
        public void Apply(BattleState battle) => target.Resolve(battle).ApplyStatus(StatusCondition.Paralysis);
    }

    internal class Burn : IEffect
    {
        private readonly ITarget target;
        public Burn(ITarget target) { this.target = target; }
        public void Apply(BattleState battle) => target.Resolve(battle).ApplyStatus(StatusCondition.Burn);
    }

    internal class Poison : IEffect
    {
        private readonly ITarget target;
        private readonly bool toxic;
        public Poison(ITarget target, bool toxic = false) { this.target = target; this.toxic = toxic; }
        public void Apply(BattleState battle)
            => target.Resolve(battle).ApplyStatus(toxic ? StatusCondition.Toxic : StatusCondition.Poison);
    }

    // Uses RandomHelper for sleep duration — no inline new Random().
    internal class Sleep : IEffect
    {
        private readonly ITarget target;
        private readonly Between turns;

        public Sleep(ITarget target, int minTurns = 1, int maxTurns = 3)
        {
            this.target = target;
            this.turns = new Between(minTurns, maxTurns);
        }

        public void Apply(BattleState battle)
        {
            var battler = target.Resolve(battle);
            int duration = (int)turns.Evaluate(battle);
            battler.ApplyStatus(StatusCondition.Sleep, duration);
        }
    }

    internal class Freeze : IEffect
    {
        private readonly ITarget target;
        public Freeze(ITarget target) { this.target = target; }
        public void Apply(BattleState battle) => target.Resolve(battle).ApplyStatus(StatusCondition.Freeze);
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

        public void Apply(BattleState battle)
        {
            int duration = (int)turns.Evaluate(battle);
            target.Resolve(battle).ApplyVolatileStatus(VolatileStatus.Confusion, duration);
        }
    }

    internal class Flinch : IEffect
    {
        private readonly ITarget target;
        public Flinch(ITarget target) { this.target = target; }
        public void Apply(BattleState battle) => target.Resolve(battle).ApplyVolatileStatus(VolatileStatus.Flinch);
    }

    // ── Stat Changes ──────────────────────────────────────────────────────────

    internal class StatChange : IEffect
    {
        private readonly ITarget target;
        private readonly Stat stat;
        private readonly int stages;
        public StatChange(ITarget target, Stat stat, int stages) { this.target = target; this.stat = stat; this.stages = stages; }
        public void Apply(BattleState battle) => target.Resolve(battle).ChangeStatStage(stat, stages);
    }

    internal class MultiStatChange : IEffect
    {
        private readonly ITarget target;
        private readonly List<(Stat stat, int stages)> changes;
        public MultiStatChange(ITarget target, List<(Stat stat, int stages)> changes) { this.target = target; this.changes = changes; }
        public void Apply(BattleState battle)
        {
            var battler = target.Resolve(battle);
            foreach (var (stat, stages) in changes)
                battler.ChangeStatStage(stat, stages);
        }
    }

    internal class ResetStats : IEffect
    {
        private readonly ITarget target;
        public ResetStats(ITarget target) { this.target = target; }
        public void Apply(BattleState battle) => target.Resolve(battle).ResetStatStages();
    }

    // ── Field / Battle-wide ───────────────────────────────────────────────────

    internal class SetHazard : IEffect
    {
        private readonly BattleSide side;
        private readonly Hazard hazard;
        public SetHazard(BattleSide side, Hazard hazard) { this.side = side; this.hazard = hazard; }
        public void Apply(BattleState battle) => battle.GetSide(side).AddHazard(hazard);
    }

    internal class SetScreen : IEffect
    {
        private readonly BattleSide side;
        private readonly Screen screen;
        private readonly int turns;
        public SetScreen(BattleSide side, Screen screen, int turns = 5) { this.side = side; this.screen = screen; this.turns = turns; }
        public void Apply(BattleState battle) => battle.GetSide(side).ActivateScreen(screen, turns);
    }

    internal class SetWeather : IEffect
    {
        private readonly Weather weather;
        private readonly int turns;
        public SetWeather(Weather weather, int turns = 5) { this.weather = weather; this.turns = turns; }
        public void Apply(BattleState battle) => battle.WeatherService.SetWeather(weather, turns);
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    internal class ForceSwitch : IEffect
    {
        private readonly ITarget target;
        public ForceSwitch(ITarget target) { this.target = target; }
        public void Apply(BattleState battle) => target.Resolve(battle).ForceSwitch(battle);
    }

    internal class CureStatus : IEffect
    {
        private readonly ITarget target;
        public CureStatus(ITarget target) { this.target = target; }
        public void Apply(BattleState battle) => target.Resolve(battle).ClearStatus();
    }

    internal class CopyLastMove : IEffect
    {
        private readonly ITarget copyFrom;
        public CopyLastMove(ITarget copyFrom) { this.copyFrom = copyFrom; }
        public void Apply(BattleState battle)
        {
            var source = copyFrom.Resolve(battle);
            battle.Attacker.CopyMove(source.LastUsedMove);
        }
    }

    internal class StoreAndRelease : IEffect
    {
        private readonly ITarget target;
        private readonly int chargeTurns;
        public StoreAndRelease(ITarget target, int chargeTurns = 2) { this.target = target; this.chargeTurns = chargeTurns; }
        public void Apply(BattleState battle) => target.Resolve(battle).StartBide(chargeTurns);
    }
}
