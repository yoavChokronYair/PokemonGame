using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Helper.MathHelper
{
    public static class ExperienceHelper
    {
        private const int MaxLevel = 100;

        /// <summary>
        /// Returns the total experience required to reach <paramref name="level"/>.
        /// Level 1 always returns 0.
        /// </summary>
        public static int GetTotalExpForLevel(int level, GrowthRateType growthRate)
        {
            if (level <= 1) return 0;
            if (level > MaxLevel) level = MaxLevel;

            return growthRate switch
            {
                GrowthRateType.Erratic => CalcErratic(level),
                GrowthRateType.Fast => CalcFast(level),
                GrowthRateType.MediumFast => CalcMediumFast(level),
                GrowthRateType.MediumSlow => CalcMediumSlow(level),
                GrowthRateType.Slow => CalcSlow(level),
                GrowthRateType.Fluctuating => CalcFluctuating(level),
                _ => throw new ArgumentOutOfRangeException(nameof(growthRate))
            };
        }

        /// <summary>
        /// Returns the experience needed to advance from <paramref name="currentLevel"/>
        /// to the next level (i.e. the gap between the two thresholds).
        /// Returns 0 at max level.
        /// </summary>
        public static int GetExpToNextLevel(int currentLevel, GrowthRateType growthRate)
        {
            if (currentLevel >= MaxLevel) return 0;

            int current = GetTotalExpForLevel(currentLevel, growthRate);
            int next = GetTotalExpForLevel(currentLevel + 1, growthRate);
            return next - current;
        }

        // -------------------------------------------------------------------------
        // Growth rate formulas — all sourced from the official Gen III–VIII specs.
        // -------------------------------------------------------------------------

        // Erratic: n^3 * modifier based on level band
        private static int CalcErratic(int n)
        {
            if (n < 50) return (int)(Math.Pow(n, 3) * (100 - n) / 50);
            if (n < 68) return (int)(Math.Pow(n, 3) * (150 - n) / 100);
            if (n < 98) return (int)(Math.Pow(n, 3) * ((1911 - 10 * n) / 3) / 500);
            /* n < 100 */
            return (int)(Math.Pow(n, 3) * (160 - n) / 100);
        }

        // Fast: 4n^3 / 5
        private static int CalcFast(int n) =>
            (int)(4 * Math.Pow(n, 3) / 5);

        // Medium Fast (standard): n^3
        private static int CalcMediumFast(int n) =>
            (int)Math.Pow(n, 3);

        // Medium Slow: 6/5 n^3 − 15n^2 + 100n − 140
        private static int CalcMediumSlow(int n) =>
            (int)(6.0 / 5.0 * Math.Pow(n, 3) - 15 * Math.Pow(n, 2) + 100 * n - 140);

        // Slow: 5n^3 / 4
        private static int CalcSlow(int n) =>
            (int)(5 * Math.Pow(n, 3) / 4);

        // Fluctuating: n^3 * modifier based on level band
        private static int CalcFluctuating(int n)
        {
            double factor;
            if (n < 15) factor = (Math.Floor((n + 1.0) / 3) + 24) / 50;
            else if (n < 36) factor = (n + 14.0) / 50;
            else factor = (Math.Floor(n / 2.0) + 32) / 50;

            return (int)(Math.Pow(n, 3) * factor);
        }
    }
}
