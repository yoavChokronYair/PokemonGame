using PokemonGame.Model.Domain.Battle;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Interface
{
    public interface IMove
    {
        void Execute(BattleDomain battle);
    }
}
