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
        public string Name => _abillityDomain.Name;

        public AbilityState(AbillityDomain abillityDomain, ICondition<BattleState> condition, IEffect effect)
        {
            _abillityDomain = abillityDomain;
            _condition = condition;
            _effect = effect;
        }

        public void Apply(BattleState battle)
        {
            if (!_abillityDomain.Used)
            {
                if (_condition.Check(battle))
                {
                    _effect.Apply(battle);
                }
                _abillityDomain.Used = true;
            }
        }
    }
}
