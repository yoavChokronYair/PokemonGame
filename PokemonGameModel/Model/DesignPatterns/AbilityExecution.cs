using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.DesignPatterns
{
    // Decorators
    public class OnHit : IAbility
    {
        private readonly IAbility _ability;
        private readonly ICondition<BattleState> _condition;
        public BattleEventTrigger Trigger => BattleEventTrigger.OnHit;

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
                battle.Logger.Log($"Ability triggered: {_ability.GetType().Name}");
            }
        }
    }

    public class OnPassive : IAbility
    {
        private readonly IAbility _ability;
        public BattleEventTrigger Trigger => BattleEventTrigger.Passive;

        public OnPassive(IAbility ability) => _ability = ability;

        public void Apply(BattleState battle)
        {
            _ability.Apply(battle);
            battle.Logger.Log($"Ability triggered: {_ability.GetType().Name}");
        }
    }

    public class OnTurnStart : IAbility
    {
        private readonly IAbility _ability;
        private readonly ICondition<BattleState> _condition;
        public BattleEventTrigger Trigger => BattleEventTrigger.TurnStart;

        public OnTurnStart(IAbility ability, ICondition<BattleState> condition)
        {
            _ability = ability;
            _condition = condition;
        }

        public void Apply(BattleState battle)
        {
            if (_condition.Check(battle))
            {
                _ability.Apply(battle);
                battle.Logger.LogTurnStart($"Ability triggered: {_ability.GetType().Name}");
            }
        }
    }

    public class OnSwitchIn : IAbility
    {
        private readonly IAbility _ability;
        public BattleEventTrigger Trigger => BattleEventTrigger.OnSwitchIn;

        public OnSwitchIn(IAbility ability) => _ability = ability;

        public void Apply(BattleState battle)
        {
            _ability.Apply(battle);
            battle.Logger.Log($"{_ability.GetType().Name} activated on switch-in!");
        }
    }

    // Replaces the _used flag that was baked into AbilityState
    public class OncePerSwitch : IAbility
    {
        private readonly IAbility _ability;
        private bool _used;
        public BattleEventTrigger Trigger => _ability.Trigger;

        public OncePerSwitch(IAbility ability) => _ability = ability;

        public void Apply(BattleState battle)
        {
            if (_used) return;
            _used = true;
            _ability.Apply(battle);
        }

        public void Reset() => _used = false; // call when the pokemon switches out
    }
}
