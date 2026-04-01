
using PokemonGame.Model.Domain;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Model
{
    public class HeldItemState : IHeldItem
    {
        private readonly HeldItemDomain _HeldItemDomain;
        private readonly ICondition<BattleState> _condition;
        private readonly IEffect _effect;

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
