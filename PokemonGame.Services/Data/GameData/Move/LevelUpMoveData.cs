namespace PokemonGame.Services.Data.GameData.Move
{
    public class LevelUpMoveData
    {
        public int PokedexID { get; set; }
        public int MoveID { get; set; }
        public int Level { get; set; }
    }
    public class MachineMoveData
    {
        public string MachineID { get; set; } = string.Empty;
        public int MoveID { get; set; }
        public int PokedexID { get; set; }
    }

    public class EggMoveData
    {
        public int PokedexID { get; set; }
        public int MoveID { get; set; }
    }

    public class TutorMoveData
    {
        public int PokedexID { get; set; }
        public int MoveID { get; set; }
        public string? Tutor_location { get; set; }
    }
}
