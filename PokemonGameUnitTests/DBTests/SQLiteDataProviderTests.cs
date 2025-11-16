using Moq;
using PokemonGame.Services;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.DataProvider;
using System.Collections.Generic;
using Xunit;

namespace PokemonGame.Tests.DBTests
{
    public class SQLiteDataProviderTests
    {
        private readonly SQLiteDataProvider _provider;
        private readonly Mock<ISQLiteConnectionService> _mockDb;

        public SQLiteDataProviderTests()
        {
            _mockDb = new Mock<ISQLiteConnectionService>();
            _provider = new SQLiteDataProvider(_mockDb.Object);
        }

        [Fact]
        public void GetAbilityData_ReturnsCorrectData()
        {
            var expected = new AbilityData
            {
                AbilityName = "Overgrow",
                AbilityDescription = "Boosts Grass moves"
            };

            _mockDb.Setup(d => d.QuerySingle<AbilityData>(
                It.IsAny<string>(),
                It.IsAny<object>()))
                .Returns(expected);

            var result = _provider.GetAbilityData("Overgrow");

            Assert.Equal(expected.AbilityName, result.AbilityName);
            Assert.Equal(expected.AbilityDescription, result.AbilityDescription);
        }

        [Fact]
        public void GetAllAbilities_ReturnsList()
        {
            var expectedList = new List<AbilityData>
            {
                new AbilityData { AbilityName = "Overgrow" },
                new AbilityData { AbilityName = "Chlorophyll" }
            };

            _mockDb.Setup(d => d.Query<AbilityData>(It.IsAny<string>()))
                   .Returns(expectedList);

            var result = _provider.GetAllAbilities();

            Assert.Equal(2, result.Count);
            Assert.Equal("Overgrow", result[0].AbilityName);
            Assert.Equal("Chlorophyll", result[1].AbilityName);
        }

        [Fact]
        public void GetPokemonData_ReturnsCorrectPokemon()
        {
            var expected = new PokemonData
            {
                PokemonID = 1,
                SpeciesName = "Bulbasaur"
            };

            _mockDb.Setup(d => d.QuerySingle<PokemonData>(
                                It.IsAny<string>(),
                                It.IsAny<object>()))
                   .Returns(expected);

            var result = _provider.GetPokemonData(1);

            Assert.Equal(1, result.PokemonID);
            Assert.Equal("Bulbasaur", result.SpeciesName);
        }
    }
}
