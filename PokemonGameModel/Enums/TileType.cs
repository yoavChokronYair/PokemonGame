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
        House,
        Lab,
        Fence,
        Building,

    }
    public enum TileTypeSecondLayer
    {
        player = -1,
        None = 0,
        Event = 1,
        Interactable = 2,
        OutOfBounds = 0,
        hill = 3,

    }
}
