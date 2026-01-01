namespace PokemonGame.Services.Data.GameData.NpcData
{
    public class TrainerData
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Sprite { get; set; }
        public string? BattleTheme { get; set; }
        public string? MapLocation { get; set; }
        public Dialogue? Dialogue { get; set; }
        public List<TrainerPokemon>? Team { get; set; }
        public Reward? Rewards { get; set; }
        public bool RematchAvailable { get; set; }
        public int FirstDirection { get; set; }//should be enum in model
        public int SecondDirection { get; set; }//should be enum in model
    }
    public class TrainerDataList
    {
        public List<TrainerData>? trainers { get; set; }
    }
}
