using PokemonGame.Services.Data.User;
using PokemonGame.Services.Data.User.OnlinePlayer;
using PokemonGame.Services.DataProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Handler
{
    public class BattleHistoryHandler
    {
        private readonly GameDataProvider provider;

        public BattleHistoryHandler(GameDataProvider dataProvider)
        {
            provider = dataProvider;
        }

        // GET BATTLE HISTORY FOR PLAYER
        public List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player)
        {
            if (player == null)
                return new List<BattleHistoryEntryData>();

            return provider.GetBattleHistory(player);
        }
    }
}
