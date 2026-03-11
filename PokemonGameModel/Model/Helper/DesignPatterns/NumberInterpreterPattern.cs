// Design: Expression tree / Interpreter pattern.
// Product / Sum / Quotient: arithmetic combinators.
// Exactly / Between / Weighted: leaf value providers.
// MaxHP / CurrentHP / Level / LastDamageDealt: battle-state accessors.
// Layer: Domain/Move — concrete INumber implementations.
// INumber interface lives in Interface/Move/INumber.cs.
// NOTE: Between and Weighted use RandomHelper — no inline new Random() in this file.

using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Model.Helper.DesignPatterns
{
    #region Combinators

    internal class Product : INumber
    {
        private readonly INumber _left;
        private readonly INumber _right;
        public Product(INumber left, INumber right) { _left = left; _right = right; }
        public double Evaluate(BattleState battle) => _left.Evaluate(battle) * _right.Evaluate(battle);
    }

    internal class Sum : INumber
    {
        private readonly INumber _left;
        private readonly INumber _right;
        public Sum(INumber left, INumber right) { _left = left; _right = right; }
        public double Evaluate(BattleState battle) => _left.Evaluate(battle) + _right.Evaluate(battle);
    }

    internal class Quotient : INumber
    {
        private readonly INumber _numerator;
        private readonly INumber _denominator;
        public Quotient(INumber numerator, INumber denominator) { _numerator = numerator; _denominator = denominator; }
        public double Evaluate(BattleState battle)
        {
            double d = _denominator.Evaluate(battle);
            if (d == 0)
            {
                return 0;
            }

            return _numerator.Evaluate(battle) / d;
        }
    }

    #endregion

    // A fixed constant value — e.g. Exactly(40) for a base 40 power move.
    internal class Exactly : INumber
    {
        private readonly double _value;
        public Exactly(double value) { _value = value; }
        public double Evaluate(BattleState battle) => _value;
    }

    // Uniform random in [min, max] — uses RandomHelper, no new Random().
    internal class Between : INumber
    {
        private readonly double _min;
        private readonly double _max;

        public Between(double min, double max) { _min = min; _max = max; }

        public double Evaluate(BattleState battle)
            => RandomHelper.NextDouble() * (_max - _min) + _min;
    }

    // Weighted random pick — e.g. { (2, 35%), (3, 35%), (4, 15%), (5, 15%) } for multi-hit.
    // Uses RandomHelper, no new Random().
    internal class Weighted : INumber
    {
        private readonly List<(double value, double weight)> _entries;

        public Weighted(List<(double value, double weight)> entries) { _entries = entries; }

        public double Evaluate(BattleState battle)
        {
            double total = _entries.Sum(e => e.weight);
            double roll = RandomHelper.NextDouble() * total;
            double cumulative = 0;
            foreach (var (value, weight) in _entries)
            {
                cumulative += weight;
                if (roll <= cumulative)
                {
                    return value;
                }
            }
            return _entries.Last().value;
        }
    }

    // ── Battle-state Accessors ────────────────────────────────────────────────

    internal class MaxHP : INumber
    {
        private readonly ITarget _target;
        public MaxHP(ITarget target) { _target = target; }
        public double Evaluate(BattleState battle) => _target.Resolve(battle).MaxHP;
    }

    internal class CurrentHP : INumber
    {
        private readonly ITarget _target;
        public CurrentHP(ITarget target) { _target = target; }
        public double Evaluate(BattleState battle) => _target.Resolve(battle).CurrentHP;
    }

    internal class Level : INumber
    {
        private readonly ITarget _target;
        public Level(ITarget target) { _target = target; }
        public double Evaluate(BattleState battle) => _target.Resolve(battle).Level;
    }

    internal class LastDamageDealt : INumber
    {
        private readonly ITarget _target;
        public LastDamageDealt(ITarget target) { _target = target; }
        public double Evaluate(BattleState battle) => _target.Resolve(battle).LastDamageDealt;
    }
}
