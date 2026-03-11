// Design: Composite pattern — And/Or/Not combine conditions.
// Design: Specification pattern — each class encodes one named battle condition.
// UserCondition / OpponentCondition: Adapter pattern (wraps PokemonDomain condition for BattleDomain).
// Layer: Domain/Move — concrete condition implementations.
// ICondition<T> and ITarget interfaces live in Interface/Move/IConditionAndTarget.cs.
// NOTE: Probability uses RandomHelper — no inline new Random() anywhere in this file.

using PokemonGame.Enums.Battle;
using PokemonGame.Interface.Move;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Helper;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Domain.Move
{
    // ── Condition Combinators ─────────────────────────────────────────────────

    internal class And<T> : ICondition<T>
    {
        private readonly ICondition<T> left;
        private readonly ICondition<T> right;
        public And(ICondition<T> left, ICondition<T> right) { this.left = left; this.right = right; }
        public bool Check(T entity) => left.Check(entity) && right.Check(entity);
    }

    internal class Or<T> : ICondition<T>
    {
        private readonly ICondition<T> left;
        private readonly ICondition<T> right;
        public Or(ICondition<T> left, ICondition<T> right) { this.left = left; this.right = right; }
        public bool Check(T entity) => left.Check(entity) || right.Check(entity);
    }

    internal class Not<T> : ICondition<T>
    {
        private readonly ICondition<T> inner;
        public Not(ICondition<T> inner) { this.inner = inner; }
        public bool Check(T entity) => !inner.Check(entity);
    }

    // ── Probability ───────────────────────────────────────────────────────────

    // Uses RandomHelper.NextBool — do not use new Random() here.
    internal class Probability<T> : ICondition<T>
    {
        private readonly double probability;
        public Probability(double probability)
        {
            this.probability = MathHelper.Clamp(probability, 0.0, 1.0);
        }
        public bool Check(T entity) => RandomHelper.NextBool(probability);
    }

    // Convenience alias — most common usage is against BattleDomain.
    internal class Probability : Probability<BattleDomain>
    {
        public Probability(double probability) : base(probability) { }
    }

    // ── Battle Conditions ─────────────────────────────────────────────────────

    internal class IsWeatherActive : ICondition<BattleDomain>
    {
        private readonly Weather weather;
        public IsWeatherActive(Weather weather) { this.weather = weather; }
        public bool Check(BattleDomain battle) => battle.IsWeatherActive(weather);
    }

    internal class IsBattleOver : ICondition<BattleDomain>
    {
        public bool Check(BattleDomain battle) => battle.IsBattleOver;
    }

    // ── Pokemon Conditions ────────────────────────────────────────────────────

    internal class HasStatus : ICondition<PokemonDomain>
    {
        private readonly StatusCondition status;
        public HasStatus(StatusCondition status) { this.status = status; }
        public bool Check(PokemonDomain pokemon) => pokemon.Status == status;
    }

    internal class HasVolatile : ICondition<PokemonDomain>
    {
        private readonly VolatileStatus status;
        public HasVolatile(VolatileStatus status) { this.status = status; }
        public bool Check(PokemonDomain pokemon) => pokemon.HasVolatileStatus(status);
    }

    internal class IsFainted : ICondition<PokemonDomain>
    {
        public bool Check(PokemonDomain pokemon) => pokemon.IsFainted;
    }

    internal class IsFullHP : ICondition<PokemonDomain>
    {
        public bool Check(PokemonDomain pokemon) => pokemon.CurrentHP == pokemon.MaxHP;
    }

    internal class HPBelow : ICondition<PokemonDomain>
    {
        private readonly double fraction;
        public HPBelow(double fraction) { this.fraction = fraction; }
        public bool Check(PokemonDomain pokemon) => pokemon.HPFraction < fraction;
    }

    internal class HasType : ICondition<PokemonDomain>
    {
        private readonly PokemonType type;
        public HasType(PokemonType type) { this.type = type; }
        public bool Check(PokemonDomain pokemon) => pokemon.HasType(type);
    }

    // Adapter: wraps a PokemonDomain condition so it can be used as ICondition<BattleDomain>.
    internal class UserCondition : ICondition<BattleDomain>
    {
        private readonly ICondition<PokemonDomain> inner;
        public UserCondition(ICondition<PokemonDomain> inner) { this.inner = inner; }
        public bool Check(BattleDomain battle) => inner.Check(battle.ActiveUser);
    }

    internal class OpponentCondition : ICondition<BattleDomain>
    {
        private readonly ICondition<PokemonDomain> inner;
        public OpponentCondition(ICondition<PokemonDomain> inner) { this.inner = inner; }
        public bool Check(BattleDomain battle) => inner.Check(battle.ActiveOpponent);
    }

    // ── Target Implementations ────────────────────────────────────────────────
    // ITarget interface lives in Interface/Move/IConditionAndTarget.cs.

    internal class AttackerTarget : ITarget
    {
        public PokemonDomain Resolve(BattleDomain battle) => battle.ActiveUser;
    }

    internal class DefenderTarget : ITarget
    {
        public PokemonDomain Resolve(BattleDomain battle) => battle.ActiveOpponent;
    }

    // Always resolves to a specific pokemon regardless of attacker/defender roles.
    // Useful for field effects, weather damage, end-of-turn effects.
    internal class SpecificTarget : ITarget
    {
        private readonly PokemonDomain pokemon;
        public SpecificTarget(PokemonDomain pokemon) { this.pokemon = pokemon; }
        public PokemonDomain Resolve(BattleDomain battle) => pokemon;
    }
}
