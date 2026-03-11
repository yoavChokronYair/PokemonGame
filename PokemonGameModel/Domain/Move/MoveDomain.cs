// Design: Entity + Decorator pattern.
// MoveDomain: core move entity (name, type, PP, Execute).
// WithPrecondition / WithApplicability / WithDisable / WithTypeOverride / WithFollowUp:
//   Decorators that wrap IMove and add conditional/override/lock behavior at the move level.
// Layer: Domain — move execution model.
// Note: MoveCategory and MoveTarget enums live in Enums/MovesEnum/MoveStateEnums.cs.

using PokemonGame.Enums.MovesEnum;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Model.Domain.Move
{
    internal class MoveDomain
    {
        public string Name { get; set; } = string.Empty;
        public PokemonType Element { get; set; }
        public MoveCategory Category { get; set; }
        public MoveTarget Target { get; set; }
        public int PP { get; set; }
        public int MaxPP { get; set; }

        public MoveDomain(string name, PokemonType element, MoveCategory category,
                         int pp = 10, MoveTarget target = MoveTarget.Opponent)
        {
            Name = name;
            Element = element;
            Category = category;
            PP = pp;
            MaxPP = pp;
            Target = target;
        }
    }
}
