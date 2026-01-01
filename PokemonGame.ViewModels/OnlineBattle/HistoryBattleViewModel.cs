using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;
using System.Collections.ObjectModel;
using PokemonGame.Services.Data.DataProvider;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.ViewModels.OnlineBattle
{
    public class HistoryBattleViewModel : ViewModelBase
    {
        public ObservableCollection<BattleDisplayData> Battles { get; }

        private readonly BattleHistoryService historyHandler;

        public HistoryBattleViewModel()
        {
            Battles = new ObservableCollection<BattleDisplayData>();
            historyHandler = new BattleHistoryService(GameDataProvider.Instance);

            // Just pass the BattlePlayerData
            var player = GameDataProvider.Instance.LoadOnlinePlayerByName("BattleHero",
                GameDataProvider.Instance.LoadUserByName("TestUser"));
            if (player != null)
                LoadBattles(player);
        }

        private void LoadBattles(BattlePlayerData player)
        {
            Battles.Clear();

            var displayBattles = historyHandler.GetBattleHistoryDisplay(player);
            foreach (var battle in displayBattles)
                Battles.Add(battle);
        }


    }
}
