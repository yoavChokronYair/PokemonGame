using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.User.OnlinePlayer
{
    public class BattleData
    {
        public int BattleID { get; set; }
        public int WinnerBattlePlayerID { get; set; }
        public DateTime BattleDate { get; set; }
    }
    public class BattleTeamData
    {
        public int BattleTeamID { get; set; }
        public int BattleID { get; set; }
        public int BattlePlayerID { get; set; }
    }
    public class BattleHistoryEntryData
    {
        public int BattleID { get; set; }
        public DateTime BattleDate { get; set; }
        public string PlayerName { get; set; }
        public string OpponentName { get; set; } = "";
        public bool IsWin { get; set; }
    }

}
