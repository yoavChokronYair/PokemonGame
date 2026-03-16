using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Repositories.SQLite
{
    internal class SQLiteUserRepository : SQLiteRepository<string, UserData>
    {
        internal SQLiteUserRepository(ISQLiteConnectionService db) : base(db) { }

        public UserData? LoadUserByName(string username) =>
            GetCached(username, () => _db.QuerySingle<UserData?>(
                "SELECT * FROM Users WHERE UserName = @UserName", new { UserName = username }));

        public bool UserExists(string username) =>
            ExistsCached(username, () => LoadUserByName(username) != null);

        public UserData CreateUser(string username, int passwordHash)
        {
            _db.Execute("INSERT INTO Users (UserName, Password) VALUES (@UserName, @Password);",
                new { UserName = username, Password = passwordHash });
            return StoreAndReturn(username, () =>
                _db.QuerySingle<UserData>("SELECT * FROM Users WHERE UserID = last_insert_rowid();"));
        }

        public List<UserData> GetAllUsers() =>
            GetAllCached(() => _db.Query<UserData>("SELECT * FROM Users").ToList(), u => u.UserName);
    }
}
