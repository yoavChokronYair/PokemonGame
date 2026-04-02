// Design: Entity + Decorator pattern.
// MoveDomain: core move entity (name, type, PP, Execute).
// WithPrecondition / WithApplicability / WithDisable / WithTypeOverride / WithFollowUp:
//   Decorators that wrap IMove and add conditional/override/lock behavior at the move level.
// Layer: Domain — move execution model.
// Note: MoveCategory and MoveTarget enums live in Enums/MovesEnum/MoveStateEnums.cs.

using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Move
{
    public class MoveDomain
    {
        public string Name { get; set; } = string.Empty;
        public PokemonType Element { get; set; }
        public MoveCategory Category { get; set; }
        public MoveTarget Target { get; set; }
        public MoveTag tag { get; set; }
        public int PP { get; set; }
        public int MaxPP { get; set; }
        public int Priority { get; set; }
        public int CritStage { get; set; }
        public string Description { get; set; } = string.Empty;

        public MoveDomain(
            string name,
            PokemonType element,
            MoveCategory category,
            int pp = 10,
            MoveTarget target = MoveTarget.Opponent,
            int priority = 0,
            int critStage = 0,
            string description = "")
        {
            Name = name;
            Element = element;
            Category = category;
            PP = pp;
            MaxPP = pp;
            Target = target;
            Priority = priority;
            CritStage = critStage;
            Description = description;
        }
    }
}