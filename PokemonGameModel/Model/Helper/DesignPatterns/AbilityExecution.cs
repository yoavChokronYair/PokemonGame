using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Model.Helper.DesignPatterns
{
    internal class OnHit : IAbility
    {
        private readonly ICondition<BattleState> _condition;
        private readonly IAbility _ability;

        public OnHit(ICondition<BattleState> condition, IAbility ability)
        {
            _condition = condition;
            _ability = ability;
        }

        public void Apply(BattleState battle)
        {
            if (_condition.Check(battle))
            {
                _ability.Apply(battle);
                battle.Logger.Log($"ability triggered: {_ability.GetType().Name}");
            }
        }
    }
    internal class OnPassive : IAbility
    {
        private readonly IAbility _ability;

        public OnPassive(IAbility ability)
        {
            _ability = ability;
        }

        public void Apply(BattleState battle)
        {
            _ability.Apply(battle);
             battle.Logger.Log($"ability triggered: {_ability.GetType().Name}");
        }
    }
    internal class OnTurnStart : IAbility
    {
        private readonly IAbility _ability;
        public OnTurnStart(IAbility ability)
        {
            _ability = ability;
        }
        public void Apply(BattleState battle)
        {
            _ability.Apply(battle);
             battle.Logger.Log($"ability triggered: {_ability.GetType().Name}");
        }
    }
}
