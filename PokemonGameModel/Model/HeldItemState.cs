
using PokemonGame.Model.Domain;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model
{
    public class HeldItemState : IHeldItem
    {
        private readonly HeldItemDomain _HeldItemDomain;
        public readonly ICondition<BattleState> _condition;
        public readonly IEffect _effect;
        public string Name => _HeldItemDomain.Name;
        public string Description => _HeldItemDomain.Description;
        public bool IsConsumable => _HeldItemDomain.IsConsumable;

        public HeldItemState(HeldItemDomain heldItemDomain, ICondition<BattleState> condition, IEffect effect)
        {
            _HeldItemDomain = heldItemDomain;
            _condition = condition;
            _effect = effect;
        }

        public void Apply(BattleState battle)
        {
            if (_condition.Check(battle))
            {
                _effect.Apply(battle);
            }
        }
    }
}
