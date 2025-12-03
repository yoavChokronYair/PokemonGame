using System;
using System.Security.Cryptography;
using System.Text;
using PokemonGame.Services.DataProvider;

namespace PokemonGame.Services.Handler
{
    public class SignUpHandler
    {
        private readonly GameDataProvider _db;

        public SignUpHandler(GameDataProvider db)
        {
            _db = db;
        }

        public bool UserNameExists(string userName)
        {
            if(string.IsNullOrEmpty(userName))
                return false;

            return  _db.UserExists(userName);

        }

        public bool CreateUser(string userName, string password)
        {
            if (UserNameExists(userName))
                return false;
            var hashedPassword = HashPassword(password);

             _db.CreateUser(userName,hashedPassword);
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
