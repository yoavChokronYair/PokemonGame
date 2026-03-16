using System.Collections.ObjectModel;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class HistoryBattleViewModel : ViewModelBase
    {
        public ObservableCollection<BattleDisplayData> Battles { get; }
        private readonly BattleHistoryService _historyHandler;
        private readonly UserStore _player;

        public HistoryBattleViewModel(UserStore player)
        {
            Battles = new ObservableCollection<BattleDisplayData>();
            _historyHandler = new BattleHistoryService();
            _player = player;
            if (player != null)
            {
                LoadBattles();
            }
            else
            {
                Console.WriteLine("error");
            }
        }

        private void LoadBattles()
        {
            Battles.Clear();
            foreach (var battle in _historyHandler.GetBattleHistoryDisplay(_player.BattlePlayerID, _player.Username))
            {
                Battles.Add(battle);
            }
        }
    }
}