using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.User
{
    public class BattlePlayerData : UserData
    {
        private string name;
        private int playerID;
        private int level;

        public string Name { get => name; set => name = value; }
        public int PlayerID { get => playerID; set => playerID = value; }
    }
}
