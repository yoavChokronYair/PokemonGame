using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.User.OnlinePlayer
{
    public class BattleHistoryItem
    {
        public int BattleID { get; set; }

        public int PlayerUserID { get; set; }
        public string PlayerName { get; set; }

        public int OpponentUserID { get; set; }
        public string OpponentName { get; set; }

        public bool IsPlayerWinner { get; set; }

        // Derived / loaded later
        public List<int> PlayerPokemonIDs { get; set; }
        public List<int> OpponentPokemonIDs { get; set; }
    }

}
