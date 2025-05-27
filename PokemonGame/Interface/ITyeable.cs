using PokemonGame.Enums;
using System.Collections.Generic;

namespace PokemonGame.Interface
{
    public interface ITypeable
    {
        List<PokemonType> Types { get; } 
    }
}
