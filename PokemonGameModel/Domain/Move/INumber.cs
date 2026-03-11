using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Domain.Move
{
    //TODO:change to actually working battler pokemon
    public interface INumber
    {
        public double Evaluate(BattleDomain battle);
    }

    #region Combinators

    public class Product : INumber
    {
        private readonly INumber left;
        private readonly INumber right;

        public Product(INumber left, INumber right)
        {
            this.left = left;
            this.right = right;
        }

        public double Evaluate(BattleDomain battle)
            => left.Evaluate(battle) * right.Evaluate(battle);
    }

    public class Sum : INumber
    {
        private readonly INumber left;
        private readonly INumber right;

        public Sum(INumber left, INumber right)
        {
            this.left = left;
            this.right = right;
        }

        public double Evaluate(BattleDomain battle)
            => left.Evaluate(battle) + right.Evaluate(battle);
    }

    public class Quotient : INumber
    {
        private readonly INumber numerator;
        private readonly INumber denominator;

        public Quotient(INumber numerator, INumber denominator)
        {
            this.numerator = numerator;
            this.denominator = denominator;
        }

        public double Evaluate(BattleDomain battle)
        {
            double d = denominator.Evaluate(battle);
            if (d == 0) return 0;
            return numerator.Evaluate(battle) / d;
        }
    }

    #endregion

    // A fixed constant value — e.g. Exactly(40) for a base 40 power move
    public class Exactly : INumber
    {
        private readonly double value;

        public Exactly(double value)
        {
            this.value = value;
        }

        public double Evaluate(BattleDomain battle) => value;
    }

    // Uniform random in [min, max] — e.g. damage roll variance
    public class Between : INumber
    {
        private readonly double min;
        private readonly double max;
        private static readonly Random rng = new();

        public Between(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        public double Evaluate(BattleDomain battle)
            => rng.NextDouble() * (max - min) + min;
    }

    // Weighted random pick from a list of (value, weight) pairs
    // e.g. Weighted { (2, 35%), (3, 35%), (4, 15%), (5, 15%) } for multi-hit
    public class Weighted : INumber
    {
        private readonly List<(double value, double weight)> entries;
        private static readonly Random rng = new();

        public Weighted(List<(double value, double weight)> entries)
        {
            this.entries = entries;
        }

        public double Evaluate(BattleDomain battle)
        {
            double total = entries.Sum(e => e.weight);
            double roll = rng.NextDouble() * total;
            double cumulative = 0;
            foreach (var (value, weight) in entries)
            {
                cumulative += weight;
                if (roll <= cumulative) return value;
            }
            return entries.Last().value;
        }
    }

    public class MaxHP : INumber
    {
        private readonly ITarget target;

        public MaxHP(ITarget target)
        {
            this.target = target;
        }

        public double Evaluate(BattleDomain battle)
            => target.Resolve(battle).MaxHP;
    }

    public class CurrentHP : INumber
    {
        private readonly ITarget target;

        public CurrentHP(ITarget target)
        {
            this.target = target;
        }

        public double Evaluate(BattleDomain battle)
            => target.Resolve(battle).CurrentHP;
    }

    public class Level : INumber
    {
        private readonly ITarget target;

        public Level(ITarget target)
        {
            this.target = target;
        }

        public double Evaluate(BattleDomain battle)
            => target.Resolve(battle).Level;
    }

    public class LastDamageDealt : INumber
    {
        private readonly ITarget target;

        public LastDamageDealt(ITarget target)
        {
            this.target = target;
        }

        public double Evaluate(BattleDomain battle)
            => target.Resolve(battle).LastDamageDealt;
    }
}