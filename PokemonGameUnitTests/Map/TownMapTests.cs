using PokemonGame.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace PokemonGameUnitTests.Map
{
    public class TownMapTest
    {
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        // xUnit will inject this automatically
        public TownMapTest(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
        }

    }
}

