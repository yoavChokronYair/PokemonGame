using PokemonGame.Services.Data.GameData.Move;

namespace PokemonGame.Services.Data.GameData.PokemonData
{
    public class AbilityData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Effect_id { get; set; }
        public int? Condition_id { get; set; }
        public string? Trigger { get; set; }
    }
    // AbilityTreeData.cs
    public class AbilityTree
    {
        public AbilityData Ability { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Trigger { get; set; }
        public MoveEffect? Effect { get; set; }
        public MoveCondition? Condition { get; set; }
    }
}
