// Layer: Interface — contract definition only, no logic or implementations here.
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Interface
{
    internal interface IMove
    {
        void Execute(BattleState battle);
    }
}
