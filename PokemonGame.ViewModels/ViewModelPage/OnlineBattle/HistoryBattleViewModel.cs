using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelUserControl;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class BattleHistoryEntry : ViewModelBase
    {
        public int BattleID { get; set; }
        public string BattleDate { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string OpponentName { get; set; } = string.Empty;
        public bool IsPlayerWinner { get; set; }
        public PokemonTeamViewModel PlayerTeam { get; } = new();
        public PokemonTeamViewModel OpponentTeam { get; } = new();
    }

    public class HistoryBattleViewModel : ViewModelBase
    {
        private readonly BattleHistoryService _historyService;
        private readonly UserStore _userStore;

        public ObservableCollection<BattleHistoryEntry> Battles { get; } = new();
        public bool HasNoBattles => Battles.Count == 0;

        public HistoryBattleViewModel(UserStore player)
        {
            _userStore = player;
            _historyService = new BattleHistoryService();

            LoadRealBattles();
        }

        private void LoadRealBattles()
        {
            Battles.Clear();

            // 1. Get the structured data from the service
            var historyData = _historyService.GetBattleHistoryDisplay(_userStore.BattlePlayerID, _userStore.Username);

            foreach (var record in historyData)
            {
                var entry = new BattleHistoryEntry
                {
                    BattleID = record.BattleID,
                    BattleDate = record.BattleDate,
                    PlayerName = record.PlayerName,
                    OpponentName = record.OpponentName,
                    IsPlayerWinner = record.IsPlayerWinner
                };

                // 2. Map Player Pokemon objects to TeamSlotDisplayEntries
                var playerSlots = record.PlayerPokemon.Select(p => new TeamSlotDisplayEntry
                {
                    PokedexId = p.PokedexId,
                    HeldItemName = p.ItemName,
                    IsEmpty = false
                });

                // 3. Map Opponent Pokemon objects to TeamSlotDisplayEntries
                var opponentSlots = record.OpponentPokemon.Select(p => new TeamSlotDisplayEntry
                {
                    PokedexId = p.PokedexId,
                    HeldItemName = p.ItemName,
                    IsEmpty = false
                });

                // 4. Load them into the Team ViewModels (which handles the padding to 6 slots)
                entry.PlayerTeam.LoadSlots(playerSlots);
                entry.OpponentTeam.LoadSlots(opponentSlots);

                Battles.Add(entry);
            }

            OnPropertyChanged(nameof(HasNoBattles));
        }
    }
}