using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.GameData.OnlineBattleData
{
    public class BattlePlayerStatsData
    {
        public int BattlePlayerID { get; set; }

        // ── 1v1 (Singles) ────────────────────────────────────────
        public int CurrentElo1v1 { get; set; }
        public int PeakElo1v1 { get; set; }
        public int Wins1v1 { get; set; }
        public int CurrentStreak1v1 { get; set; }
        public int BestStreak1v1 { get; set; }

        // ── 2v2 (Doubles) ────────────────────────────────────────
        public int CurrentElo2v2 { get; set; }
        public int PeakElo2v2 { get; set; }
        public int Wins2v2 { get; set; }
        public int CurrentStreak2v2 { get; set; }
        public int BestStreak2v2 { get; set; }

        public int? FaveTeamID { get; set; }
    }
}
