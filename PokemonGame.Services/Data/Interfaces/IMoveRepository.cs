using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;

namespace PokemonGame.Services.Data.Interfaces
{
    internal interface IMoveRepository
    {
        MoveData LoadMoveData(string moveName);
        List<MoveData> GetAllMoves();

        AbilityData LoadAbilityData(string abilityName);
        List<AbilityData> GetAllAbilities();
    }

}
