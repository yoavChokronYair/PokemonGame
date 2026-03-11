// Layer: Interface — contract definition only, no logic or implementations here.
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Interface.Move
{
    internal interface IMove
    {
        void Execute(BattleState battle);
    }
}
