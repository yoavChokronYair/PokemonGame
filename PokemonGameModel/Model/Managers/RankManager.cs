namespace PokemonGame.Model.Model.Managers
{
    public class RankManager
    {
        // Constants based on your requirements
        private const int _pointsPerStage = 100;
        private const int _stagesPerTier = 5;
        private const int _pointsPerTier = _pointsPerStage * _stagesPerTier; // 500

        private static readonly string[] _tierNames =
        {
            "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master"
        };

        /// <summary>
        /// Calculates rank details based on total Elo.
        /// </summary>
        /// <param name="totalElo">The total rating from the DB (e.g., 1525)</param>
        public static RankResult GetRank(int totalElo)
        {
            // 1. Ensure Elo isn't negative
            int elo = Math.Max(0, totalElo);

            // 2. Determine Tier (Gold, Silver, etc.)
            int tierIndex = elo / _pointsPerTier;

            // Cap at the highest tier (Master)
            if (tierIndex >= _tierNames.Length)
                tierIndex = _tierNames.Length - 1;

            string tier = _tierNames[tierIndex];

            // 3. Determine Stage (V, IV, III, II, I)
            // Points remaining within the current 500-point tier
            int eloInTier = elo % _pointsPerTier;

            // Calculate stage index (0 to 4)
            int stageIndex = eloInTier / _pointsPerStage;

            // Invert so higher points = lower Roman Numeral (100pts = IV, 400pts = I)
            int stageValue = 5 - stageIndex;
            string romanNumeral = GetRomanNumeral(stageValue);

            // 4. Determine Slider Progress (0-100)
            int progress = eloInTier % _pointsPerStage;

            return new RankResult
            {
                RankName = $"{tier} {romanNumeral}",
                CurrentProgress = progress,
                MaxProgress = _pointsPerStage,
                Tier = tier,
                Stage = romanNumeral
            };
        }

        private static string GetRomanNumeral(int number) => number switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            _ => "V"
        };
    }

    public struct RankResult
    {
        public string RankName { get; set; }        // e.g. "Gold III"
        public int CurrentProgress { get; set; }    // e.g. 25
        public int MaxProgress { get; set; }        // Always 100
        public string Tier { get; set; }            // e.g. "Gold"
        public string Stage { get; set; }           // e.g. "III"
    }
}
