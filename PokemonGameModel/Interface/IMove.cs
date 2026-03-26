// Layer: Interface — contract definition only, no logic or implementations here.
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Interface
{
    public interface IMove
    {
        void Execute(BattleState battle);
    }

}
