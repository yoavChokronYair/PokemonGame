using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;
using System;
using System.Collections.Generic;
using System.Text;

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
