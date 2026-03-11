// Design: Composite pattern — And/Or/Not combine conditions.
// Design: Specification pattern — each class encodes one named battle condition.
// UserCondition / OpponentCondition: Adapter pattern (wraps PokemonDomain condition for BattleDomain).
// Layer: Domain/Move — concrete condition implementations.
// ICondition<T> and ITarget interfaces live in Interface/Move/IConditionAndTarget.cs.
// NOTE: Probability uses RandomHelper — no inline new Random() anywhere in this file.

using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Model.Model.Helper.DesignPatterns
{
    // ── Condition Combinators ─────────────────────────────────────────────────

    internal class And<T> : ICondition<T>
    {
        private readonly ICondition<T> _left;
        private readonly ICondition<T> _right;
        public And(ICondition<T> left, ICondition<T> right) { _left = left; _right = right; }
        public bool Check(T entity) => _left.Check(entity) && _right.Check(entity);
    }

    internal class Or<T> : ICondition<T>
    {
        private readonly ICondition<T> _left;
        private readonly ICondition<T> _right;
        public Or(ICondition<T> left, ICondition<T> right) { _left = left; _right = right; }
        public bool Check(T entity) => _left.Check(entity) || _right.Check(entity);
    }

    internal class Not<T> : ICondition<T>
    {
        private readonly ICondition<T> _inner;
        public Not(ICondition<T> inner) { _inner = inner; }
        public bool Check(T entity) => !_inner.Check(entity);
    }

    // ── Probability ───────────────────────────────────────────────────────────

    // Uses RandomHelper.NextBool — do not use new Random() here.
    internal class Probability<T> : ICondition<T>
    {
        private readonly double _probability;
        public Probability(double probability)
        {
            _probability = MathHelper.Clamp(probability, 0.0, 1.0);
        }
        public bool Check(T entity) => RandomHelper.NextBool(_probability);
    }

    // Convenience alias — most common usage is against BattleDomain.
    internal class Probability : Probability<BattleState>
    {
        public Probability(double probability) : base(probability) { }
    }

    // ── Battle Conditions ─────────────────────────────────────────────────────

    internal class IsWeatherActive : ICondition<BattleState>
    {
        private readonly Weather _weather;
        public IsWeatherActive(Weather weather) { _weather = weather; }
        public bool Check(BattleState battle) => battle.WeatherService.IsWeatherActive(_weather);
    }

    internal class IsBattleOver : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.IsBattleOver;
    }

    // ── Pokemon Conditions ────────────────────────────────────────────────────

    internal class HasStatus : ICondition<PokemonState>
    {
        private readonly StatusCondition _status;
        public HasStatus(StatusCondition status) { _status = status; }
        public bool Check(PokemonState pokemon) => pokemon.PokemonStatusCondition() == _status;
    }

    internal class HasVolatile : ICondition<PokemonState>
    {
        private readonly VolatileStatus _status;
        public HasVolatile(VolatileStatus status) { _status = status; }
        public bool Check(PokemonState pokemon) => pokemon.HasVolatileStatus(_status);
    }

    internal class IsFainted : ICondition<PokemonState>
    {
        public bool Check(PokemonState pokemon) => pokemon.IsFainted;
    }

    internal class IsFullHP : ICondition<PokemonState>
    {
        public bool Check(PokemonState pokemon) => pokemon.CurrentHP == pokemon.MaxHP;
    }

    internal class HPBelow : ICondition<PokemonState>
    {
        private readonly double _fraction;
        public HPBelow(double fraction) { _fraction = fraction; }
        public bool Check(PokemonState pokemon) => pokemon.GetHPFraction() < _fraction;
    }

    internal class HasType : ICondition<PokemonState>
    {
        private readonly PokemonType _type;
        public HasType(PokemonType type) { _type = type; }
        public bool Check(PokemonState pokemon) => pokemon.HasType(_type);
    }

    // Adapter: wraps a PokemonDomain condition so it can be used as ICondition<BattleDomain>.
    internal class UserCondition : ICondition<BattleState>
    {
        private readonly ICondition<PokemonState> _inner;
        public UserCondition(ICondition<PokemonState> inner) { _inner = inner; }
        public bool Check(BattleState battle) => _inner.Check(battle.Attacker);
    }

    internal class OpponentCondition : ICondition<BattleState>
    {
        private readonly ICondition<PokemonState> _inner;
        public OpponentCondition(ICondition<PokemonState> inner) { _inner = inner; }
        public bool Check(BattleState battle) => _inner.Check(battle.Attacker);
    }

    // ── Target Implementations ────────────────────────────────────────────────
    // ITarget interface lives in Interface/Move/IConditionAndTarget.cs.

    internal class AttackerTarget : ITarget
    {
        public PokemonState Resolve(BattleState battle) => battle.Attacker;
    }

    internal class DefenderTarget : ITarget
    {
        public PokemonState Resolve(BattleState battle) => battle.Attacker;
    }

    // Always resolves to a specific pokemon regardless of attacker/defender roles.
    // Useful for field effects, weather damage, end-of-turn effects.
    internal class SpecificTarget : ITarget
    {
        private readonly PokemonState _pokemon;
        public SpecificTarget(PokemonState pokemon) { _pokemon = pokemon; }
        public PokemonState Resolve(BattleState battle) => _pokemon;
    }
}
