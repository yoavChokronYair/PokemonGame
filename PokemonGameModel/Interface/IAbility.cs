using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Interface
{
    public interface IAbility
    {
        void Apply(BattleState battle);
    }
}
