using PokemonGame.Services.Data.DataCache;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.GameData.User.OnlinePlayer;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class BattleHistoryService
    {
        private readonly BattleCacheService _battleCache;

        public BattleHistoryService()
        {
            // Use the singleton factory to get the public BattleCacheService
            _battleCache = ServiceFactory.Instance.BattleCache;
        }

        // GET RAW BATTLE HISTORY (cached)
        public List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player)
        {
            if (player == null)
            {
                return new List<BattleHistoryEntryData>();
            }

            return _battleCache.GetBattleHistory(player);
        }

        // TRANSFORM INTO VIEWMODEL-FRIENDLY DATA
        public List<BattleDisplayData> GetBattleHistoryDisplay(BattlePlayerData player)
        {
            var history = _battleCache.GetBattleHistory(player);
            var displayList = new List<BattleDisplayData>();

            foreach (var entry in history)
            {
                // Player Pokémon team (max 6)
                var playerPokemon = _battleCache.GetBattleTeamPokemonForPlayer(entry.BattleID, player.BattlePlayerID)
                    .Select(p => p.SpeciesName)
                    .Take(6)
                    .ToList();

                // Opponent
                var opponentPlayer = _battleCache.GetOpponentPlayer(entry.BattleID, player.BattlePlayerID);
                var opponentPokemon = opponentPlayer != null
                    ? _battleCache.GetBattleTeamPokemonForPlayer(entry.BattleID, opponentPlayer.BattlePlayerID)
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
