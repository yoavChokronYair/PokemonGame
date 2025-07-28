using PokemonGameModel.Enums;
using System.Collections.Generic;

namespace PokemonGameModel.Interface
{
    public interface ITypeable
    {
        List<PokemonType> Types { get; } 
    }
}
