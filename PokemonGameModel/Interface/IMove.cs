using PokemonGameModel.Enums;

namespace PokemonGameModel.Interface
{
    public interface IMove
    {
        string Name { get; }
        int Power { get; }
        int Accuracy { get; }
        PokemonType Type { get; }
    }
}
