// Layer: Interface — defines the attempt contract used by MoveDomain and all Attempt classes.
// An IAttempt represents one or more hit attempts that a move makes against the battle state.

using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Interface.Move
{
    internal interface IAttempt
    {
        void Execute(BattleDomain battle);
    }
}
