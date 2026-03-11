using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Model.Model.Helper.Decorator
{
    internal class WithPrecondition : IMove
    {
        private readonly ICondition<BattleState> condition;
        private readonly IMove move;
        private readonly string? failMessage;

        public WithPrecondition(ICondition<BattleState> condition, IMove move, string? failMessage = null)
        {
            this.condition = condition;
            this.move = move;
            this.failMessage = failMessage;
        }

        public void Execute(BattleState battle)
        {
            if (!condition.Check(battle))
            {
                battle.Logger.Log(failMessage ?? "But it failed!");
                return;
            }
            move.Execute(battle);
        }
    }

    // Blocks execution if the user pokemon itself doesn't meet a condition.
    // e.g. can't move while paralyzed/frozen, can't use Fly if already airborne.
    internal class WithApplicability : IMove
    {
        private readonly ICondition<PokemonState> condition;
        private readonly IMove move;
        private readonly string? failMessage;

        public WithApplicability(ICondition<PokemonState> condition, IMove move, string? failMessage = null)
        {
            this.condition = condition;
            this.move = move;
            this.failMessage = failMessage;
        }

        public void Execute(BattleState battle)
        {
            if (!condition.Check(battle.Attacker))
            {
                battle.Logger.Log(failMessage ?? "But it failed!");
                return;
            }
            move.Execute(battle);
        }
    }

    // Disables a move after use for N turns — e.g. Disable, Encore lock.
    internal class WithDisable : IMove
    {
        private readonly IMove move;
        private readonly int lockTurns;
        private int turnsLocked = 0;

        public WithDisable(IMove move, int lockTurns)
        {
            this.move = move;
            this.lockTurns = lockTurns;
        }

        public bool IsLocked => turnsLocked > 0;
        public void Tick()
        {
            if (turnsLocked > 0)
            {
                turnsLocked--;
            }
        }

        public void Execute(BattleState battle)
        {
            if (IsLocked)
            {
                battle.Logger.Log("The move is disabled!");
                return;
            }
            move.Execute(battle);
            turnsLocked = lockTurns;
        }
    }

    // Overrides type effectiveness — e.g. Scrappy lets Normal hit Ghost.
    internal class WithTypeOverride : IMove
    {
        private readonly IMove move;
        private readonly PokemonType overrideType;

        public WithTypeOverride(IMove move, PokemonType overrideType)
        {
            this.move = move;
            this.overrideType = overrideType;
        }

        public void Execute(BattleState battle)
        {
            battle.ActiveTypeOverride = overrideType;
            move.Execute(battle);
            battle.ActiveTypeOverride = null;
        }
    }

    // Executes a follow-up effect automatically after the main move.
    // e.g. Relic Song transforms Meloetta, U-turn forces a switch after damage.
    internal class WithFollowUp : IMove
    {
        private readonly IMove main;
        private readonly IEffect followUp;

        public WithFollowUp(IMove main, IEffect followUp)
        {
            this.main = main;
            this.followUp = followUp;
        }

        public void Execute(BattleState battle)
        {
            main.Execute(battle);
            followUp.Apply(battle);
        }
    }
}
