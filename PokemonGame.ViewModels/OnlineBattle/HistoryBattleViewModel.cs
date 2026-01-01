using PokemonGame.Services.Data.DataCache;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;
using System.Collections.ObjectModel;

namespace PokemonGame.ViewModels.OnlineBattle
{
    public class HistoryBattleViewModel : ViewModelBase
    {
        public ObservableCollection<BattleDisplayData> Battles { get; }

        private readonly BattleHistoryService _historyHandler;

        public HistoryBattleViewModel()
        {
            Battles = new ObservableCollection<BattleDisplayData>();

            var player = ServiceFactory.Instance.OnlinePlayerCache.GetOnlinePlayer("BattleHero",
                ServiceFactory.Instance.UserCache.GetUserByName("TestUser"));

            // Use the factory to get the public cache service
            var battleCache = ServiceFactory.Instance.BattleCache;

            // Create BattleHistoryService using the cached repository
            _historyHandler = new BattleHistoryService();

            // Load the current online player
           // var player = ServiceFactory.Instance.OnlinePlayerCache.GetOnlinePlayer(onlinePlayerName, currentUser);
            if (player != null)
                LoadBattles(player);
        }

        private void LoadBattles(BattlePlayerData player)
        {
            Battles.Clear();

            var displayBattles = _historyHandler.GetBattleHistoryDisplay(player);
            foreach (var battle in displayBattles)
                Battles.Add(battle);
        }
    }
}
