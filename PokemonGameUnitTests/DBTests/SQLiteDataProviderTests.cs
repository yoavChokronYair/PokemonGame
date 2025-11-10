using Moq;
using PokemonGame.Services;
using PokemonGame.Services.Data;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.DataProvider;
using System.Collections.Generic;
using Xunit;

namespace PokemonGame.Tests.DBTests
{
    public class SQLiteDataProviderEmptyDBTests
    {
        private readonly SQLiteDataProvider _provider;
        private readonly Mock<ISQLiteConnectionService> _mockDb;

        public SQLiteDataProviderEmptyDBTests()
        {
            _mockDb = new Mock<ISQLiteConnectionService>();
            _provider = new SQLiteDataProvider(_mockDb.Object);
        }

        [Fact]
        public void GetSingle_ReturnsNull_WhenDataNotFound()
        {
            _mockDb.Setup(d => d.QuerySingle<PokemonData>(
                It.IsAny<string>(),
                It.IsAny<object>()
            )).Returns((PokemonData)null!); // simulate empty DB

            var result = _provider.Get<PokemonData, int>(1, "PokemonID");

            Assert.Null(result); // should return null if not found
        }

        [Fact]
        public void GetAll_ReturnsEmptyList_WhenTableEmpty()
        {
            _mockDb.Setup(d => d.Query<AbilityData>(
                It.IsAny<string>(),
                It.IsAny<object>()
            )).Returns(new List<AbilityData>()); // simulate empty table

            var result = _provider.GetAll<AbilityData>();

            Assert.Empty(result); // should return empty list
        }

        [Fact]
        public void GetMultiKey_ReturnsNull_WhenNoData()
        {
            _mockDb.Setup(d => d.QuerySingle<AbilityData>(
                It.IsAny<string>(),
                It.IsAny<object>()
            )).Returns((AbilityData)null!); // simulate missing row

            var resultByName = _provider.Get<AbilityData, string>("NonExistent", "AbilityName");
            var resultByDesc = _provider.Get<AbilityData, string>("NoDesc", "AbilityDescription");

            Assert.Null(resultByName);
            Assert.Null(resultByDesc);
        }

        [Fact]
        public void CacheDoesNotThrow_WhenEmpty()
        {
            // Clear cache on empty DB should not throw
            _provider.ClearCache();
        }
    }
}
