using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.DesignPatterns
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
            int baseAmount = (int)_power.Evaluate(battle);
            int amount = PokemonStatCalculatorHelper.PokemonDamageFormulaCaculator(battle, baseAmount);
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
    public class CureVolatile : IEffect
    {
        // Mental Herb — removes a specific volatile status
        private readonly ITarget _target;
        private readonly VolatileStatus _status;
        public CureVolatile(ITarget target, VolatileStatus status) { _target = target; _status = status; }
        public void Apply(BattleState battle) => _target.Resolve(battle).RemoveVolatileStatus(_status);
    }

    public class SkipChargeTurn : IEffect
    {
        // Power Herb — forces the charge release immediately if the Pokémon is charging
        private readonly ITarget _target;
        public SkipChargeTurn(ITarget target) { _target = target; }
        public void Apply(BattleState battle)
        {
            var battler = _target.Resolve(battle);
            if (battler.IsCharging())
            {
                battler.EndCharge();
            }
        }
    }
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

    public class ModifyDamageDealt : IEffect
    {
        private readonly ITarget _target;
        private readonly double _multiplier;
        public ModifyDamageDealt(ITarget target, double multiplier) { _target = target; _multiplier = multiplier; }
        public void Apply(BattleState battle)
        {
            // Multiplies the last damage dealt — called after FormulaDamage resolves
            int boosted = (int)(battle.LastDamageDealt * _multiplier);
            int extra = boosted - battle.LastDamageDealt;
            _target.Resolve(battle).TakeDamage(extra);
            battle.LastDamageDealt = boosted;
        }
    }

    public class DamageOnAttack : IEffect
    {
        // Life Orb recoil — damages the attacker after dealing damage
        private readonly ITarget _target;
        private readonly double _fraction;
        public DamageOnAttack(ITarget target, double fraction) { _target = target; _fraction = fraction; }
        public void Apply(BattleState battle)
        {
            int recoil = (int)(_target.Resolve(battle).MaxHP * _fraction);
            _target.Resolve(battle).TakeDamage(recoil);
        }
    }

    // ── Stat Modifiers ────────────────────────────────────────────────────────────

    public class ModifySpeedMultiplier : IEffect
    {
        // Choice Scarf — multiplies speed directly rather than using stat stages
        private readonly ITarget _target;
        private readonly double _multiplier;
        public ModifySpeedMultiplier(ITarget target, double multiplier) { _target = target; _multiplier = multiplier; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ApplySpeedMultiplier(_multiplier);
    }

    public class ResetNegativeStatStages : IEffect
    {
        // White Herb — clears only lowered stat stages, leaves boosts intact
        private readonly ITarget _target;
        public ResetNegativeStatStages(ITarget target) { _target = target; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ResetNegativeStatStages();
    }

    public class LockMove : IEffect
    {
        // Choice Band/Specs/Scarf — locks the holder to the first move used
        private readonly ITarget _target;
        public LockMove(ITarget target) { _target = target; }
        public void Apply(BattleState battle) => _target.Resolve(battle).LockToLastMove();
    }

    // ── Accuracy / Evasion Modifiers ──────────────────────────────────────────────

    public class ModifyAccuracy : IEffect
    {
        // Wide Lens — flat accuracy multiplier applied when calculating hit chance
        private readonly ITarget _target;
        private readonly double _multiplier;
        public ModifyAccuracy(ITarget target, double multiplier) { _target = target; _multiplier = multiplier; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ApplyAccuracyMultiplier(_multiplier);
    }

    public class ModifyEvasion : IEffect
    {
        // Bright Powder / Lax Incense — raises holder's evasion multiplier
        private readonly ITarget _target;
        private readonly double _multiplier;
        public ModifyEvasion(ITarget target, double multiplier) { _target = target; _multiplier = multiplier; }
        public void Apply(BattleState battle) => _target.Resolve(battle).ApplyEvasionMultiplier(_multiplier);
    }

    // ── Crit Modifier ─────────────────────────────────────────────────────────────

    public class ModifyCritRatio : IEffect
    {
        // Scope Lens / Razor Claw — raises crit stage by a flat amount
        private readonly ITarget _target;
        private readonly int _stages;
        public ModifyCritRatio(ITarget target, int stages) { _target = target; _stages = stages; }
        public void Apply(BattleState battle) => _target.Resolve(battle).RaiseCritStage(_stages);
    }
    public class PowerUpMove : IEffect
    {
        private readonly double _moveMultiplier;

        public PowerUpMove(double moveMultiplier)
        {
            _moveMultiplier = moveMultiplier;
        }
        public void Apply(BattleState battle)
        {
            PokemonStatCalculatorHelper.Multiplyer = _moveMultiplier;
        }
    }

    // ── Flinch on Attack ──────────────────────────────────────────────────────────

    public class ChanceFlinch : IEffect
    {
        // King's Rock / Razor Fang — chance to flinch the defender after the move hits
        private readonly double _probability;
        public ChanceFlinch(double probability) { _probability = probability; }
        public void Apply(BattleState battle)
        {
            if (RandomHelper.NextBool(_probability))
            {
                battle.Defender.ApplyVolatileStatus(VolatileStatus.Flinch);
            }
        }
    }

    // ── Priority Modifier ─────────────────────────────────────────────────────────

    public class ModifyPriority : IEffect
    {
        // Quick Claw — grants a chance to move first regardless of speed
        private readonly ITarget _target;
        private readonly double _probability;
        public ModifyPriority(ITarget target, double probability) { _target = target; _probability = probability; }
        public void Apply(BattleState battle)
        {
            if (RandomHelper.NextBool(_probability))
            {
                _target.Resolve(battle).SetPriorityOverride(1);
            }
        }
    }

    // ── Status ────────────────────────────────────────────────────────────────────

    public class CureSpecificStatus : IEffect
    {
        // Status berries — only cures if the Pokémon has the specific status
        private readonly ITarget _target;
        private readonly StatusCondition _status;
        public CureSpecificStatus(ITarget target, StatusCondition status) { _target = target; _status = status; }
        public void Apply(BattleState battle)
        {
            var battler = _target.Resolve(battle);
            if (battler.PokemonStatusCondition() == _status)
            {
                battler.ClearStatus();
            }
        }
    }
    // ── Ability Passive Hooks ─────────────────────────────────────────────────────
   
    public class Immune : IEffect
    {
        private readonly ITarget _target;
        private readonly StatusCondition? _status;
        public Immune(ITarget target, StatusCondition? status = null) { _target = target; _status = status; }
        public void Apply(BattleState battle) { } // Checked before effect application, not here
    }

    public class BlockCritical : IEffect
    {
        private readonly ITarget _target;
        public BlockCritical(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked in crit calculation
    }

    public class PreventStatReduction : IEffect
    {
        private readonly ITarget _target;
        private readonly Stat? _stat;
        public PreventStatReduction(ITarget target, Stat? stat = null) { _target = target; _stat = stat; }
        public void Apply(BattleState battle) { } // Checked in ChangeStatStage
    }

    public class BlockSecondaryEffects : IEffect
    {
        private readonly ITarget _target;
        public BlockSecondaryEffects(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked before secondary effect fires
    }

    public class BlockRecoil : IEffect
    {
        private readonly ITarget _target;
        public BlockRecoil(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked before Recoil fires
    }

    public class BlockIndirectDamage : IEffect
    {
        private readonly ITarget _target;
        public BlockIndirectDamage(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked before any non-attack damage
    }

    public class Endure : IEffect
    {
        private readonly ITarget _target;
        public Endure(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked in TakeDamage when HP would hit 0
    }

    public class SuperEffectiveOnly : IEffect
    {
        private readonly ITarget _target;
        public SuperEffectiveOnly(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked in hit/type effectiveness calc
    }

    public class ModifyStatStages : IEffect
    {
        private readonly ITarget _target;
        private readonly double _multiplier;
        public ModifyStatStages(ITarget target, double multiplier) { _target = target; _multiplier = multiplier; }
        public void Apply(BattleState battle) { } // Checked in ChangeStatStage
    }

    public class IgnoreStatChanges : IEffect
    {
        private readonly ITarget _target;
        public IgnoreStatChanges(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked in damage calculation
    }

    public class MaxMultiStrike : IEffect
    {
        private readonly ITarget _target;
        public MaxMultiStrike(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked in multi-hit move resolution
    }

    public class NormalizeType : IEffect
    {
        private readonly ITarget _target;
        public NormalizeType(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked in move type resolution
    }

    public class PreventFlee : IEffect
    {
        private readonly ITarget _target;
        public PreventFlee(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked on flee attempt
    }

    public class PreventSwitch : IEffect
    {
        private readonly ITarget _target;
        public PreventSwitch(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked on switch attempt
    }

    public class PreventItemTheft : IEffect
    {
        private readonly ITarget _target;
        public PreventItemTheft(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked on item steal attempt
    }

    public class WeatherTransform : IEffect
    {
        private readonly ITarget _target;
        public WeatherTransform(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked on weather change / turn start
    }

    public class Pickup : IEffect
    {
        private readonly ITarget _target;
        public Pickup(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked after battle ends
    }

    public class GuaranteedFlee : IEffect
    {
        private readonly ITarget _target;
        public GuaranteedFlee(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked on flee attempt
    }

    public class ModifySleepTurns : IEffect
    {
        private readonly ITarget _target;
        private readonly double _multiplier;
        public ModifySleepTurns(ITarget target, double multiplier) { _target = target; _multiplier = multiplier; }
        public void Apply(BattleState battle) { } // Checked in sleep turn countdown
    }

    public class Truant : IEffect
    {
        private readonly ITarget _target;
        public Truant(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked at move selection
    }

    public class SlowStart : IEffect
    {
        private readonly ITarget _target;
        private readonly double _multiplier;
        private readonly int _turns;
        public SlowStart(ITarget target, double multiplier, int turns = 5) { _target = target; _multiplier = multiplier; _turns = turns; }
        public void Apply(BattleState battle) { } // Checked in stat calculation for first N turns
    }

    public class DoublePPUsage : IEffect
    {
        private readonly ITarget _target;
        public DoublePPUsage(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked when opponent uses a move
    }

    public class IgnoreAbility : IEffect
    {
        private readonly ITarget _target;
        public IgnoreAbility(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked before ability hooks fire
    }
    
    public class GenderRivalry : IEffect
    {
        private readonly ITarget _target;
        public GenderRivalry(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked in damage calculation
    }

    public class BlockMove : IEffect
    {
        private readonly ITarget _target;
        public BlockMove(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked before specific move types fire
    }

    public class SuppressWeather : IEffect
    {
        private readonly ITarget _target;
        public SuppressWeather(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked in weather effect application
    }

    public class MultitypeChange : IEffect
    {
        private readonly ITarget _target;
        public MultitypeChange(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked on entry / item change
    }

    public class MoveLastPriority : IEffect
    {
        private readonly ITarget _target;
        public MoveLastPriority(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked in turn order resolution
    }

    public class TypeChange : IEffect
    {
        private readonly ITarget _target;
        public TypeChange(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked after being hit
    }

    public class CopyAbility : IEffect
    {
        private readonly ITarget _target;
        public CopyAbility(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { }
    }

    public class PassStatus : IEffect
    {
        private readonly ITarget _target;
        public PassStatus(ITarget target) { _target = target; }
        public void Apply(BattleState battle)
        {
            var status = battle.Attacker.PokemonStatusCondition();
            _target.Resolve(battle).ApplyStatus(status);
        }
    }

    public class DamageRedirect : IEffect
    {
        private readonly ITarget _target;
        public DamageRedirect(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // Checked when drain move resolves
    }

    public class ModifyChance : IEffect
    {
        private readonly ITarget _target;
        private readonly double _multiplier;
        public ModifyChance(ITarget target, double multiplier) { _target = target; _multiplier = multiplier; }
        public void Apply(BattleState battle) { } // Checked when secondary chance rolls
    }

    public class InspectOpponent : IEffect
    {
        private readonly ITarget _target;
        public InspectOpponent(ITarget target) { _target = target; }
        public void Apply(BattleState battle) { } // UI reveal of opponent item/moves
    }
}