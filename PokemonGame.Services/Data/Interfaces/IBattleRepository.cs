using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.GameData.User.OnlinePlayer;

namespace PokemonGame.Services.Data.Interfaces
{
    internal interface IBattleRepository
    {
        List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player);
        List<PokemonData> GetBattleTeamPokemonForPlayer(int battleID, int battlePlayerID);
        BattlePlayerData? GetOpponentPlayer(int battleID, int playerID);
    }

}
