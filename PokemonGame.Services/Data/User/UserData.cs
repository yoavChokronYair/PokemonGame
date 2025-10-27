using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.User
{
    public class UserData
    {
        private int userID;
        private string userName;
        private int password;
        public int UserID { get => userID; set => userID = value; }
        public string UserName { get => userName; set => userName = value; }
        public int Password { get => password; set => password = value; }
    }
}
