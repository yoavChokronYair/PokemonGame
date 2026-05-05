using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;

namespace PokemonGame.Services.Interfaces
{
    public interface IItemService
    {
        ItemTree? GetItem(string name);
        ItemTree? GetItemById(int id);
    }
    public class ItemTree
    {
        public ItemData Item { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsConsumable { get; set; }
        public MoveEffect? Effect { get; set; }
        public MoveCondition? Condition { get; set; }
    }
}
