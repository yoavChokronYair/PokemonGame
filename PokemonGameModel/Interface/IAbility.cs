using PokemonGame.Model.Domain.Battle;
namespace PokemonGame.Model.Interface
{
    public enum AbilityTrigger
    {
        None,
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
