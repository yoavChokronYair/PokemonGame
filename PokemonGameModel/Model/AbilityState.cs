using PokemonGame.Model.Domain;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model
{
    public class AbilityState : IAbility
    {
        private readonly AbillityDomain _abillityDomain;
        
        private readonly IEffect _effect;
        public string Name => _abillityDomain.Name;

        public AbilityTrigger Trigger => throw new NotImplementedException();

        public AbilityState(AbillityDomain abillityDomain, IEffect effect)
        {
            _abillityDomain = abillityDomain;
            _effect = effect;
        }

        public void Apply(BattleState battle)
        {
            if (!_abillityDomain.Used)
            {
                _effect.Apply(battle);  
                _abillityDomain.Used = true;
            }
        }
    }
}
