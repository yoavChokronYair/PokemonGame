using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.User.OnlinePlayer
{
    public class OnlineFriend
    {
        public int UserID { get; set; }
        public int FriendUserID { get; set; }

        public string Username { get; set; }
        public bool IsOnline { get; set; }
    }

}
