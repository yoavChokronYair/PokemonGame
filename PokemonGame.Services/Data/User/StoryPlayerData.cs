using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.User
{
    public class StoryPlayerData : UserData
    {
        private int name;
        private int pID;
        public int Name { get => name; set => name = value; }
        public int PID { get => pID; set => pID = value; }
    }
}
