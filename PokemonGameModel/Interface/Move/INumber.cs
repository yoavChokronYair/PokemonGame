// Layer: Interface — evaluates to a double from the current battle state.
// Used for dynamic values: damage power, hit count, HP amounts, etc.

using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Interface.Move
{
    internal interface INumber
    {
        double Evaluate(BattleDomain battle);
    }
}
