// Layer: Interface — defines the attempt contract used by MoveDomain and all Attempt classes.
// An IAttempt represents one or more hit attempts that a move makes against the battle state.

using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Interface
{
    internal interface IAttempt
    {
        void Execute(BattleState battle);
    }
}
