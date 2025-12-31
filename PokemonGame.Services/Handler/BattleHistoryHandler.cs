using PokemonGame.Services.Data.User;
using PokemonGame.Services.Data.User.OnlinePlayer;
using PokemonGame.Services.DataProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Services.Handler
{
    public class BattleHistoryHandler
    {
        private readonly GameDataProvider provider;

        public BattleHistoryHandler(GameDataProvider dataProvider)
        {
            provider = dataProvider;
        }

        // GET RAW BATTLE HISTORY (from database)
        public List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player)
        {
            if (player == null)
                return new List<BattleHistoryEntryData>();

            return provider.GetBattleHistory(player);
        }

        // TRANSFORM INTO VIEWMODEL-FRIENDLY DATA
        public List<BattleDisplayData> GetBattleHistoryDisplay(BattlePlayerData player)
        {
            var history = provider.GetBattleHistory(player);
            var displayList = new List<BattleDisplayData>();

            foreach (var entry in history)
            {
                // Player Pokémon team (max 6)
                var playerPokemon = provider.GetBattleTeamPokemonForPlayer(entry.BattleID, player.BattlePlayerID)
                    .Select(p => p.SpeciesName)
                    .Take(6)
                    .ToList();

                // Opponent
                var opponentPlayer = provider.GetOpponentPlayer(entry.BattleID, player.BattlePlayerID);
                var opponentPokemon = opponentPlayer != null
                    ? provider.GetBattleTeamPokemonForPlayer(entry.BattleID, opponentPlayer.BattlePlayerID)
                        .Select(p => p.SpeciesName)
                        .Take(6)
                        .ToList()
                    : new List<string>();

                displayList.Add(new BattleDisplayData
                {
                    BattleID = entry.BattleID,
                    PlayerName = player.Name,
                    OpponentName = entry.OpponentName,
                    IsPlayerWinner = entry.IsWin,
                    BattleDate = entry.BattleDate,
                    PlayerPokemon = playerPokemon,
                    OpponentPokemon = opponentPokemon
                });
            }

            return displayList;
        }



    }

    // This can be shared with your ViewModel
    public class BattleDisplayData
    {
        public int BattleID { get; set; }
        public string PlayerName { get; set; } = "";
        public string OpponentName { get; set; } = "";
        public bool IsPlayerWinner { get; set; }
        public DateTime BattleDate { get; set; }

        public List<string> PlayerPokemon { get; set; } = new List<string>();
        public List<string> OpponentPokemon { get; set; } = new List<string>();
    }



}
