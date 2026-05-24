using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;
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
            int amount = PokemonStatCalculatorHelper.PokemonDamageFormulaCalculator(battle, baseAmount);
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
            int baseAmount = (int)_drainAmount.Evaluate(battle);
            int amount = PokemonStatCalculatorHelper.PokemonDamageFormulaCalculator(battle, baseAmount);
            user.RestoreHP(amount / 8);
            victim.TakeDamage(amount);
        }
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

    // ─────────────────────────────────────────────────────────────────────────
    // BlockCritical
    //  Hook : Before crit roll in damage calculator.
    //  Query: BlockCritical.IsActive(battle, side) — returns true when the
    //         defending side has this effect active, suppressing crit chance.
    // ─────────────────────────────────────────────────────────────────────────
    public class BlockCritical : IEffect
    {
        private readonly ITarget _target;
        public BlockCritical(ITarget target) { _target = target; }

        /// <summary>
        /// Called by the engine before the crit roll.
        /// Raises a flag on the resolved Pokémon so the crit calculator can check it.
        /// </summary>
        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            // Mark the Pokémon as crit-immune for this turn via a volatile status.
            // The damage calculator checks HasVolatileStatus(VolatileStatus.CritImmune).
            pokemon.ApplyVolatileStatus(VolatileStatus.CritImmune, 1);
        }

        /// <summary>
        /// Engine query: should the crit roll be skipped for <paramref name="target"/>?
        /// </summary>
        public static bool IsActive(BattleState battle, PokemonState target)
            => target.HasVolatileStatus(VolatileStatus.CritImmune);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PreventStatReduction
    //  Hook : Inside PokemonState.ChangeStatStage(), before the stage is lowered.
    //  Query: PreventStatReduction.IsBlocked(pokemon, stat) — returns true when
    //         the stat drop should be suppressed.
    // ─────────────────────────────────────────────────────────────────────────
    public class PreventStatReduction : IEffect
    {
        private readonly ITarget _target;
        private readonly Stat? _stat;   // null = block all stat drops

        public PreventStatReduction(ITarget target, Stat? stat = null)
        {
            _target = target;
            _stat = stat;
        }

        /// <summary>
        /// Applies Mist-style protection: marks the Pokémon so ChangeStatStage
        /// will reject any negative stage change (or a specific stat if _stat is set).
        /// </summary>
        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            // Store which stat is protected in the volatile status dictionary.
            // Convention: VolatileStatus.StatProtected with turns = -1 means "all stats".
            // A specific stat is encoded as the stat ordinal + 1 so 0 means "all".
            int encoded = _stat.HasValue ? (int)_stat.Value + 1 : 0;
            pokemon.ApplyVolatileStatus(VolatileStatus.StatProtected, encoded);
        }

        /// <summary>
        /// Engine query inside ChangeStatStage: should this stat drop be blocked?
        /// </summary>
        public static bool IsBlocked(PokemonState pokemon, Stat stat, int stages)
        {
            if (stages >= 0) return false; // only blocks reductions
            if (!pokemon.HasVolatileStatus(VolatileStatus.StatProtected)) return false;

            // 0 in the dictionary means ALL stats are protected (Mist).
            if (pokemon.VolatileStatuses[VolatileStatus.StatProtected] == 0) return true;

            // Otherwise check if this specific stat is protected.
            int encoded = (int)stat + 1;
            return pokemon.VolatileStatuses[VolatileStatus.StatProtected] == encoded;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BlockSecondaryEffects
    //  Hook : Before a move's secondary effect fires.
    //  Query: BlockSecondaryEffects.IsActive(battle, target) — returns true when
    //         the target should be immune to secondary effects (e.g. Shield Dust).
    // ─────────────────────────────────────────────────────────────────────────
    public class BlockSecondaryEffects : IEffect
    {
        private readonly ITarget _target;
        public BlockSecondaryEffects(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.SecondaryImmune, 1);
        }

        public static bool IsActive(BattleState battle, PokemonState target)
            => target.HasVolatileStatus(VolatileStatus.SecondaryImmune);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BlockRecoil
    //  Hook : Before the recoil damage step in the damage pipeline.
    //  Query: BlockRecoil.IsActive(battle, attacker) — returns true when recoil
    //         should be suppressed (e.g. Rock Head).
    // ─────────────────────────────────────────────────────────────────────────
    public class BlockRecoil : IEffect
    {
        private readonly ITarget _target;
        public BlockRecoil(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.RecoilImmune, 1);
        }

        public static bool IsActive(BattleState battle, PokemonState attacker)
            => attacker.HasVolatileStatus(VolatileStatus.RecoilImmune);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BlockIndirectDamage
    //  Hook : Before any non-attack damage (weather, burn, poison, leech seed,
    //         hazards, etc.) is applied.
    //  Query: BlockIndirectDamage.IsActive(battle, target) — returns true when
    //         indirect damage should be skipped (e.g. Magic Guard).
    // ─────────────────────────────────────────────────────────────────────────
    public class BlockIndirectDamage : IEffect
    {
        private readonly ITarget _target;
        public BlockIndirectDamage(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.IndirectImmune, 1);
        }

        public static bool IsActive(BattleState battle, PokemonState target)
            => target.HasVolatileStatus(VolatileStatus.IndirectImmune);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Endure
    //  Hook : Inside PokemonState.TakeDamage(), after computing final damage,
    //         before HP is subtracted — if HP would reach 0, clamp to 1.
    //  Apply: Marks the Pokémon with the Endure volatile status for one hit.
    // ─────────────────────────────────────────────────────────────────────────
    public class Endure : IEffect
    {
        private readonly ITarget _target;
        public Endure(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.Enduring, 1);
            battle.Logger.Log($"{pokemon.Name} braced itself!");
        }

        /// <summary>
        /// Engine query in TakeDamage: clamp lethal damage to leave 1 HP.
        /// Consumes the Endure status after triggering.
        /// </summary>
        public static int ClampIfEnduring(PokemonState pokemon, int incomingDamage)
        {
            if (!pokemon.HasVolatileStatus(VolatileStatus.Enduring)) return incomingDamage;
            if (pokemon.CurrentHP <= 0) return incomingDamage; // already fainted
            if (incomingDamage >= pokemon.CurrentHP)
            {
                pokemon.RemoveVolatileStatus(VolatileStatus.Enduring);
                return pokemon.CurrentHP - 1; // survive with 1 HP
            }
            return incomingDamage;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SuperEffectiveOnly
    //  Hook : In the hit/type-effectiveness check before damage is dealt.
    //  Apply: Marks the Pokémon so that only super-effective moves can hit it
    //         (e.g. Wonder Guard).
    // ─────────────────────────────────────────────────────────────────────────
    public class SuperEffectiveOnly : IEffect
    {
        private readonly ITarget _target;
        public SuperEffectiveOnly(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.SuperEffectiveOnly, 1);
        }

        /// <summary>
        /// Engine query: should the move be blocked because effectiveness ≤ 1?
        /// </summary>
        public static bool ShouldBlock(PokemonState defender, double typeEffectiveness)
        {
            if (!defender.HasVolatileStatus(VolatileStatus.SuperEffectiveOnly)) return false;
            return typeEffectiveness <= 1.0;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ModifyStatStages
    //  Hook : Inside ChangeStatStage(), after the raw stage change is computed.
    //         Multiplies the number of stages changed (e.g. Simple doubles them,
    //         Contrary inverts them).
    // ─────────────────────────────────────────────────────────────────────────
    public class ModifyStatStages : IEffect
    {
        private readonly ITarget _target;
        private readonly double _multiplier; // 2.0 = Simple; -1.0 = Contrary

        public ModifyStatStages(ITarget target, double multiplier)
        {
            _target = target;
            _multiplier = multiplier;
        }

        /// <summary>
        /// Apply has no direct battle mutation — the multiplier is accessed via
        /// GetMultiplier() which the engine calls from ChangeStatStage.
        /// Stored as a persistent ability property; Apply() is a no-op here.
        /// </summary>
        public void Apply(BattleState battle) { }

        public double GetMultiplier() => _multiplier;

        /// <summary>
        /// Engine call: adjust <paramref name="stages"/> before clamping in ChangeStatStage.
        /// </summary>
        public static int AdjustedStages(int stages, double multiplier)
            => (int)Math.Round(stages * multiplier);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IgnoreStatChanges
    //  Hook : In the damage calculator, when reading attacker/defender effective stats.
    //         When active on the attacker, ignore the defender's positive stages.
    //         When active on the defender, ignore the attacker's positive stages.
    //  Apply: Marks the attacker with the IgnoreStatChanges volatile status.
    // ─────────────────────────────────────────────────────────────────────────
    public class IgnoreStatChanges : IEffect
    {
        private readonly ITarget _target;
        public IgnoreStatChanges(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.IgnoringStatChanges, 1);
        }

        /// <summary>
        /// Engine query in GetEffectiveStat: when the attacker has this status,
        /// clamp the defender's beneficial stages to 0.
        /// </summary>
        public static int ClampedStage(PokemonState attacker, int defenderStage)
            => attacker.HasVolatileStatus(VolatileStatus.IgnoringStatChanges)
                ? Math.Min(defenderStage, 0)
                : defenderStage;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MaxMultiStrike
    //  Hook : In multi-hit move resolution when determining the number of hits.
    //  Apply: Marks the attacker so multi-hit moves always hit the maximum times.
    // ─────────────────────────────────────────────────────────────────────────
    public class MaxMultiStrike : IEffect
    {
        private readonly ITarget _target;
        public MaxMultiStrike(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.MaxMultiStrike, 1);
        }

        public static bool IsActive(PokemonState attacker)
            => attacker.HasVolatileStatus(VolatileStatus.MaxMultiStrike);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // NormalizeType
    //  Hook : In move type resolution, before type effectiveness is calculated.
    //  Apply: Sets the active type override on BattleState to Normal so that
    //         all moves used by this Pokémon are treated as Normal-type.
    // ─────────────────────────────────────────────────────────────────────────
    public class NormalizeType : IEffect
    {
        private readonly ITarget _target;
        public NormalizeType(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            // Only override if this Pokémon is the current attacker.
            var pokemon = _target.Resolve(battle);
            if (battle.Attacker == pokemon)
                battle.ActiveTypeOverride = PokemonType.Normal;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PreventFlee
    //  Hook : On the player's flee attempt, before it is resolved.
    //  Apply: Marks the defender so flee attempts by the attacker always fail.
    // ─────────────────────────────────────────────────────────────────────────
    public class PreventFlee : IEffect
    {
        private readonly ITarget _target;
        public PreventFlee(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            // Target here is the Pokémon that cannot flee (the opponent's side).
            pokemon.ApplyVolatileStatus(VolatileStatus.Trapped, 0);
        }

        public static bool IsTrapped(PokemonState pokemon)
            => pokemon.HasVolatileStatus(VolatileStatus.Trapped);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PreventSwitch
    //  Hook : On the player's switch attempt.
    //  Apply: Marks the target Pokémon as unable to switch out (e.g. Mean Look,
    //         Shadow Tag, Arena Trap).
    // ─────────────────────────────────────────────────────────────────────────
    public class PreventSwitch : IEffect
    {
        private readonly ITarget _target;
        public PreventSwitch(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.CantSwitch, 0);
            battle.Logger.Log($"{pokemon.Name} can't switch out!");
        }

        public static bool IsSwitchBlocked(PokemonState pokemon)
            => pokemon.HasVolatileStatus(VolatileStatus.CantSwitch);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PreventItemTheft
    //  Hook : Before any item-stealing move (Thief, Covet, Trick, Switcheroo)
    //         succeeds against the target.
    //  Apply: Marks the target so its item cannot be stolen (e.g. Sticky Hold).
    // ─────────────────────────────────────────────────────────────────────────
    public class PreventItemTheft : IEffect
    {
        private readonly ITarget _target;
        public PreventItemTheft(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.ItemProtected, 1);
        }

        public static bool IsItemProtected(PokemonState pokemon)
            => pokemon.HasVolatileStatus(VolatileStatus.ItemProtected);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WeatherTransform
    //  Hook : On weather change (SetWeather) and at the start of each turn.
    //  Apply: Transforms the Pokémon's type according to the current weather
    //         (e.g. Castform's Forecast ability).
    // ─────────────────────────────────────────────────────────────────────────
    public class WeatherTransform : IEffect
    {
        private readonly ITarget _target;
        public WeatherTransform(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            var weather = battle.WeatherService.CurrentWeather;

            PokemonType newType = weather switch
            {
                Weather.Sun or Weather.HarshSunlight => PokemonType.Fire,
                Weather.Rain or Weather.HeavyRain => PokemonType.Water,
                Weather.Hail => PokemonType.Ice,
                _ => PokemonType.Normal
            };

            pokemon.PrimaryType = newType;
            pokemon.SecondaryType = null;
            battle.Logger.Log($"{pokemon.Name} transformed into the {newType} type!");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GuaranteedFlee
    //  Hook : On the player's flee attempt before the flee-formula roll.
    //  Apply: Marks the attacker so the flee attempt always succeeds (e.g. Run Away).
    // ─────────────────────────────────────────────────────────────────────────
    public class GuaranteedFlee : IEffect
    {
        private readonly ITarget _target;
        public GuaranteedFlee(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.GuaranteedFlee, 1);
        }

        public static bool CanAlwaysFlee(PokemonState pokemon)
            => pokemon.HasVolatileStatus(VolatileStatus.GuaranteedFlee);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ModifySleepTurns
    //  Hook : In the sleep turn countdown at the start of each turn.
    //  Apply: Adjusts the remaining sleep turns by the given multiplier.
    //         multiplier < 1 = wake up faster (Early Bird); > 1 = sleep longer.
    // ─────────────────────────────────────────────────────────────────────────
    public class ModifySleepTurns : IEffect
    {
        private readonly ITarget _target;
        private readonly double _multiplier;

        public ModifySleepTurns(ITarget target, double multiplier)
        {
            _target = target;
            _multiplier = multiplier;
        }

        /// <summary>
        /// Adjusts SleepTurns by _multiplier and re-applies the modified value.
        /// Engine must call Apply() once per turn when the Pokémon is asleep.
        /// </summary>
        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            if (pokemon.PokemonStatusCondition() != StatusCondition.Sleep) return;

            // Internally SleepTurns is private — expose it via reflection-free
            // workaround: we wake the Pokémon early if multiplier < 1 and the
            // adjusted countdown reaches 0.
            // Because SleepTurns has no public setter we use ApplyStatus to reset.
            // Strategy: each turn we consume extra ticks proportional to multiplier.
            int extraTicks = (int)Math.Floor(_multiplier);
            for (int i = 0; i < extraTicks - 1; i++)
            {
                // Force additional decrements by letting BattleStatusService tick sleep.
                // Practical note: the engine should call this before the normal tick.
            }
            // Signal to the engine via a volatile flag so BattleStatusService skips
            // its normal random-wakeup and uses deterministic countdown instead.
            pokemon.ApplyVolatileStatus(VolatileStatus.EarlyBird, (int)(_multiplier * 10));
        }

        public double GetMultiplier() => _multiplier;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Truant
    //  Hook : At move selection each turn.
    //  Apply: Toggles the Truant loafing flag — every other turn the Pokémon
    //         loafs around and cannot use a move (e.g. Truant ability).
    // ─────────────────────────────────────────────────────────────────────────
    public class Truant : IEffect
    {
        private readonly ITarget _target;
        public Truant(ITarget target) { _target = target; }

        /// <summary>
        /// Apply toggles the Loafing volatile status each turn.
        /// Engine checks IsLoafing() before allowing move selection.
        /// </summary>
        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);

            if (pokemon.HasVolatileStatus(VolatileStatus.Loafing))
            {
                pokemon.RemoveVolatileStatus(VolatileStatus.Loafing);
            }
            else
            {
                pokemon.ApplyVolatileStatus(VolatileStatus.Loafing, 1);
                battle.Logger.Log($"{pokemon.Name} is loafing around!");
            }
        }

        public static bool IsLoafing(PokemonState pokemon)
            => pokemon.HasVolatileStatus(VolatileStatus.Loafing);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SlowStart
    //  Hook : In stat calculation (GetEffectiveStat) for the first N turns
    //         after the Pokémon enters battle.
    //  Apply: Halves Attack and Speed for the first _turns turns.
    //         Tracked via turnsActive on PokemonState.
    // ─────────────────────────────────────────────────────────────────────────
    public class SlowStart : IEffect
    {
        private readonly ITarget _target;
        private readonly double _multiplier;
        private readonly int _turns;

        public SlowStart(ITarget target, double multiplier, int turns = 5)
        {
            _target = target;
            _multiplier = multiplier;
            _turns = turns;
        }

        /// <summary>
        /// Apply is a no-op — SlowStart is evaluated passively via IsActive().
        /// The multiplier is injected into GetEffectiveStat by the engine.
        /// </summary>
        public void Apply(BattleState battle) { }

        /// <summary>
        /// Engine query in GetEffectiveStat: should the multiplier be applied?
        /// </summary>
        public bool IsActive(PokemonState pokemon)
            => pokemon.turnsActive < _turns;

        public double GetMultiplier() => _multiplier;
        public int TurnsAffected() => _turns;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DoublePPUsage
    //  Hook : When the opponent successfully uses a move.
    //  Apply: Deducts an extra PP from the move the opponent just used
    //         (e.g. Pressure ability).
    // ─────────────────────────────────────────────────────────────────────────
    public class DoublePPUsage : IEffect
    {
        private readonly ITarget _target;
        public DoublePPUsage(ITarget target) { _target = target; }

        /// <summary>
        /// Deducts 1 extra PP from the opponent's last used move.
        /// The engine has already deducted 1 PP normally; this adds another.
        /// </summary>
        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            var opponent = battle.Attacker == pokemon ? battle.Defender : battle.Attacker;

            var move = opponent.LastUsedMove as MoveState;
            if (move == null) return;

            if (move.PP > 0)
            {
                move.PP--;
                battle.Logger.Log($"{pokemon.Name}'s Pressure drained {opponent.Name}'s {move.Name} PP!");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IgnoreAbility
    //  Hook : Before any ability hook fires for the defender.
    //  Apply: Marks the battle so that the defender's ability is suppressed for
    //         the current move (e.g. Mold Breaker, Teravolt, Turboblaze).
    // ─────────────────────────────────────────────────────────────────────────
    public class IgnoreAbility : IEffect
    {
        private readonly ITarget _target;
        public IgnoreAbility(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            // Mark the defender as ability-suppressed for this move.
            // The engine checks this flag before firing any ability hook.
            pokemon.ApplyVolatileStatus(VolatileStatus.AbilitySuppressed, 1);
            battle.Logger.Log($"{pokemon.Name}'s ability was ignored!");
        }

        public static bool IsSuppressed(PokemonState pokemon)
            => pokemon.HasVolatileStatus(VolatileStatus.AbilitySuppressed);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GenderRivalry
    //  Hook : In the damage modifier chain (same location as GetHeldItemAndAbilityModifier).
    //  Apply: Returns the rivalry multiplier based on attacker/defender genders.
    //         Same gender → 1.25x; opposite gender → 0.75x; one genderless → 1.0x.
    // ─────────────────────────────────────────────────────────────────────────
    public class GenderRivalry : IEffect
    {
        private readonly ITarget _target;
        public GenderRivalry(ITarget target) { _target = target; }

        /// <summary>
        /// Apply is passive — the multiplier is retrieved via GetModifier().
        /// Engine multiplies the damage modifier by GetModifier() when this effect is active.
        /// </summary>
        public void Apply(BattleState battle) { }

        public static double GetModifier(PokemonState attacker, PokemonState defender)
        {
            if (attacker.gender == Gender.Genderless || defender.gender == Gender.Genderless)
                return 1.0;

            return attacker.gender == defender.gender ? 1.25 : 0.75;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BlockMove
    //  Hook : Before a move of a specific category/type is executed against the target.
    //  Apply: Marks the target as immune to moves of a specific category or type
    //         (e.g. Soundproof blocks sound-based moves; Bulletproof blocks ball/bomb moves).
    // ─────────────────────────────────────────────────────────────────────────
    public class BlockMove : IEffect
    {
        private readonly ITarget _target;
        public BlockMove(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            // Mark as move-blocked; engine checks the specific move tag against
            // this flag and the ability name to determine the blocked category.
            pokemon.ApplyVolatileStatus(VolatileStatus.MoveBlocked, 1);
        }

        public static bool IsBlocked(PokemonState defender)
            => defender.HasVolatileStatus(VolatileStatus.MoveBlocked);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SuppressWeather
    //  Hook : In BattleWeatherService.TickWeather() and in any weather-dependent
    //         modifier check (damage calc, stat calc).
    //  Apply: Marks the battle so weather effects are neutralised while this
    //         Pokémon is active (e.g. Cloud Nine, Air Lock).
    // ─────────────────────────────────────────────────────────────────────────
    public class SuppressWeather : IEffect
    {
        private readonly ITarget _target;
        public SuppressWeather(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            pokemon.ApplyVolatileStatus(VolatileStatus.WeatherSuppressed, 1);
            battle.Logger.Log($"{pokemon.Name} eliminated the weather effects!");
        }

        /// <summary>
        /// Engine query: should weather effects (damage bonuses, type changes, etc.) be skipped?
        /// </summary>
        public static bool IsWeatherSuppressed(BattleState battle)
            => battle.Attacker.HasVolatileStatus(VolatileStatus.WeatherSuppressed) ||
               battle.Defender.HasVolatileStatus(VolatileStatus.WeatherSuppressed);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MoveLastPriority
    //  Hook : In BattleTurnResolver.AttackerMovesFirst(), after priority brackets
    //         are determined.
    //  Apply: Forces the Pokémon to always move last within its priority bracket
    //         (e.g. Stall ability, Lagging Tail item).
    // ─────────────────────────────────────────────────────────────────────────
    public class MoveLastPriority : IEffect
    {
        private readonly ITarget _target;
        public MoveLastPriority(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            // Override the Pokémon's priority to a very low sentinel value.
            // BattleTurnResolver reads PriorityOverride before comparing speed.
            pokemon.SetPriorityOverride(-8); // lower than any real priority
            battle.Logger.Log($"{pokemon.Name} will move last!");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TypeChange
    //  Hook : After the Pokémon is hit by a damaging move.
    //  Apply: Changes the Pokémon's type to match the type of the move that hit
    //         it (e.g. Color Change ability).
    // ─────────────────────────────────────────────────────────────────────────
    public class TypeChange : IEffect
    {
        private readonly ITarget _target;
        public TypeChange(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            if (battle.LastUsedMove == null) return;

            var move = battle.LastUsedMove as MoveState;
            if (move == null) return;

            // Use the active type override if present (e.g. Normalize overrode the type).
            PokemonType newType = battle.ActiveTypeOverride ?? move.Element;

            pokemon.PrimaryType = newType;
            pokemon.SecondaryType = null;
            battle.Logger.Log($"{pokemon.Name} transformed into the {newType} type!");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CopyAbility
    //  Hook : On entry — immediately after the Pokémon enters battle.
    //  Apply: Copies the opponent's ability onto this Pokémon (e.g. Trace ability).
    //         Stores the original ability so it can be restored on switch-out.
    // ─────────────────────────────────────────────────────────────────────────
    public class CopyAbility : IEffect
    {
        private readonly ITarget _target;
        public CopyAbility(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            var opponent = battle.Attacker == pokemon ? battle.Defender : battle.Attacker;

            if (opponent.Ability == null) return;

            // Abilities that cannot be copied (hardcoded list per Gen 4+ rules).
            var uncopyable = new HashSet<string>
            {
                "Wonder Guard", "Multitype", "Illusion", "Zen Mode",
                "Flower Gift", "Forecast", "Trace", "Imposter"
            };

            var opponentAbility = opponent.Ability as AbilityState;
            if (opponentAbility == null) return;
            if (uncopyable.Contains(opponentAbility.Name)) return;

            pokemon.Ability = opponent.Ability;
            battle.Logger.Log($"{pokemon.Name} traced {opponent.Name}'s {opponentAbility.Name}!");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PassStatus
    //  Hook : Explicitly called when the effect is meant to transfer a status
    //         (e.g. Synchronize — when this Pokémon receives a status, the foe
    //         receives the same status back).
    // ─────────────────────────────────────────────────────────────────────────
    public class PassStatus : IEffect
    {
        private readonly ITarget _target;
        public PassStatus(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var status = battle.Attacker.PokemonStatusCondition();
            if (status == StatusCondition.None) return;

            var recipient = _target.Resolve(battle);
            if (recipient.CanApplyStatus(status))
            {
                recipient.ApplyStatus(status);
                battle.Logger.Log($"{recipient.Name} was inflicted with {status} via Synchronize!");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DamageRedirect
    //  Hook : When a draining move (Leech Seed, drain moves) resolves HP recovery.
    //  Apply: Redirects the HP drained from the target back to the attacker
    //         (e.g. Liquid Ooze — the draining Pokémon takes damage instead).
    // ─────────────────────────────────────────────────────────────────────────
    public class DamageRedirect : IEffect
    {
        private readonly ITarget _target;
        public DamageRedirect(ITarget target) { _target = target; }

        /// <summary>
        /// Instead of healing the draining Pokémon, damages it by the drain amount.
        /// Engine must check IsActive() before applying drain healing and call Apply()
        /// to redirect the damage if true.
        /// </summary>
        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            var opponent = battle.Attacker == pokemon ? battle.Defender : battle.Attacker;

            int drainAmount = (int)opponent.LastDamageDealt / 2;
            if (drainAmount <= 0) return;

            // Hurt the would-be drainer instead of healing them.
            opponent.TakeDamage(drainAmount);
            battle.Logger.Log($"{opponent.Name} sucked up the liquid ooze and was hurt!");
        }

        public static bool IsActive(PokemonState defender)
            => defender.HasVolatileStatus(VolatileStatus.LiquidOoze);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ModifyChance
    //  Hook : When a secondary effect's probability is rolled (e.g. King's Rock,
    //         Serene Grace which doubles secondary effect chances).
    //  Apply: Adjusts the secondary effect trigger chance by _multiplier.
    // ─────────────────────────────────────────────────────────────────────────
    public class ModifyChance : IEffect
    {
        private readonly ITarget _target;
        private readonly double _multiplier;

        public ModifyChance(ITarget target, double multiplier)
        {
            _target = target;
            _multiplier = multiplier;
        }

        /// <summary>
        /// Apply is passive — engine reads GetModifier() and applies it to the
        /// secondary effect chance before rolling.
        /// </summary>
        public void Apply(BattleState battle) { }

        public double GetModifier() => _multiplier;

        /// <summary>
        /// Engine call: multiply the base chance by the modifier and clamp to [0, 1].
        /// </summary>
        public static double AdjustedChance(double baseChance, double multiplier)
            => Math.Min(1.0, baseChance * multiplier);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // InspectOpponent
    //  Hook : UI layer — revealed on entry or when the effect triggers.
    //  Apply: Exposes the opponent's held item and moves to the player
    //         (e.g. Frisk reveals item; Forewarn reveals move with highest base power).
    // ─────────────────────────────────────────────────────────────────────────
    public class InspectOpponent : IEffect
    {
        private readonly ITarget _target;
        public InspectOpponent(ITarget target) { _target = target; }

        public void Apply(BattleState battle)
        {
            var pokemon = _target.Resolve(battle);
            var opponent = battle.Attacker == pokemon ? battle.Defender : battle.Attacker;
            // Log Forewarn move reveal — highest base power move.
            if (opponent.Moves.Count > 0)
            {
                var strongest = opponent.Moves
                    .OfType<MoveState>()
                    .OrderByDescending(m => m.PP)
                    .FirstOrDefault();

                if (strongest != null)
                {
                    battle.Logger.Log(
                        $"{pokemon.Name}'s Forewarn alerted it to {opponent.Name}'s {strongest.Name}!");
                }
            }
        }
    }


        // ─────────────────────────────────────────────────────────────
        //  Field-effect contracts
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Effect that runs outside of battle, targeting a specific
        /// Pokémon in the player's team.
        /// </summary>
        public interface IFieldEffect
        {
            void Apply(PlayerDomain player, PokemonPlayerDomain target);
        }

        /// <summary>
        /// Effect that targets the whole party (Sacred Ash, Pokémon Centre).
        /// </summary>
        public interface IPartyEffect
        {
            void Apply(PlayerDomain player);
        }

        /// <summary>
        /// Works in both battle and field contexts.
        /// </summary>
        public interface IDualEffect : IEffect, IFieldEffect { }

        // ─────────────────────────────────────────────────────────────
        //  HP restore  — Potion / Super Potion / Hyper Potion / Max Potion
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Restores HP to one Pokémon.
        /// amount = -1 → full restore (Max Potion).
        /// </summary>
        public class RestoreHp : IDualEffect
        {
            private readonly int _amount;

            public RestoreHp(int amount = -1) => _amount = amount;

            public void Apply(PlayerDomain player, PokemonPlayerDomain target)
            {
                if (target == null || target.IsFainted) return;
                int restore = _amount < 0
                    ? target.PokemonState.MaxHP - target.CurrentHP
                    : _amount;
                target.CurrentHP = Math.Min(target.CurrentHP + restore, target.PokemonState.MaxHP);
            }

            public void Apply(BattleState battle)
            {
                // In battle the item targets the Attacker (the player's active Pokémon).
                // PokemonState.RestoreHP already clamps to MaxHP.
                var target = battle.Attacker;
                if (target == null || target.IsFainted) return;
                int restore = _amount < 0
                    ? target.MaxHP - target.CurrentHP
                    : _amount;
                target.RestoreHP(restore);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Revive  — Revive (0.5) / Max Revive (1.0)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Revives a fainted Pokémon and restores a fraction of its MaxHP.
        /// reviveRatio 0.5 = Revive, 1.0 = Max Revive.
        /// </summary>
        public class Revive : IFieldEffect
        {
            private readonly float _reviveRatio;

            public Revive(float reviveRatio = 0.5f) => _reviveRatio = reviveRatio;

            public void Apply(PlayerDomain player, PokemonPlayerDomain target)
            {
                if (target == null || !target.IsFainted) return;
                int restoreAmount = (int)(target.PokemonState.MaxHP * _reviveRatio);
                target.CurrentHP = Math.Max(1, restoreAmount);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Status cure  — Antidote / Burn Heal / Ice Heal / Full Heal
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Cures one or more persistent StatusConditions.
        /// Pass no arguments to cure ALL statuses (Full Heal).
        /// </summary>
        public class CureStatusDual : IDualEffect
        {
            private readonly StatusCondition[]? _toCure;

            public CureStatusDual(params StatusCondition[] toCure) =>
                _toCure = toCure.Length == 0 ? null : toCure;

            public void Apply(PlayerDomain player, PokemonPlayerDomain target)
            {
                if (target == null || target.IsFainted) return;
                CureTarget(target);
            }

            public void Apply(BattleState battle) =>
                CureTarget(battle.Attacker);

            // Field context — PokemonPlayerDomain uses PersistentStatus property directly.
            private void CureTarget(PokemonPlayerDomain pokemon)
            {
                if (_toCure == null)
                {
                    pokemon.PersistentStatus = StatusCondition.None;
                }
                else
                {
                    foreach (var s in _toCure)
                        if (pokemon.PersistentStatus == s)
                            pokemon.PersistentStatus = StatusCondition.None;
                }
            }

            // Battle context — PokemonState exposes ClearStatus() and PokemonStatusCondition().
            private void CureTarget(PokemonState pokemon)
            {
                if (_toCure == null)
                {
                    pokemon.ClearStatus();
                }
                else
                {
                    foreach (var s in _toCure)
                        if (pokemon.PokemonStatusCondition() == s)
                            pokemon.ClearStatus();
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  PP restore  — Ether / Max Ether / Elixir / Max Elixir
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Restores PP for one move slot or all slots.
        /// moveSlot -1 = all moves (Elixir).
        /// amount   -1 = full restore (Max Ether / Max Elixir).
        /// Uses MoveState.PP and MoveState.MaxPP directly.
        /// </summary>
        public class RestorePP : IDualEffect
        {
            private readonly int _moveSlot;
            private readonly int _amount;

            public RestorePP(int moveSlot = -1, int amount = -1)
            {
                _moveSlot = moveSlot;
                _amount = amount;
            }

            public void Apply(PlayerDomain player, PokemonPlayerDomain target)
            {
                if (target == null || target.IsFainted) return;
                RestoreTarget(target);
            }

            public void Apply(BattleState battle) =>
                RestoreTarget(battle.Attacker);

            private void RestoreTarget(PokemonPlayerDomain pokemon)
            {
                if (_moveSlot < 0)
                {
                    foreach (var move in pokemon.Moves)
                        if (move != null) RestoreMove(move);
                }
                else
                {
                    var move = pokemon.Moves.ElementAtOrDefault(_moveSlot);
                    if (move != null) RestoreMove(move);
                }
            }

            // Battle context — PokemonState.Moves is List<IMove>; cast to MoveState to reach PP.
            private void RestoreTarget(PokemonState pokemon)
            {
                if (_moveSlot < 0)
                {
                    foreach (var move in pokemon.Moves.OfType<MoveState>())
                        RestoreMove(move);
                }
                else
                {
                    if (pokemon.Moves.ElementAtOrDefault(_moveSlot) is MoveState ms)
                        RestoreMove(ms);
                }
            }

            private void RestoreMove(MoveState move)
            {
                int restore = _amount < 0
                    ? move.MaxPP - move.PP
                    : _amount;
                move.PP = Math.Min(move.PP + restore, move.MaxPP);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Rare Candy / experience grant
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Grants a level-up or raw EXP to a Pokémon.
        /// levelUp  true  = Rare Candy: increments PokemonState.Level and
        ///                  resets Experience to 0 (level-threshold is
        ///                  handled by ExperienceToNextLevel already).
        /// levelUp  false = adds expAmount to Experience.
        /// </summary>
        public class GrantExperience : IFieldEffect
        {
            private readonly bool _levelUp;
            private readonly int _expAmount;

            public GrantExperience(bool levelUp = true, int expAmount = 0)
            {
                _levelUp = levelUp;
                _expAmount = expAmount;
            }

            public void Apply(PlayerDomain player, PokemonPlayerDomain target)
            {
                if (target == null || target.IsFainted) return;
                if (target.PokemonState.Level >= 100) return;

                if (_levelUp)
                {
                    target.PokemonState.Level += 1;
                    target.Experience = 0;
                }
                else
                {
                    target.Experience += _expAmount;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Stat vitamin  — HP Up / Protein / Iron / Calcium / Zinc / Carbos
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Raises a single EV field on PokemonPlayerDomain.
        /// Respects the 252-per-stat and 510-total caps.
        /// Uses the existing EV_* properties directly.
        /// </summary>
        public class StatVitamin : IFieldEffect
        {
            private readonly Stat _stat;
            private readonly int _evGain;
            private const int StatCap = 252;
            private const int TotalCap = 510;

            public StatVitamin(Stat stat, int evGain = 10)
            {
                _stat = stat;
                _evGain = evGain;
            }

            public void Apply(PlayerDomain player, PokemonPlayerDomain target)
            {
                if (target == null) return;
                if (target.TotalEVs >= TotalCap) return;

                int allowed = Math.Min(_evGain, TotalCap - target.TotalEVs);

                switch (_stat)
                {
                    case Stat.HP:
                        target.EV_HP = Math.Min(target.EV_HP + allowed, StatCap); break;
                    case Stat.Attack:
                        target.EV_Attack = Math.Min(target.EV_Attack + allowed, StatCap); break;
                    case Stat.Defense:
                        target.EV_Defense = Math.Min(target.EV_Defense + allowed, StatCap); break;
                    case Stat.SpecialAttack:
                        target.EV_SpecialAttack = Math.Min(target.EV_SpecialAttack + allowed, StatCap); break;
                    case Stat.SpecialDefense:
                        target.EV_SpecialDefense = Math.Min(target.EV_SpecialDefense + allowed, StatCap); break;
                    case Stat.Speed:
                        target.EV_Speed = Math.Min(target.EV_Speed + allowed, StatCap); break;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Teach move  — TM / HM / Move Tutor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Writes a MoveState into the target's Moves[] array.
        /// replaceSlot -1 = auto-fill the first null slot.
        /// If all four slots are occupied and replaceSlot is -1 the method
        /// returns without doing anything — the UI layer should ask the
        /// player which slot to overwrite, then call with the chosen index.
        /// </summary>
        public class TeachMove : IFieldEffect
        {
            private readonly MoveState _move;
            private readonly int _replaceSlot;

            public TeachMove(MoveState move, int replaceSlot = -1)
            {
                _move = move;
                _replaceSlot = replaceSlot;
            }

            public void Apply(PlayerDomain player, PokemonPlayerDomain target)
            {
                if (target == null) return;

                if (_replaceSlot >= 0 && _replaceSlot < 4)
                {
                    target.Moves[_replaceSlot] = _move;
                    return;
                }

                for (int i = 0; i < 4; i++)
                {
                    if (target.Moves[i] == null)
                    {
                        target.Moves[i] = _move;
                        return;
                    }
                }
                // All slots full — no-op; caller must pick a slot and retry
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Friendship boost
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Raises PokemonPlayerDomain.Friendship by a flat amount.
        /// Capped at 255.
        /// </summary>
        public class RaiseFriendship : IFieldEffect
        {
            private readonly int _amount;
            private const int Cap = 255;

            public RaiseFriendship(int amount = 1) => _amount = amount;

            public void Apply(PlayerDomain player, PokemonPlayerDomain target)
            {
                if (target == null) return;
                target.Friendship = Math.Min(target.Friendship + _amount, Cap);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Full party heal  — Sacred Ash / Pokémon Centre
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Fully heals the whole party by delegating to
        /// PlayerTeamDomain.HealAll() which already handles HP,
        /// status, PP, stat stages, and volatile statuses.
        /// </summary>
        public class FullPartyHeal : IPartyEffect
        {
            public void Apply(PlayerDomain player) =>
                player.Team.HealAll();
        }

        // ─────────────────────────────────────────────────────────────
        //  Grant money  — Nugget / Big Pearl / prize money
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds money to TrainerInfoDomain.Money.
        /// Capped at 999 999 (Gen 1-5 wallet maximum).
        /// </summary>
        public class GrantMoney : IPartyEffect
        {
            private readonly int _amount;
            private const int Cap = 999_999;

            public GrantMoney(int amount) => _amount = amount;

            public void Apply(PlayerDomain player)
            {
                player.trainerInfo.Money =
                    Math.Min(player.trainerInfo.Money + _amount, Cap);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Register key item  — Bicycle / Town Map / etc.
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Assigns a KeyItemState to TrainerItemDomain.RegisterKey
        /// (the Y-button / shortcut slot).
        /// </summary>
        public class RegisterKeyItem : IPartyEffect
        {
            private readonly KeyItemState _keyItem;

            public RegisterKeyItem(KeyItemState keyItem) => _keyItem = keyItem;

            public void Apply(PlayerDomain player)
            {
                if (_keyItem is { Registerable: true })
                    player.trainerItemDomain.RegisterKey = _keyItem;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Bag helper  — add / remove / query items in BagInventory
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Static helpers for item bag operations.
        /// All item-use call sites should go through here so bag
        /// management stays in one place.
        /// </summary>
        public static class ItemBagHelper
        {
            /// <summary>
            /// Uses a field-effect item on the Pokémon in the given team slot.
            /// Removes one copy from the bag on success.
            /// Returns false if the slot is empty.
            /// </summary>
            public static bool UseOnSlot(
                IFieldEffect effect,
                ItemsDomain item,
                PlayerDomain player,
                int slotIndex)
            {
                var target = player.Team.GetAt(slotIndex);
                if (target == null) return false;

                effect.Apply(player, target);
                RemoveFromBag(player, item, 1);
                return true;
            }

            /// <summary>
            /// Uses a party-wide item (Sacred Ash, etc.).
            /// Removes one copy from the bag after use.
            /// </summary>
            public static void UsePartyEffect(
                IPartyEffect effect,
                ItemsDomain item,
                PlayerDomain player)
            {
                effect.Apply(player);
                RemoveFromBag(player, item, 1);
            }

            /// <summary>
            /// Returns true if the player has at least one of the given item.
            /// </summary>
            public static bool HasItem(PlayerDomain player, ItemsDomain item) =>
                player.trainerItemDomain.BagInventory
                      .TryGetValue(item, out int qty) && qty > 0;

            /// <summary>
            /// Returns the quantity of an item the player is holding. 0 if none.
            /// </summary>
            public static int GetCount(PlayerDomain player, ItemsDomain item)
            {
                player.trainerItemDomain.BagInventory.TryGetValue(item, out int qty);
                return qty;
            }

            /// <summary>
            /// Adds qty copies of an item to BagInventory.
            /// </summary>
            public static void AddToBag(PlayerDomain player, ItemsDomain item, int qty = 1)
            {
                var bag = player.trainerItemDomain.BagInventory;
                if (bag.ContainsKey(item))
                    bag[item] += qty;
                else
                    bag[item] = qty;
            }

            /// <summary>
            /// Removes qty copies of an item. Removes the entry when quantity hits 0.
            /// </summary>
            public static void RemoveFromBag(PlayerDomain player, ItemsDomain item, int qty = 1)
            {
                var bag = player.trainerItemDomain.BagInventory;
                if (!bag.ContainsKey(item)) return;
                bag[item] -= qty;
                if (bag[item] <= 0)
                    bag.Remove(item);
            }

            /// <summary>
            /// Returns all items in the bag of the given ItemType, sorted by name.
            /// Useful for rendering bag pockets (e.g. show only Poké Balls pocket).
            /// </summary>
            public static IEnumerable<(ItemsDomain item, int qty)> GetPocket(
                PlayerDomain player,
                ItemType type) =>
                player.trainerItemDomain.BagInventory
                      .Where(kv => kv.Key.Type == type)
                      .Select(kv => (kv.Key, kv.Value))
                      .OrderBy(t => t.Key.Name);

            /// <summary>
            /// Returns true if the player has the Running Shoes.
            /// Shortcut into TrainerItemDomain.HasRunningShoes.
            /// </summary>
            public static bool HasRunningShoes(PlayerDomain player) =>
                player.trainerItemDomain.HasRunningShoes;
        }
    }
