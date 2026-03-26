using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Model.Helper.DesignPatterns
{
    #region Combinators

    public class Sequence : IEffect
    {
        private readonly List<IEffect> _effects;
        public Sequence(List<IEffect> effects) { _effects = effects; }
        public Sequence(params IEffect[] effects) { _effects = new List<IEffect>(effects); }
        public void Apply(BattleState battle)
        {
            foreach (var effect in _effects)
            {
                effect.Apply(battle);
            }
        }
    }

    public class Conditional : IEffect
    {
        private readonly ICondition<BattleState> _condition;
        private readonly IEffect _onPass;
        private readonly IEffect? _onFail;

        public Conditional(ICondition<BattleState> condition, IEffect onPass, IEffect? onFail = null)
        {
            _condition = condition;
            _onPass = onPass;
            _onFail = onFail;
        }

        public void Apply(BattleState battle)
        {
            if (_condition.Check(battle))
            {
                _onPass.Apply(battle);
            }
            else
            {
                _onFail?.Apply(battle);
            }
        }
    }

    public class Chance : IEffect
    {
        private readonly double _probability;
        private readonly IEffect _effect;

        public Chance(double probability, IEffect effect)
        {
            _probability = probability;
            _effect = effect;
        }

        public void Apply(BattleState battle)
        {
            if (RandomHelper.NextBool(_probability))
            {
                _effect.Apply(battle);
            }
        }
    }

    #endregion

    public class NoEffect : IEffect
    {
        public void Apply(BattleState battle) { }
    }

    // ── Damage ────────────────────────────────────────────────────────────────

    public class FormulaDamage : IEffect
    {
        private readonly ITarget _target;
        private readonly INumber _power;

        public FormulaDamage(ITarget target, INumber power) { _target = target; _power = power; }

        public void Apply(BattleState battle)
        {
            var defender = _target.Resolve(battle);
            int amount = (int)_power.Evaluate(battle);
            defender.TakeDamage(amount);
            battle.Attacker.RegisterDamageDealt(amount);
            battle.LastDamageDealt = amount;
        }
    }

    public class DirectDamage : IEffect
    {
        private readonly ITarget _target;
        private readonly INumber _amount;

        public DirectDamage(ITarget target, INumber amount) { _target = target; _amount = amount; }

        public void Apply(BattleState battle)
        {
            var defender = _target.Resolve(battle);
            int amt = (int)_amount.Evaluate(battle);
            defender.TakeDamage(amt);
            battle.Attacker.RegisterDamageDealt(amt);
            battle.LastDamageDealt = amt;
        }
    }

    public class OHKO : IEffect
    {
        private readonly ITarget _target;
        public OHKO(ITarget target) { _target = target; }
        public void Apply(BattleState battle)
        {
            var battler = _target.Resolve(battle);
            battler.TakeDamage(battler.CurrentHP);
        }
    }

    public class Drain : IEffect
    {
        private readonly ITarget _damageTarget;
        private readonly ITarget _healTarget;
        private readonly INumber _drainAmount;

        public Drain(ITarget damageTarget, ITarget healTarget, INumber drainAmount)
        {
            _damageTarget = damageTarget;
            _healTarget = healTarget;
            _drainAmount = drainAmount;
        }

        public void Apply(BattleState battle)
        {
            var victim = _damageTarget.Resolve(battle);
            var user = _healTarget.Resolve(battle);
            int amount = (int)_drainAmount.Evaluate(battle);
            user.TakeDamage(amount);
            victim.RestoreHP(amount);
        }
    }

    public class CrashDamage : IEffect
    {
        private readonly ITarget _target;
        private readonly INumber _amount;
        public CrashDamage(ITarget target, INumber amount) { _target = target; _amount = amount; }
        public void Apply(BattleState battle) => _target.Resolve(battle).TakeDamage((int)_amount.Evaluate(battle));
    }

    public class Recoil : IEffect
    {
        private readonly ITarget _target;
        private readonly INumber _amount;
        public Recoil(ITarget target, INumber amount) { _target = target; _amount = amount; }
        public void Apply(BattleState battle) => _target.Resolve(battle).TakeDamage((int)_amount.Evaluate(battle));
    }

    // ── HP ────────────────────────────────────────────────────────────────────

    public class RestoreHP : IEffect
    {
        private readonly ITarget _target;
        private readonly INumber _amount;
        public RestoreHP(ITarget target, INumber amount) { _target = target; _amount = amount; }
        public void Apply(BattleState battle) => _target.Resolve(battle).RestoreHP((int)_amount.Evaluate(battle));
    }

    public class Faint : IEffect
    {
        private readonly ITarget _target;
        public Faint(ITarget target) { _target = target; }
        public void Apply(BattleState battle)
        {
            var battler = _target.Resolve(battle);
            battler.TakeDamage(battler.CurrentHP);
        }
    }

    // ── Status Conditions ─────────────────────────────────────────────────────

    public class Paralyze : IEffect
    {
        private readonly ITarget _target;
        public Paralyze(ITarget target) { _target = target; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ApplyStatus(StatusCondition.Paralysis);
    }

    public class Burn : IEffect
    {
        private readonly ITarget _target;
        public Burn(ITarget target) { _target = target; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ApplyStatus(StatusCondition.Burn);
    }

    public class Poison : IEffect
    {
        private readonly ITarget _target;
        private readonly bool _toxic;
        public Poison(ITarget target, bool toxic = false) { _target = target; _toxic = toxic; }
        public void Apply(BattleState battle)
            => _target.Resolve(battle).ApplyStatus(_toxic ? StatusCondition.Toxic : StatusCondition.Poison);
    }

    public class Sleep : IEffect
    {
        private readonly ITarget _target;
        private readonly Between _turns;

        public Sleep(ITarget target, int minTurns = 1, int maxTurns = 3)
        {
            _target = target;
            _turns = new Between(minTurns, maxTurns);
        }

        public void Apply(BattleState battle)
        {
            var battler = _target.Resolve(battle);
            int duration = (int)_turns.Evaluate(battle);
            battler.ApplyStatus(StatusCondition.Sleep, duration);
        }
    }

    public class Freeze : IEffect
    {
        private readonly ITarget _target;
        public Freeze(ITarget target) { _target = target; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ApplyStatus(StatusCondition.Freeze);
    }

    public class Confuse : IEffect
    {
        private readonly ITarget _target;
        private readonly Between _turns;

        public Confuse(ITarget target, int minTurns = 1, int maxTurns = 4)
        {
            _target = target;
            _turns = new Between(minTurns, maxTurns);
        }

        public void Apply(BattleState battle)
        {
            int duration = (int)_turns.Evaluate(battle);
            _target.Resolve(battle).ApplyVolatileStatus(VolatileStatus.Confusion, duration);
        }
    }

    public class Flinch : IEffect
    {
        private readonly ITarget _target;
        public Flinch(ITarget target) { _target = target; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ApplyVolatileStatus(VolatileStatus.Flinch);
    }

    // ── Stat Changes ──────────────────────────────────────────────────────────

    public class StatChange : IEffect
    {
        private readonly ITarget _target;
        private readonly Stat _stat;
        private readonly int _stages;
        public StatChange(ITarget target, Stat stat, int stages) { _target = target; _stat = stat; _stages = stages; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ChangeStatStage(_stat, _stages);
    }

    public class MultiStatChange : IEffect
    {
        private readonly ITarget _target;
        private readonly List<(Stat stat, int stages)> _changes;
        public MultiStatChange(ITarget target, List<(Stat stat, int stages)> changes) { _target = target; _changes = changes; }
        public void Apply(BattleState battle)
        {
            var battler = _target.Resolve(battle);
            foreach (var (stat, stages) in _changes)
            {
                battler.ChangeStatStage(stat, stages);
            }
        }
    }

    public class ResetStats : IEffect
    {
        private readonly ITarget _target;
        public ResetStats(ITarget target) { _target = target; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ResetStatStages();
    }

    // ── Field / Battle-wide ───────────────────────────────────────────────────

    public class SetHazard : IEffect
    {
        private readonly BattleSide _side;
        private readonly Hazard _hazard;
        public SetHazard(BattleSide side, Hazard hazard) { _side = side; _hazard = hazard; }
        public void Apply(BattleState battle) => battle.GetSide(_side).AddHazard(_hazard);
    }

    public class SetScreen : IEffect
    {
        private readonly BattleSide _side;
        private readonly Screen _screen;
        private readonly int _turns;
        public SetScreen(BattleSide side, Screen screen, int turns = 5) { _side = side; _screen = screen; _turns = turns; }
        public void Apply(BattleState battle) => battle.GetSide(_side).ActivateScreen(_screen, _turns);
    }

    public class SetWeather : IEffect
    {
        private readonly Weather _weather;
        private readonly int _turns;
        public SetWeather(Weather weather, int turns = 5) { _weather = weather; _turns = turns; }
        public void Apply(BattleState battle) => battle.WeatherService.SetWeather(_weather, _turns);
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    public class ForceSwitch : IEffect
    {
        private readonly ITarget _target;
        public ForceSwitch(ITarget target) { _target = target; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ForceSwitch(battle);
    }

    public class CureStatus : IEffect
    {
        private readonly ITarget _target;
        public CureStatus(ITarget target) { _target = target; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ClearStatus();
    }

    public class CopyLastMove : IEffect
    {
        private readonly ITarget _copyFrom;
        public CopyLastMove(ITarget copyFrom) { _copyFrom = copyFrom; }
        public void Apply(BattleState battle)
        {
            var source = _copyFrom.Resolve(battle);
            battle.Attacker.CopyMove(source.LastUsedMove);
        }
    }

    public class StoreAndRelease : IEffect
    {
        private readonly ITarget _target;
        private readonly int _chargeTurns;
        public StoreAndRelease(ITarget target, int chargeTurns = 2) { _target = target; _chargeTurns = chargeTurns; }
        public void Apply(BattleState battle) => _target.Resolve(battle).StartBide(_chargeTurns);
    }
}