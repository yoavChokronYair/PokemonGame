using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.DesignPatterns
{
    internal class WithPrecondition : IMove
    {
        private readonly ICondition<BattleState> _condition;
        private readonly IMove _move;
        private readonly string? _failMessage;

        public WithPrecondition(ICondition<BattleState> condition, IMove move, string? failMessage = null)
        {
            _condition = condition;
            _move = move;
            _failMessage = failMessage;
        }

        public void Execute(BattleState battle)
        {
            if (!_condition.Check(battle))
            {
                battle.Logger.Log(_failMessage ?? "But it failed!");
                return;
            }
            _move.Execute(battle);
        }
    }

    // Blocks execution if the user pokemon itself doesn't meet a condition.
    // e.g. can't move while paralyzed/frozen, can't use Fly if already airborne.
    internal class WithApplicability : IMove
    {
        private readonly ICondition<PokemonState> _condition;
        private readonly IMove _move;
        private readonly string? _failMessage;

        public WithApplicability(ICondition<PokemonState> condition, IMove move, string? failMessage = null)
        {
            _condition = condition;
            _move = move;
            _failMessage = failMessage;
        }

        public void Execute(BattleState battle)
        {
            if (!_condition.Check(battle.Attacker))
            {
                battle.Logger.Log(_failMessage ?? "But it failed!");
                return;
            }
            _move.Execute(battle);
        }
    }

    // Disables a move after use for N turns — e.g. Disable, Encore lock.
    internal class WithDisable : IMove
    {
        private readonly IMove _move;
        private readonly int _lockTurns;
        private int _turnsLocked = 0;

        public WithDisable(IMove move, int lockTurns)
        {
            _move = move;
            _lockTurns = lockTurns;
        }

        public bool IsLocked => _turnsLocked > 0;
        public void Tick()
        {
            if (_turnsLocked > 0)
            {
                _turnsLocked--;
            }
        }

        public void Execute(BattleState battle)
        {
            if (IsLocked)
            {
                battle.Logger.Log("The move is disabled!");
                return;
            }
            _move.Execute(battle);
            _turnsLocked = _lockTurns;
        }
    }

    // Overrides type effectiveness — e.g. Scrappy lets Normal hit Ghost.
    internal class WithTypeOverride : IMove
    {
        private readonly IMove _move;
        private readonly PokemonType _overrideType;

        public WithTypeOverride(IMove move, PokemonType overrideType)
        {
            _move = move;
            _overrideType = overrideType;
        }

        public void Execute(BattleState battle)
        {
            battle.ActiveTypeOverride = _overrideType;
            _move.Execute(battle);
            battle.ActiveTypeOverride = null;
        }
    }

    // Executes a follow-up effect automatically after the main move.
    // e.g. Relic Song transforms Meloetta, U-turn forces a switch after damage.
    internal class WithFollowUp : IMove
    {
        private readonly IMove _main;
        private readonly IEffect _followUp;

        public WithFollowUp(IMove main, IEffect followUp)
        {
            _main = main;
            _followUp = followUp;
        }

        public void Execute(BattleState battle)
        {
            _main.Execute(battle);
            _followUp.Apply(battle);
        }
    }
}
