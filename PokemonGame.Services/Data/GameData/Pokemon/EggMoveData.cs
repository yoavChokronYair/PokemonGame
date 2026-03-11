namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public sealed class EggMoveData
    {
        public int MoveID { get; set; }    // FK to Move.MoveID
        public int PokemonID { get; set; }
    }
}