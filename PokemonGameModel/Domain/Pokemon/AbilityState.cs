using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Pokemon
{
    public class AbilityState : IAbility
    {
        private readonly IEffect _effect;

        public string Name { get; }
        public string Description { get; }
        public BattleEventTrigger Trigger => BattleEventTrigger.None;

        public AbilityState(string name, IEffect effect, string description = "")
        {
            Name = name;
            Description = description;
            _effect = effect;
        }

        public void Apply(BattleState battle)
        {
            _effect.Apply(battle);
        }
    }
}