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
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Domain.Move
{
    #region Combinators

    internal class Product : INumber
    {
        private readonly INumber left;
        private readonly INumber right;
        public Product(INumber left, INumber right) { this.left = left; this.right = right; }
        public double Evaluate(BattleState battle) => left.Evaluate(battle) * right.Evaluate(battle);
    }

    internal class Sum : INumber
    {
        private readonly INumber left;
        private readonly INumber right;
        public Sum(INumber left, INumber right) { this.left = left; this.right = right; }
        public double Evaluate(BattleState battle) => left.Evaluate(battle) + right.Evaluate(battle);
    }

    internal class Quotient : INumber
    {
        private readonly INumber numerator;
        private readonly INumber denominator;
        public Quotient(INumber numerator, INumber denominator) { this.numerator = numerator; this.denominator = denominator; }
        public double Evaluate(BattleState battle)
        {
            double d = denominator.Evaluate(battle);
            if (d == 0) return 0;
            return numerator.Evaluate(battle) / d;
        }
    }

    #endregion

    // A fixed constant value — e.g. Exactly(40) for a base 40 power move.
    internal class Exactly : INumber
    {
        private readonly double value;
        public Exactly(double value) { this.value = value; }
        public double Evaluate(BattleState battle) => value;
    }

    // Uniform random in [min, max] — uses RandomHelper, no new Random().
    internal class Between : INumber
    {
        private readonly double min;
        private readonly double max;

        public Between(double min, double max) { this.min = min; this.max = max; }

        public double Evaluate(BattleState battle)
            => RandomHelper.NextDouble() * (max - min) + min;
    }

    // Weighted random pick — e.g. { (2, 35%), (3, 35%), (4, 15%), (5, 15%) } for multi-hit.
    // Uses RandomHelper, no new Random().
    internal class Weighted : INumber
    {
        private readonly List<(double value, double weight)> entries;

        public Weighted(List<(double value, double weight)> entries) { this.entries = entries; }

        public double Evaluate(BattleState battle)
        {
            double total = entries.Sum(e => e.weight);
            double roll = RandomHelper.NextDouble() * total;
            double cumulative = 0;
            foreach (var (value, weight) in entries)
            {
                cumulative += weight;
                if (roll <= cumulative) return value;
            }
            return entries.Last().value;
        }
    }

    // ── Battle-state Accessors ────────────────────────────────────────────────

    internal class MaxHP : INumber
    {
        private readonly ITarget target;
        public MaxHP(ITarget target) { this.target = target; }
        public double Evaluate(BattleState battle) => target.Resolve(battle).MaxHP;
    }

    internal class CurrentHP : INumber
    {
        private readonly ITarget target;
        public CurrentHP(ITarget target) { this.target = target; }
        public double Evaluate(BattleState battle) => target.Resolve(battle).CurrentHP;
    }

    internal class Level : INumber
    {
        private readonly ITarget target;
        public Level(ITarget target) { this.target = target; }
        public double Evaluate(BattleState battle) => target.Resolve(battle).Level;
    }

    internal class LastDamageDealt : INumber
    {
        private readonly ITarget target;
        public LastDamageDealt(ITarget target) { this.target = target; }
        public double Evaluate(BattleState battle) => target.Resolve(battle).LastDamageDealt;
    }
}
