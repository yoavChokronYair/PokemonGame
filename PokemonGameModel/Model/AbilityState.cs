using PokemonGame.Model.Domain;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Model
{
    public class AbilityState : IAbility
    {
        private readonly AbillityDomain _abillityDomain;
        private readonly ICondition<BattleState> _condition;
        private readonly IEffect _effect;

        public AbilityState(AbillityDomain abillityDomain, ICondition<BattleState> condition, IEffect effect)
        {
            _abillityDomain = abillityDomain;
            _condition = condition;
            _effect = effect;
        }

        public void Apply(BattleState battle)
        {
            if (!_abillityDomain.used)
            {
                if (_condition.Check(battle))
                {
                    _effect.Apply(battle);
                }
                _abillityDomain.used = true;
            }
        }
    }
}
