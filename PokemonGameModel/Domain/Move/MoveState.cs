using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Move
{
    // Design: Entity + Decorator pattern.
    // MoveState: core move entity (name, type, PP, Execute).
    // WithPrecondition / WithApplicability / WithDisable / WithTypeOverride / WithFollowUp:
    //   Decorators that wrap IMove and add conditional/override/lock behavior at the move level.
    // Layer: Domain — move execution model.
    // Note: MoveCategory and MoveTarget enums live in Enums/MovesEnum/MoveStateEnums.cs.
    public class MoveState : IMove
    {
        private readonly IAttempt _attempt;

        public string Name { get; }
        public PokemonType Element { get; }
        public MoveCategory Category { get; }
        public MoveTarget Target { get; }
        public MoveTag Tag { get; }
        public int PP { get; private set; }
        public int MaxPP { get; }
        public int Priority { get; }
        public int CritStage { get; }
        public string Description { get; }
        public bool HasPP => PP > 0;

        public MoveState(
            IAttempt attempt,
            string name,
            PokemonType element,
            MoveCategory category,
            int pp = 10,
            MoveTarget target = MoveTarget.Opponent,
            MoveTag tag = default,
            int priority = 0,
            int critStage = 0,
            string description = "")
        {
            _attempt = attempt;
            Name = name;
            Element = element;
            Category = category;
            PP = pp;
            MaxPP = pp;
            Target = target;
            Tag = tag;
            Priority = priority;
            CritStage = critStage;
            Description = description;
        }

        public void Execute(BattleState battle)
        {
            if (PP <= 0)
            {
                battle.Logger.Log($"{Name} has no PP left!");
                return;
            }

            PP--;
            battle.RegisterMove(this);
            battle.Logger.Log($"{battle.Attacker.Name} used {Name}!");
            _attempt.Execute(battle);
        }

    }
}