using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.DesignPatterns
{
    internal class OnHit : IAbility
    {
        private readonly IAbility _ability;
        private readonly ICondition<BattleState> _condition;

        public OnHit(ICondition<BattleState> condition, IAbility ability)
        {
            _condition = condition;
            _ability = ability;
        }

        public AbilityTrigger Trigger => throw new NotImplementedException();

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

        public AbilityTrigger Trigger => throw new NotImplementedException();

        public void Apply(BattleState battle)
        {
            _ability.Apply(battle);
             battle.Logger.Log($"ability triggered: {_ability.GetType().Name}");
        }
    }
    internal class OnTurnStart : IAbility
    {
        private readonly IAbility _ability;
        private readonly ICondition<BattleState> _condition;
        public AbilityTrigger Trigger { get; }
        public OnTurnStart(IAbility ability,AbilityTrigger trigger, ICondition<BattleState> condition)
        {
            _ability = ability;
            Trigger = trigger;
            _condition = condition;
        }
        public void Apply(BattleState battle)
        {
            if(Trigger == AbilityTrigger.TurnStart && _condition.Check(battle))
            {
                _ability.Apply(battle);
                battle.Logger.LogTurnStart($"ability triggered: {_ability.GetType().Name}");
            }
        }
    }
    internal class OnSwitchIn : IAbility
    {
        private readonly IAbility _ability;
        public OnSwitchIn(IAbility ability) => _ability = ability;

        public AbilityTrigger Trigger => throw new NotImplementedException();

        public void Apply(BattleState battle)
        {
            _ability.Apply(battle);
            battle.Logger.Log($"{_ability.GetType().Name} activated on switch-in!");
        }
    }
}
