using Microsoft.Data.Sqlite;
using Moq;
using PokemonGame.Services.Data.ConnectionsService;


namespace PokemonGame.Tests.DBTests
{
    public class SQLiteDataProviderTests
    {
        private readonly Mock<ISQLiteConnectionService> _mockDb;

        public SQLiteDataProviderTests()
        {


        }

        [Fact]
        public void GetAbilityData_ReturnsCorrectData()
        {
            var conn = new SqliteConnection("Data Source=C:\\Users\\yoav\\Source\\Repos\\PokemonGame\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db");
            conn.Open();
            Console.WriteLine("SQLite works!");
            conn.Close();
        }


    }
}
