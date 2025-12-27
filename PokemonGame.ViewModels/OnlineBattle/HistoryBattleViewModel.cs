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
        public ObservableCollection<BattleData> Battles { get; }

        public HistoryBattleViewModel()
        {
            Battles = new ObservableCollection<BattleData>
            {
                new BattleData
                {
                    PlayerName = "You",
                    OpponentName = "Ash",
                    IsPlayerWinner = true
                },
                new BattleData
                {
                    PlayerName = "You",
                    OpponentName = "Misty",
                    IsPlayerWinner = false
                },
                new BattleData
                {
                    PlayerName = "You",
                    OpponentName = "Brock",
                    IsPlayerWinner = true
                }
            };
        }
    }
}
