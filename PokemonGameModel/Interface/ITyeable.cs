using PokemonGame.Enums;
using PokemonGame.Services.Enums.PokemonEnum;
using System.Collections.Generic;

namespace PokemonGame.Interface
{
    public interface ITypeable
    {
        List<PokemonType> Types { get; } 
    }
}
