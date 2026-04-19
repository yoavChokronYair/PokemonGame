using PokemonGame.Model.Domain.Battle;
namespace PokemonGame.Model.Interface
{
    public enum BattleEventTrigger
    {
        None,
        Passive,
        TurnStart,
        OnHit,
        OnSwitchIn
    }

    public interface IAbility
    {
        BattleEventTrigger Trigger { get; } // Add this
        void Apply(BattleState battle);
    }
}
