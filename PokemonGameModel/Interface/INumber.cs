// Layer: Interface — evaluates to a double from the current battle state.
// Used for dynamic values: damage power, hit count, HP amounts, etc.

using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Interface
{
    public interface INumber
    {
        double Evaluate(BattleState battle);
    }
}
