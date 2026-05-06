using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.GameData.PokemonData;

namespace PokemonGame.Services.Interfaces
{
    public interface IAbilityService
    {
        AbilityTree? GetAbility(string name);
        AbilityTree? GetAbilityById(int id);
    }
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
