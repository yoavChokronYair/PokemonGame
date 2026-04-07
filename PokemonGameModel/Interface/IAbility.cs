using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Interface
{
    public enum AbilityTrigger
    {
        Passive,
        TurnStart,
        OnHit,
        OnSwitchIn
    }

    public interface IAbility
    {
        AbilityTrigger Trigger { get; } // Add this
        void Apply(BattleState battle);
    }
}
