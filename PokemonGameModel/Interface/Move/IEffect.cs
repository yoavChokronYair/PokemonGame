// Layer: Interface — defines the effect contract (applied to a BattleDomain).
// Every battle action (damage, status, stat change, etc.) implements IEffect.

using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Interface.Move
{
    internal interface IEffect
    {
        void Apply(BattleDomain battle);
    }
}
