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

        public PokeballData? Pokeball { get; set; }
        public TmHmData? TmHm { get; set; }
        public KeyItemData? KeyItem { get; set; }
        public HeldItemData? HeldItem { get; set; }

        public MoveEffect? PokeballCaughtEffect { get; set; }
        public MoveCondition? PokeballCondition { get; set; }

        public MoveEffect? KeyItemUsageEffect { get; set; }
        public MoveCondition? KeyItemCondition { get; set; }

        public MoveEffect? HeldItemEffect { get; set; }
        public MoveCondition? HeldItemCondition { get; set; }

        public bool IsPokeball => Pokeball != null;
        public bool IsTmHm => TmHm != null;
        public bool IsKeyItem => KeyItem != null;
        public bool IsHeldItem => HeldItem != null;
    }
}