namespace PokemonGameModel.Enums
{
    public enum TileTypeFirstLayer
    {
        Empty,
        Path,
        Grass,
        Water,
        Black,
        Trainer,
        
    }
    public enum TileTypeSecondLayer
    {
        player = -1,
        None = 0,
        Event = 1,
        Interactable = 2,

    }
}
