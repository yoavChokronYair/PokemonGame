using PokemonGame.Services.Data.User;
using PokemonGame.Services.DataProvider;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PokemonGame.Services.Handler
{
    public class GameModeChooserHandler
    {
        private readonly GameDataProvider provider;

        public GameModeChooserHandler(GameDataProvider dataProvider)
        {
            provider = dataProvider;
        }

        public bool AddOnlineModePayer(string userName, string password)
        {
            if (UserExists(userName))
                return false;
            var hashedPassword = HashPassword(password);
            Random Random = new Random();
            string VisibleuserID = Random.Next(0, 10000).ToString();
            provider.CreateUser(userName +"#"+VisibleuserID, hashedPassword);
            return true;
        }

        public bool OnlinePlayerLogIN(string username, string password,int userID)
        {
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            StoryPlayerData user = provider.LoadOnlinePlayerByName(username);
            if (user == null )
                return false;
            if(user.UserID != userID) return false;

            int hash = HashPassword(password);
            return user.Password == hash;
        }

        // CHECK EXISTS
        public bool UserExists(string username)
        {
            return provider.UserExists(username);
        }

        // GET USER
        public StoryPlayerData? GetUser(string username)
        {
            return provider.LoadOnlinePlayerByName(username);
        }
        private int HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToInt32(bytes, 0);
            }
        }

    }
}
