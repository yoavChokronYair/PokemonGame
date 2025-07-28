using PokemonGameModel.Enums;
namespace PokemonGameModel.Model.Data.NpcData
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
        public Direction? FirstDirection { get; set; }
        public Direction? SecondDirection { get; set; }
    }
    public class TrainerDataList
    {
        public List<TrainerData>? trainers { get; set; }
    }
}
