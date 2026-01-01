using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.GameData.User
{
    public class StoryPlayerData : UserData
    {
        private int name;
        private int playerID;
        public int Name { get => name; set => name = value; }
        public int PlayerID { get => playerID; set => playerID = value; }
    }
}
