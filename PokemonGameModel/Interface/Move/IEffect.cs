// Layer: Interface — defines the effect contract (applied to a BattleDomain).
// Every battle action (damage, status, stat change, etc.) implements IEffect.

using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Interface.Move
{
    internal interface IEffect
    {
        void Apply(BattleState battle);
    }
}
