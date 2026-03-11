using PokemonGame.Model.Domain.Battle;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Interface
{
    internal interface IMove
    {
        void Execute(BattleDomain battle);
    }
}
