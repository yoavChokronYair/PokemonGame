using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.GameData.User.OnlinePlayer;
using PokemonGame.Services.Data.Repositories.SQLite;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class BattleHistoryService
    {
        private readonly SQLiteBattleRepository _battles;

        public BattleHistoryService()
        {
            _battles = ServiceFactory.Instance.BattleRepository;
        }

        public List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player)
        {
            if (player == null) return new List<BattleHistoryEntryData>();
            return _battles.GetBattleHistory(player);
        }

        public List<BattleDisplayData> GetBattleHistoryDisplay(string name,string user)
        {
            var player = ServiceFactory.Instance.OnlinePlayerRepository.LoadOnlinePlayerByName(name,ServiceFactory.Instance.UserRepository.LoadUserByName(user).UserID);
            var history = _battles.GetBattleHistory(player);
            var displayList = new List<BattleDisplayData>();

            foreach (var entry in history)
            {
                var playerPokemon = _battles.GetBattleTeamPokemonForPlayer(entry.BattleID, player.BattlePlayerID)
                    .Select(p => p.SpeciesName).Take(6).ToList();

                var opponent = _battles.GetOpponentPlayer(entry.BattleID, player.BattlePlayerID);
                var opponentPokemon = opponent != null
                    ? _battles.GetBattleTeamPokemonForPlayer(entry.BattleID, opponent.BattlePlayerID)
                        .Select(p => p.SpeciesName).Take(6).ToList()
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
        public List<string> PlayerPokemon { get; set; } = new();
        public List<string> OpponentPokemon { get; set; } = new();
    }
}