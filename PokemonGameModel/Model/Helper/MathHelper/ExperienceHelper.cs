using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Helper.MathHelper
{
    /// <summary>
    /// Handles Pokémon experience curves and level calculations.
    /// Official formulas from mainline Pokémon games (Gen III+).
    /// </summary>
    public static class ExperienceHelper
    {
        public const int MaxLevel = 100;

        // ─────────────────────────────────────────────────────────────────────
        // TOTAL EXPERIENCE
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns TOTAL accumulated experience required for a level.
        /// Level 1 = 0 EXP.
        /// </summary>
        public static int GetTotalExpForLevel(
            int level,
            GrowthRateType growthRate)
        {
            if (level <= 1)
                return 0;

            if (level > MaxLevel)
                level = MaxLevel;

            return growthRate switch
            {
                GrowthRateType.Erratic =>
                    CalcErratic(level),

                GrowthRateType.Fast =>
                    CalcFast(level),

                GrowthRateType.MediumFast =>
                    CalcMediumFast(level),

                GrowthRateType.MediumSlow =>
                    CalcMediumSlow(level),

                GrowthRateType.Slow =>
                    CalcSlow(level),

                GrowthRateType.Fluctuating =>
                    CalcFluctuating(level),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(growthRate),
                    growthRate,
                    null)
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // EXPERIENCE TO NEXT LEVEL
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns EXP needed to reach next level.
        /// </summary>
        public static int GetExpToNextLevel(
            int currentLevel,
            GrowthRateType growthRate)
        {
            if (currentLevel >= MaxLevel)
                return 0;

            int current =
                GetTotalExpForLevel(
                    currentLevel,
                    growthRate);

            int next =
                GetTotalExpForLevel(
                    currentLevel + 1,
                    growthRate);

            return next - current;
        }

        // ─────────────────────────────────────────────────────────────────────
        // LEVEL FROM EXPERIENCE
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the level corresponding to the total EXP.
        /// </summary>
        public static int GetLevelFromExperience(
            int experience,
            GrowthRateType growthRate)
        {
            if (experience <= 0)
                return 1;

            for (int level = 1; level <= MaxLevel; level++)
            {
                int required =
                    GetTotalExpForLevel(level, growthRate);

                if (experience < required)
                    return level - 1;
            }

            return MaxLevel;
        }

        // ─────────────────────────────────────────────────────────────────────
        // CAN LEVEL UP
        // ─────────────────────────────────────────────────────────────────────

        public static bool CanLevelUp(
            int currentLevel,
            int experience,
            GrowthRateType growthRate)
        {
            if (currentLevel >= MaxLevel)
                return false;

            int required =
                GetTotalExpForLevel(
                    currentLevel + 1,
                    growthRate);

            return experience >= required;
        }

        // ─────────────────────────────────────────────────────────────────────
        // EXP REMAINING INSIDE CURRENT LEVEL
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// EXP accumulated inside current level.
        /// </summary>
        public static int GetCurrentLevelProgress(
            int experience,
            int level,
            GrowthRateType growthRate)
        {
            int currentLevelExp =
                GetTotalExpForLevel(level, growthRate);

            return Math.Max(0, experience - currentLevelExp);
        }

        // ─────────────────────────────────────────────────────────────────────
        // EXP NEEDED TO FINISH CURRENT LEVEL
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// EXP remaining before next level.
        /// </summary>
        public static int GetRemainingExpToNextLevel(
            int experience,
            int level,
            GrowthRateType growthRate)
        {
            if (level >= MaxLevel)
                return 0;

            int nextLevelExp =
                GetTotalExpForLevel(level + 1, growthRate);

            return Math.Max(0, nextLevelExp - experience);
        }

        // ─────────────────────────────────────────────────────────────────────
        // LEVEL PROGRESS PERCENT
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns progress inside current level as 0.0 - 1.0
        /// Useful for EXP bars.
        /// </summary>
        public static double GetLevelProgressPercent(
            int experience,
            int level,
            GrowthRateType growthRate)
        {
            if (level >= MaxLevel)
                return 1.0;

            int currentLevelExp =
                GetTotalExpForLevel(level, growthRate);

            int nextLevelExp =
                GetTotalExpForLevel(level + 1, growthRate);

            int expInsideLevel =
                experience - currentLevelExp;

            int totalNeeded =
                nextLevelExp - currentLevelExp;

            if (totalNeeded <= 0)
                return 1.0;

            return PokemonGame.Model.Helper.MathHelper.Clamp(
                (double)expInsideLevel / totalNeeded,
                0.0,
                1.0);
        }

        // ─────────────────────────────────────────────────────────────────────
        // EXPERIENCE GAIN
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates EXP gained from defeating a Pokémon.
        /// Simplified modern formula.
        /// </summary>
        public static int CalculateBattleExperience(
            int defeatedPokemonBaseExp,
            int defeatedPokemonLevel,
            bool isTrainerBattle,
            bool participatedInBattle,
            bool hasLuckyEgg = false)
        {
            if (!participatedInBattle)
                return 0;

            double exp =
                defeatedPokemonBaseExp *
                defeatedPokemonLevel / 7.0;

            // Trainer bonus
            if (isTrainerBattle)
                exp *= 1.5;

            // Lucky Egg bonus
            if (hasLuckyEgg)
                exp *= 1.5;

            return (int)Math.Floor(exp);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GROWTH FORMULAS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Erratic growth rate.
        /// </summary>
        private static int CalcErratic(int n)
        {
            if (n <= 50)
            {
                return (int)(
                    Math.Pow(n, 3) *
                    (100 - n) / 50);
            }

            if (n <= 68)
            {
                return (int)(
                    Math.Pow(n, 3) *
                    (150 - n) / 100);
            }

            if (n <= 98)
            {
                return (int)(
                    Math.Pow(n, 3) *
                    ((1911 - 10 * n) / 3.0) / 500);
            }

            return (int)(
                Math.Pow(n, 3) *
                (160 - n) / 100);
        }

        /// <summary>
        /// Fast growth rate.
        /// </summary>
        private static int CalcFast(int n)
        {
            return (int)(
                4 * Math.Pow(n, 3) / 5);
        }

        /// <summary>
        /// Medium Fast growth rate.
        /// </summary>
        private static int CalcMediumFast(int n)
        {
            return (int)Math.Pow(n, 3);
        }

        /// <summary>
        /// Medium Slow growth rate.
        /// </summary>
        private static int CalcMediumSlow(int n)
        {
            return (int)(
                (6.0 / 5.0) * Math.Pow(n, 3)
                - 15 * Math.Pow(n, 2)
                + 100 * n
                - 140);
        }

        /// <summary>
        /// Slow growth rate.
        /// </summary>
        private static int CalcSlow(int n)
        {
            return (int)(
                5 * Math.Pow(n, 3) / 4);
        }

        /// <summary>
        /// Fluctuating growth rate.
        /// </summary>
        private static int CalcFluctuating(int n)
        {
            double factor;

            if (n <= 15)
            {
                factor =
                    (Math.Floor((n + 1.0) / 3.0) + 24)
                    / 50.0;
            }
            else if (n <= 36)
            {
                factor =
                    (n + 14.0) / 50.0;
            }
            else
            {
                factor =
                    (Math.Floor(n / 2.0) + 32)
                    / 50.0;
            }

            return (int)(
                Math.Pow(n, 3) * factor);
        }
    }
}