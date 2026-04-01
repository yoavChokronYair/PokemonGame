// Design: Composite pattern — And/Or/Not combine conditions.
// Design: Specification pattern — each class encodes one named battle condition.
// UserCondition / OpponentCondition: Adapter pattern (wraps PokemonDomain condition for BattleDomain).
// Layer: Domain/Move — concrete condition implementations.
// ICondition<T> and ITarget interfaces live in Interface/Move/IConditionAndTarget.cs.
// NOTE: Probability uses RandomHelper — no inline new Random() anywhere in this file.

using System.Reflection;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Model.Helper.MoveHelper;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Model.Model.Helper.DesignPatterns
{
    // ── Condition Combinators ─────────────────────────────────────────────────

    public class And<T> : ICondition<T>
    {
        private readonly ICondition<T> _left;
        private readonly ICondition<T> _right;
        public And(ICondition<T> left, ICondition<T> right) { _left = left; _right = right; }
        public bool Check(T entity) => _left.Check(entity) && _right.Check(entity);
    }

    public class Or<T> : ICondition<T>
    {
        private readonly ICondition<T> _left;
        private readonly ICondition<T> _right;
        public Or(ICondition<T> left, ICondition<T> right) { _left = left; _right = right; }
        public bool Check(T entity) => _left.Check(entity) || _right.Check(entity);
    }

    public class Not<T> : ICondition<T>
    {
        private readonly ICondition<T> _inner;
        public Not(ICondition<T> inner) { _inner = inner; }
        public bool Check(T entity) => !_inner.Check(entity);
    }

    // ── Probability ───────────────────────────────────────────────────────────

    // Uses RandomHelper.NextBool — do not use new Random() here.
    public class Probability<T> : ICondition<T>
    {
        private readonly double _probability;
        public Probability(double probability)
        {
            _probability = MathHelper.Clamp(probability, 0.0, 1.0);
        }
        public bool Check(T entity) => RandomHelper.NextBool(_probability);
    }

    // Convenience alias — most common usage is against BattleDomain.
    public class Probability : Probability<BattleState>
    {
        public Probability(double probability) : base(probability) { }
    }

    // ── Battle Conditions ─────────────────────────────────────────────────────
    public class IsNewPokemon : ICondition<BattleState>
    {
        // Checks if the Attacker has spent 0 full turns on the field
        public bool Check(BattleState battle) => battle.Attacker.turnsActive == 0;
    }

    public class IsWeatherActive : ICondition<BattleState>
    {
        private readonly Weather _weather;
        public IsWeatherActive(Weather weather) { _weather = weather; }
        public bool Check(BattleState battle) => battle.WeatherService.IsWeatherActive(_weather);
    }
    public class IsTerrainActive : ICondition<BattleState>
    {
        private readonly TerrainType _terrain;
        public IsTerrainActive(TerrainType terrain) { _terrain = terrain; }
        public bool Check(BattleState battle) => battle.TerrainService.CurrentTerrain == _terrain;
    }

    public class IsAnyTerrainActive : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.TerrainService.CurrentTerrain != TerrainType.None;
    }
    public class IsBattleOver : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.IsBattleOver;
    }

    // ── Pokemon Conditions ────────────────────────────────────────────────────
    public class WasHitByContact : ICondition<BattleState>
    {
        public bool Check(BattleState battle)
        {
            if(battle.LastUsedMove != null)
            {
                MoveState lastMoveState = (MoveState)battle.LastUsedMove;
                if(lastMoveState.Category == MoveCategory.Physical )
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class MoveHasTag : ICondition<BattleState>
    {
        private readonly MoveTag _tag;
        public MoveHasTag(MoveTag tag) { _tag = tag; }
        public bool Check(BattleState battle) 
        {
            if(battle.LastUsedMove != null)
            {
                return ((MoveState) battle.LastUsedMove).Tag == _tag;
            }
            return false;
        }
    }
   
    public class MoveIsCategory : ICondition<BattleState>
    {
        private readonly MoveCategory _category;
        public MoveIsCategory(MoveCategory category) { _category = category; }
        public bool Check(BattleState battle)
        {
            if(battle.LastUsedMove != null)
            {
                return ((MoveState)battle.LastUsedMove).Category == _category;
            }
            return false;
        }
    }
    public class DidKnockoutOpponent : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.Defender.IsFainted && !battle.Attacker.IsFainted;
    }
    public class TookDamageThisTurn : ICondition<PokemonState>
    {
        public bool Check(PokemonState pokemon) => pokemon.LastDamageTaken > 0;
    }
    public class HasAnyStatus : ICondition<PokemonState>
    {
        public bool Check(PokemonState pokemon) => pokemon.PokemonStatusCondition() != StatusCondition.None;
    }
    public class HasStatus : ICondition<PokemonState>
    {
        private readonly StatusCondition _status;
        public HasStatus(StatusCondition status) { _status = status; }
        public bool Check(PokemonState pokemon) => pokemon.PokemonStatusCondition() == _status;
    }
    //public class IsHoldingItem : ICondition<PokemonState>
    //{
    //    public bool Check(PokemonState pokemon) => pokemon.HeldItem != null;
    //}

    //public class WasItemConsumed : ICondition<PokemonState>
    //{
    //    // This requires a bool flag in PokemonState reset each switch-in
    //    public bool Check(PokemonState pokemon) => pokemon.ItemWasConsumedThisBattle;
    //}
    public class HasVolatile : ICondition<PokemonState>
    {
        private readonly VolatileStatus _status;
        public HasVolatile(VolatileStatus status) { _status = status; }
        public bool Check(PokemonState pokemon) => pokemon.HasVolatileStatus(_status);
    }
    public class HasBaseStatChanged : ICondition<BattleState>
    {
        public bool Check(BattleState entity)
        {
            // Competitive/Defiant only trigger if a stat was LOWERED
            return entity.Attacker.WasStatLoweredThisTurn;
        }
    }

    public class IsFainted : ICondition<PokemonState>
    {
        public bool Check(PokemonState pokemon) => pokemon.IsFainted;
    }

    public class IsFullHP : ICondition<PokemonState>
    {
        public bool Check(PokemonState pokemon) => pokemon.CurrentHP == pokemon.MaxHP;
    }

    public class HPBelow : ICondition<PokemonState>
    {
        private readonly double _fraction;
        public HPBelow(double fraction) { _fraction = fraction; }
        public bool Check(PokemonState pokemon) => pokemon.GetHPFraction() < _fraction;
    }

    public class HasType : ICondition<PokemonState>
    {
        private readonly PokemonType _type;
        public HasType(PokemonType type) { _type = type; }
        public bool Check(PokemonState pokemon) => pokemon.HasType(_type);
    }

    // Adapter: wraps a PokemonDomain condition so it can be used as ICondition<BattleDomain>.
    public class UserCondition : ICondition<BattleState>
    {
        private readonly ICondition<PokemonState> _inner;
        public UserCondition(ICondition<PokemonState> inner) { _inner = inner; }
        public bool Check(BattleState battle) => _inner.Check(battle.Attacker);
    }

    public class OpponentCondition : ICondition<BattleState>
    {
        private readonly ICondition<PokemonState> _inner;
        public OpponentCondition(ICondition<PokemonState> inner) { _inner = inner; }
        public bool Check(BattleState battle) => _inner.Check(battle.Defender); // ← fix
    }

    // ── Target Implementations ────────────────────────────────────────────────
    // ITarget interface lives in Interface/Move/IConditionAndTarget.cs.

    public class AttackerTarget : ITarget
    {
        public PokemonState Resolve(BattleState battle) => battle.Attacker;
    }

    public class DefenderTarget : ITarget
    {
        public PokemonState Resolve(BattleState battle) => battle.Defender;
    }

    // Always resolves to a specific pokemon regardless of attacker/defender roles.
    // Useful for field effects, weather damage, end-of-turn effects.
    public class SpecificTarget : ITarget
    {
        private readonly PokemonState _pokemon;
        public SpecificTarget(PokemonState pokemon) { _pokemon = pokemon; }
        public PokemonState Resolve(BattleState battle) => _pokemon;
    }
}
