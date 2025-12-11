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
        UserData userData;

        public GameModeChooserHandler(GameDataProvider dataProvider, UserData userData)
        {
            provider = dataProvider;
            this.userData = userData;
        }

        public bool AddOnlineModePayer(string userName)
        {
            if (UserExists(userName))
                return false;
            provider.CreateOnlinePlayer(userName,userData);
            return true;
        }

        public bool OnlinePlayerLogIn(string username)
        {  
            if (string.IsNullOrWhiteSpace(username))
                return false;

            BattlePlayerData user = GetOnlinePlayer(username);
            if (user == null)
                return false;
            return true;
        }

        // CHECK EXISTS
        public bool UserExists(string username)
        {
            return provider.OnlinePlayerExists(username,userData);
        }

        // GET USER
        public BattlePlayerData? GetOnlinePlayer(string username)
        {
            return provider.LoadOnlinePlayerByName(username,userData);
        }
        public List<BattlePlayerData> GetAllOnlinePlayers()
        {
            return GameDataProvider.Instance.GetAllOnlinePlayers(userData);
        }


    }
}
