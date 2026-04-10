using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Interface
{
    public interface IHeldItem
    {
        void Apply(BattleState battle);

    }
}
