using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Helper;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Domain.Move
{
    // ── Condition Interface ───────────────────────────────────────────────────

    internal interface ICondition<T>
    {
        bool Check(T entity);
    }

    // ── Condition Combinators ─────────────────────────────────────────────────

    internal class And<T> : ICondition<T>
    {
        private readonly ICondition<T> left;
        private readonly ICondition<T> right;

        public And(ICondition<T> left, ICondition<T> right)
        {
            this.left = left;
            this.right = right;
        }

        public bool Check(T entity) => left.Check(entity) && right.Check(entity);
    }

    internal class Or<T> : ICondition<T>
    {
        private readonly ICondition<T> left;
        private readonly ICondition<T> right;

        public Or(ICondition<T> left, ICondition<T> right)
        {
            this.left = left;
            this.right = right;
        }

        public bool Check(T entity) => left.Check(entity) || right.Check(entity);
    }

    internal class Not<T> : ICondition<T>
    {
        private readonly ICondition<T> inner;

        public Not(ICondition<T> inner)
        {
            this.inner = inner;
        }

        public bool Check(T entity) => !inner.Check(entity);
    }

    // ── Probability ───────────────────────────────────────────────────────────

    // Works for any T — probability doesn't need to inspect the entity
    internal class Probability<T> : ICondition<T>
    {
        private readonly double probability; // 0.0 to 1.0
        private static readonly Random rng = new();

        public Probability(double probability)
        {
            this.probability = MathHelper.Clamp(probability, 0.0, 1.0);
        }

        public bool Check(T entity) => rng.NextDouble() < probability;
    }

    // Convenience alias — most common usage is against BattleDomain
    internal class Probability : Probability<BattleDomain>
    {
        public Probability(double probability) : base(probability) { }
    }

    // ── Battle Conditions ─────────────────────────────────────────────────────

    internal class IsWeatherActive : ICondition<BattleDomain>
    {
        private readonly Weather weather;

        public IsWeatherActive(Weather weather)
        {
            this.weather = weather;
        }

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

        public HasStatus(StatusCondition status)
        {
            this.status = status;
        }

        public bool Check(PokemonDomain pokemon) => pokemon.Status == status;
    }

    internal class HasVolatile : ICondition<PokemonDomain>
    {
        private readonly VolatileStatus status;

        public HasVolatile(VolatileStatus status)
        {
            this.status = status;
        }

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
        private readonly double fraction; // e.g. 0.5 = below half HP

        public HPBelow(double fraction)
        {
            this.fraction = fraction;
        }

        public bool Check(PokemonDomain pokemon) => pokemon.HPFraction < fraction;
    }

    internal class HasType : ICondition<PokemonDomain>
    {
        private readonly PokemonType type;

        public HasType(PokemonType type)
        {
            this.type = type;
        }

        public bool Check(PokemonDomain pokemon) => pokemon.HasType(type);
    }

    // Wraps a PokemonDomain condition so it can be used as an ICondition<BattleDomain>
    // e.g. check if the active user is below half HP
    internal class UserCondition : ICondition<BattleDomain>
    {
        private readonly ICondition<PokemonDomain> inner;

        public UserCondition(ICondition<PokemonDomain> inner)
        {
            this.inner = inner;
        }

        public bool Check(BattleDomain battle) => inner.Check(battle.ActiveUser);
    }

    internal class OpponentCondition : ICondition<BattleDomain>
    {
        private readonly ICondition<PokemonDomain> inner;

        public OpponentCondition(ICondition<PokemonDomain> inner)
        {
            this.inner = inner;
        }

        public bool Check(BattleDomain battle) => inner.Check(battle.ActiveOpponent);
    }
    
    // ── Target Interface & Implementations ────────────────────────────────────

    internal interface ITarget
    {
        PokemonDomain Resolve(BattleDomain battle);
    }

    internal class AttackerTarget : ITarget
    {
        public PokemonDomain Resolve(BattleDomain battle) => battle.ActiveUser;
    }

    internal class DefenderTarget : ITarget
    {
        public PokemonDomain Resolve(BattleDomain battle) => battle.ActiveOpponent;
    }

    // Always resolves to a specific pokemon regardless of attacker/defender roles
    // Useful for field effects, weather damage, end-of-turn effects
    internal class SpecificTarget : ITarget
    {
        private readonly PokemonDomain pokemon;

        public SpecificTarget(PokemonDomain pokemon)
        {
            this.pokemon = pokemon;
        }

        public PokemonDomain Resolve(BattleDomain battle) => pokemon;
    }
}