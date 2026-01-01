using System;
using System.Security.Cryptography;
using System.Text;
using PokemonGame.Services.Data.DataProvider;

namespace PokemonGame.Services.Handler
{
    public class SignUpService
    {
        private readonly GameDataProvider provider;

        public SignUpService(GameDataProvider db)
        {
            provider = db;
        }

        public bool UserNameExists(string userName)
        {
            if(string.IsNullOrEmpty(userName))
                return false;

            return  provider.UserExists(userName);

        }

        public bool CreateUser(string userName, string password)
        {
            if (UserNameExists(userName))
                return false;
            var hashedPassword = HashPassword(password);

             provider.CreateUser(userName,hashedPassword);
            return true;
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
