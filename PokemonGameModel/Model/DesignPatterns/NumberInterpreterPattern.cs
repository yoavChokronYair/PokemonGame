// Design: Expression tree / Interpreter pattern.
// Product / Sum / Quotient: arithmetic combinators.
// Exactly / Between / Weighted: leaf value providers.
// MaxHP / CurrentHP / Level / LastDamageDealt: battle-state accessors.
// Layer: Domain/Move — concrete INumber implementations.
// INumber interface lives in Interface/Move/INumber.cs.
// NOTE: Between and Weighted use RandomHelper — no inline new Random() in this file.

using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.DesignPatterns
{
    #region Combinators

    public class Product : INumber
    {
        private readonly INumber _left;
        private readonly INumber _right;
        public Product(INumber left, INumber right) { _left = left; _right = right; }
        public double Evaluate(BattleState battle) => _left.Evaluate(battle) * _right.Evaluate(battle);
    }

    public class Sum : INumber
    {
        private readonly INumber _left;
        private readonly INumber _right;
        public Sum(INumber left, INumber right) { _left = left; _right = right; }
        public double Evaluate(BattleState battle) => _left.Evaluate(battle) + _right.Evaluate(battle);
    }

    public class Quotient : INumber
    {
        private readonly INumber _numerator;
        private readonly INumber _denominator;

        public Quotient(INumber numerator, INumber denominator)
        {
            _numerator = numerator;
            _denominator = denominator;
        }

        public double Evaluate(BattleState battle)
        {
            double denominator = _denominator.Evaluate(battle);

            if (Math.Abs(denominator) < double.Epsilon)
            {
                throw new DivideByZeroException(
                    "Cannot evaluate Quotient because denominator evaluated to zero.");
            }

            return _numerator.Evaluate(battle) / denominator;
        }
    }

    #endregion

    // A fixed constant value — e.g. Exactly(40) for a base 40 power move.
    public class Exactly : INumber
    {
        private readonly double _value;
        public Exactly(double value) { _value = value; }
        public double Evaluate(BattleState battle) => _value;
    }

    // Uniform random in [min, max] — uses RandomHelper, no new Random().
    public class Between : INumber
    {
        private readonly int _min;
        private readonly int _max;

        public Between(int min, int max)
        {
            if (min > max)
                throw new ArgumentException("Between min cannot be greater than max.");

            _min = min;
            _max = max;
        }

        public double Evaluate(BattleState battle)
        {
            return RandomHelper.Next(_min, _max + 1);
        }
    }

    // Weighted random pick — e.g. { (2, 35%), (3, 35%), (4, 15%), (5, 15%) } for multi-hit.
    // Uses RandomHelper, no new Random().
    public class Weighted : INumber
    {
        private readonly List<(double value, double weight)> _entries;

        public Weighted(List<(double value, double weight)> entries)
        {
            if (entries == null || entries.Count == 0)
                throw new ArgumentException("Weighted requires at least one entry.");

            if (entries.Any(e => e.weight < 0))
                throw new ArgumentException("Weighted entries cannot have negative weight.");

            double total = entries.Sum(e => e.weight);

            if (total <= 0)
                throw new ArgumentException("Weighted total weight must be greater than zero.");

            _entries = entries;
        }

        public double Evaluate(BattleState battle)
        {
            double total = _entries.Sum(e => e.weight);
            double roll = RandomHelper.NextDouble() * total;
            double cumulative = 0;

            foreach (var (value, weight) in _entries)
            {
                cumulative += weight;

                if (roll < cumulative)
                    return value;
            }

            return _entries[_entries.Count - 1].value;
        }
    }

    // ── Battle-state Accessors ────────────────────────────────────────────────

    public class MaxHP : INumber
    {
        private readonly ITarget _target;
        public MaxHP(ITarget target) { _target = target; }
        public double Evaluate(BattleState battle) => _target.Resolve(battle).MaxHP;
    }

    public class CurrentHP : INumber
    {
        private readonly ITarget _target;
        public CurrentHP(ITarget target) { _target = target; }
        public double Evaluate(BattleState battle) => _target.Resolve(battle).CurrentHP;
    }

    public class Level : INumber
    {
        private readonly ITarget _target;
        public Level(ITarget target) { _target = target; }
        public double Evaluate(BattleState battle) => _target.Resolve(battle).Level;
    }

    public class LastDamageDealt : INumber
    {
        public double Evaluate(BattleState battle)
        {
            return battle.LastDamageDealt;
        }
    }
}
