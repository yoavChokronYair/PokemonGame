using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Services.Data.DataProvider.SQLite
{
    internal class SQLiteUserRepository : IUserRepository
    {
        private readonly ISQLiteConnectionService _db;

        public SQLiteUserRepository(ISQLiteConnectionService dbService)
        {
            _db = dbService;
        }

        public UserData? LoadUserByName(string username) =>
            _db.QuerySingle<UserData?>(
                "SELECT * FROM Users WHERE UserName = @UserName",
                new { UserName = username });

        public bool UserExists(string username) => LoadUserByName(username) != null;

        public UserData CreateUser(string username, int passwordHash)
        {
            _db.Execute(
                "INSERT INTO Users (UserName, Password) VALUES (@UserName, @Password);",
                new { UserName = username, Password = passwordHash });

            return _db.QuerySingle<UserData>(
                "SELECT * FROM Users WHERE UserID = last_insert_rowid();");
        }

        public List<UserData> GetAllUsers() =>
            _db.Query<UserData>("SELECT * FROM Users").ToList();
    }
}
