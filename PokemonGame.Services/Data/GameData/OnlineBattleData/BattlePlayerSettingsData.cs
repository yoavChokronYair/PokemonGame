using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.GameData.OnlineBattleData
{
    public class BattlePlayerSettingsData
    {
        public int BattlePlayerID { get; set; }
        public int AnimationsEnabled { get; set; }
        public int TextSpeedID { get; set; }
        public int BackgroundID { get; set; }
        public int ShowTypeEffectiveness { get; set; }
    }
}
