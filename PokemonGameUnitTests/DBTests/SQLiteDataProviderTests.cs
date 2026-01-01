using Microsoft.Data.Sqlite;
using Moq;
using PokemonGame.Services.GameData;
using PokemonGame.Services.GameData.Pokemon;
using System.Collections.Generic;
using Xunit;
using Microsoft.Data.Sqlite;
using PokemonGame.Services.Data.DataProvider;
using PokemonGame.Services.Data.ConnectionsService;


namespace PokemonGame.Tests.DBTests
{
    public class SQLiteDataProviderTests
    {
        private readonly SQLiteDataProvider _provider;
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
