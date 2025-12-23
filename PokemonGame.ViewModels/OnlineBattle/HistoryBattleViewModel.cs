using PokemonGame.Services.Data.User.OnlinePlayer;
using PokemonGame.ViewModels.ViewModelHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PokemonGame.ViewModels.OnlineBattle
{
    public class HistoryBattleViewModel : ViewModelBase
    {
        public ObservableCollection<BattleHistoryItem> Battles { get; }

        public HistoryBattleViewModel()
        {
            Battles = new ObservableCollection<BattleHistoryItem>
            {
                new BattleHistoryItem
                {
                    PlayerName = "You",
                    OpponentName = "Ash",
                    IsPlayerWinner = true
                },
                new BattleHistoryItem
                {
                    PlayerName = "You",
                    OpponentName = "Misty",
                    IsPlayerWinner = false
                },
                new BattleHistoryItem
                {
                    PlayerName = "You",
                    OpponentName = "Brock",
                    IsPlayerWinner = true
                }
            };
        }
    }
}
