using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Interface
{
    public interface IMove
    {
        string Name { get; }
        int Power { get; }
        int Accuracy { get; }
        PokemonType Type { get; }
    }
}
