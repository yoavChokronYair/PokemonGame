using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model
{
    public class HeldItemState : IHeldItem
    {
        private readonly ICondition<BattleState> _condition;
        private readonly IEffect _effect;

        public string Name { get; }
        public string Description { get; }
        public bool IsConsumable { get; }

        public HeldItemState(string name, ICondition<BattleState> condition, IEffect effect,
            bool isConsumable = false, string description = "")
        {
            Name = name;
            Description = description;
            IsConsumable = isConsumable;
            _condition = condition;
            _effect = effect;
        }

        public void Apply(BattleState battle)
        {
            if (_condition.Check(battle))
                _effect.Apply(battle);
        }
    }
}