// Layer: Interface — evaluates to a double from the current battle state.
// Used for dynamic values: damage power, hit count, HP amounts, etc.

using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Interface
{
    internal interface INumber
    {
        double Evaluate(BattleState battle);
    }
}
