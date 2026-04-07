// Layer: Interface — contract definition only, no logic or implementations here.

using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Interface
{
    public interface IMove
    {
        void Execute(BattleState battle);
    }

}
