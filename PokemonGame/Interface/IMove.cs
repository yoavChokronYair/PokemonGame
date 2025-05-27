namespace PokemonGame.Interface
{
    public interface IMove
    {
        string Name { get; }
        int Power { get; }
        int Accuracy { get; }
        string Type { get; }
    }
}
