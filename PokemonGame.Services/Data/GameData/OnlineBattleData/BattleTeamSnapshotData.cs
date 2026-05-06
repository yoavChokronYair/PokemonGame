using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.GameData.OnlineBattleData
{
    internal class BattleTeamSnapshotData
    {
        public int BattleID { get; set; }
        public int BattlePlayerID { get; set; }
        public int Slot { get; set; }
        public int PokemonID { get; set; }
    }
}
