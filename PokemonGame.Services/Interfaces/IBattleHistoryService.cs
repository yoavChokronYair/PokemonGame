
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.Interfaces
{
    public interface IBattleHistoryService
    {
        List<BattleTreeData> GetBattleHistoryDisplay(int battlePlayerID, string username);
        int SaveBattleRecord();
    }
}
