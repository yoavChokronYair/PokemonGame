using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Model.Data.NpcData
{
    public class RivalDataList
    {
        public List<RivalData> Rival { get; set; }
    }

    public class RivalData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Sprite { get; set; }
        public string BattleTheme { get; set; }
        public string MapLocation { get; set; }
        public Dialogue Dialogue { get; set; }
        public List<CaughtPokemonData> Team { get; set; }
        public Reward Rewards { get; set; }
        public bool RematchAvailable { get; set; }
    }

    public class Dialogue
    {
        public string PreBattle { get; set; }
        public string PostBattleWin { get; set; }
        public string PostBattleLoss { get; set; }
    }
    public class Reward
    {
        public int Money { get; set; }
        public List<string> Items { get; set; }
        public string UnlockEvent { get; set; }
    }

}
