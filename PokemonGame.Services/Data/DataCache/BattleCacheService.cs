using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.GameData.User.OnlinePlayer;
using PokemonGame.Services.Data.Interfaces;
using System.Collections.Generic;

namespace PokemonGame.Services.Data.DataCache
{
    public class BattleCacheService
    {
        private readonly IBattleRepository _repository;

        private readonly Dictionary<int, List<BattleHistoryEntryData>> _historyCache = new();
        private readonly Dictionary<(int battleID, int playerID), List<PokemonData>> _teamPokemonCache = new();

        // **internal constructor**
        internal BattleCacheService(IBattleRepository repository)
        {
            _repository = repository;
        }

        public List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player, bool useCache = true)
        {
            if (useCache && _historyCache.TryGetValue(player.BattlePlayerID, out var history))
                return history;

            history = _repository.GetBattleHistory(player);
            if (useCache)
                _historyCache[player.BattlePlayerID] = history;

            return history;
        }

        public List<PokemonData> GetBattleTeamPokemonForPlayer(int battleID, int playerID, bool useCache = true)
        {
            var key = (battleID, playerID);
            if (useCache && _teamPokemonCache.TryGetValue(key, out var team))
                return team;

            team = _repository.GetBattleTeamPokemonForPlayer(battleID, playerID);
            if (useCache)
                _teamPokemonCache[key] = team;

            return team;
        }

        public BattlePlayerData? GetOpponentPlayer(int battleID, int playerID)
        {
            return _repository.GetOpponentPlayer(battleID, playerID);
        }
    }
}
