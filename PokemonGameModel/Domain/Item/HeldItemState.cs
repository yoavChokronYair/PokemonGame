using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Item
{
    public enum ItemType
    {
        HeldItem,
        Consumable,
        KeyItem
    }
    public class itemsDomain
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public IEffect Effect { get; set; }
        public string Description { get; set; }
        public bool UsableInBattle { get; set; }
        public bool UsableInField { get; set; }
    }
    public class KeyItemState :itemsDomain
    {
        private readonly ICondition<BattleState> _condition;
        public bool registrable;

    }
    public class HeldItemState : itemsDomain, IHeldItem
    {
        private readonly ICondition<BattleState> _condition;
        public bool IsOneTimeUse { get; set; }
        public BattleEventTrigger Trigger { get; set; }

        public HeldItemState(string name, ICondition<BattleState> condition, IEffect effect,
            bool isConsumable = false, string description = "")
        {
            
            IsOneTimeUse = isConsumable;
            _condition = condition;
        }

        public void Apply(BattleState battle)
        {
            if (_condition.Check(battle))
                Effect.Apply(battle);
        }
    }
}