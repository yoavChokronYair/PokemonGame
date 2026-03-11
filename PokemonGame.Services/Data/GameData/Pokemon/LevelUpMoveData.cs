namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public sealed class LevelUpMoveData
    {
        public int MoveID { get; set; }    // FK to Move.MoveID
        public byte Level { get; set; }
        public int PokemonID { get; set; }
    }
}