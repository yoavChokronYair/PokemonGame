// Design: Entity + Decorator pattern.
// MoveDomain: core move entity (name, type, PP, Execute).
// WithPrecondition / WithApplicability / WithDisable / WithTypeOverride / WithFollowUp:
//   Decorators that wrap IMove and add conditional/override/lock behavior at the move level.
// Layer: Domain — move execution model.
// Note: MoveCategory and MoveTarget enums live in Enums/MovesEnum/MoveStateEnums.cs.

using PokemonGame.Enums.MovesEnum;
using PokemonGame.Interface;
using PokemonGame.Interface.Move;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Domain.Move
{
    internal class MoveDomain : IMove
    {
        public string Name { get; }
        public PokemonType Element { get; }
        public MoveCategory Category { get; }
        public MoveTarget Target { get; }
        public int PP { get; private set; }
        public int MaxPP { get; }
        public IAttempt Attempt { get; }

        public MoveDomain(
            string name,
            PokemonType element,
            MoveCategory category,
            IAttempt attempt,
            int pp = 10,
            MoveTarget target = MoveTarget.Opponent)
        {
            Name = name;
            Element = element;
            Category = category;
            Attempt = attempt;
            PP = pp;
            MaxPP = pp;
            Target = target;
        }

        public void Execute(BattleDomain battle)
        {
            if (PP <= 0)
            {
                battle.Log($"{Name} has no PP left!");
                return;
            }

            PP--;
            battle.RegisterMove(this);
            battle.Log($"{battle.ActiveUser.Name} used {Name}!");
            Attempt.Execute(battle);
        }

        public void RestorePP(int amount) => PP = Math.Min(PP + amount, MaxPP);
        public bool HasPP => PP > 0;
    }

    // Blocks execution if a battle-level condition is not met.
    // e.g. can't use a fire move in heavy rain, can't sleep an already-asleep target.
    internal class WithPrecondition : IMove
    {
        private readonly ICondition<BattleDomain> condition;
        private readonly IMove move;
        private readonly string? failMessage;

        public WithPrecondition(ICondition<BattleDomain> condition, IMove move, string? failMessage = null)
        {
            this.condition = condition;
            this.move = move;
            this.failMessage = failMessage;
        }

        public void Execute(BattleDomain battle)
        {
            if (!condition.Check(battle))
            {
                battle.Log(failMessage ?? "But it failed!");
                return;
            }
            move.Execute(battle);
        }
    }

    // Blocks execution if the user pokemon itself doesn't meet a condition.
    // e.g. can't move while paralyzed/frozen, can't use Fly if already airborne.
    internal class WithApplicability : IMove
    {
        private readonly ICondition<PokemonDomain> condition;
        private readonly IMove move;
        private readonly string? failMessage;

        public WithApplicability(ICondition<PokemonDomain> condition, IMove move, string? failMessage = null)
        {
            this.condition = condition;
            this.move = move;
            this.failMessage = failMessage;
        }

        public void Execute(BattleDomain battle)
        {
            if (!condition.Check(battle.ActiveUser))
            {
                battle.Log(failMessage ?? "But it failed!");
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
        public void Tick() { if (turnsLocked > 0) turnsLocked--; }

        public void Execute(BattleDomain battle)
        {
            if (IsLocked)
            {
                battle.Log("The move is disabled!");
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

        public void Execute(BattleDomain battle)
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

        public void Execute(BattleDomain battle)
        {
            main.Execute(battle);
            followUp.Apply(battle);
        }
    }
}
