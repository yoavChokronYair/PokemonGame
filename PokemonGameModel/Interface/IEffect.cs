// Layer: Interface — defines the effect contract (applied to a BattleDomain).
// Every battle action (damage, status, stat change, etc.) implements IEffect.

using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Interface
{
    public interface IEffect
    {
        void Apply(BattleState battle);
    }
}
