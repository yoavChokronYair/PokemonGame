using PokemonGame.Services.Data.GameData.User.OnlinePlayer;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Interfaces;
using System.Collections.Generic;

namespace PokemonGame.Services.Data.DataCache
{
    public class OnlinePlayerCacheService
    {
        private readonly IOnlinePlayerRepository _repository;

        // --- Caches ---
        // Keyed by (username + userID) to avoid clashes between different accounts
        private readonly Dictionary<string, BattlePlayerData> _onlinePlayerCache = new();

        internal OnlinePlayerCacheService(IOnlinePlayerRepository repository)
        {
            _repository = repository;
        }

        private string GetCacheKey(string username, int userID) => $"{username}_{userID}";

        public BattlePlayerData? GetOnlinePlayer(string username, UserData user, bool useCache = true)
        {
            var key = GetCacheKey(username, user.UserID);
            if (useCache && _onlinePlayerCache.TryGetValue(key, out var player))
                return player;

            player = _repository.LoadOnlinePlayerByName(username, user);
            if (player != null && useCache)
                _onlinePlayerCache[key] = player;

            return player;
        }

        public bool OnlinePlayerExists(string username, UserData user, bool useCache = true)
        {
            var key = GetCacheKey(username, user.UserID);
            if (useCache && _onlinePlayerCache.ContainsKey(key))
                return true;

            return _repository.OnlinePlayerExists(username, user);
        }

        public BattlePlayerData CreateOnlinePlayer(string username, UserData user, bool useCache = true)
        {
            var player = _repository.CreateOnlinePlayer(username, user);

            if (useCache)
            {
                var key = GetCacheKey(username, user.UserID);
                _onlinePlayerCache[key] = player;
            }

            return player;
        }

        public List<BattlePlayerData> GetAllOnlinePlayers(UserData user, bool useCache = true)
        {
            if (useCache)
            {
                // Return cached players for this user
                var cachedPlayers = new List<BattlePlayerData>();
                foreach (var kvp in _onlinePlayerCache)
                {
                    if (kvp.Key.EndsWith($"_{user.UserID}"))
                        cachedPlayers.Add(kvp.Value);
                }

                if (cachedPlayers.Count > 0)
                    return cachedPlayers;
            }

            var allPlayers = _repository.GetAllOnlinePlayers(user);

            if (useCache)
            {
                foreach (var p in allPlayers)
                {
                    var key = GetCacheKey(p.Name, user.UserID);
                    _onlinePlayerCache[key] = p;
                }
            }

            return allPlayers;
        }

        public BattlePlayerData? GetOpponentPlayer(BattlePlayerData player, int battleID)
        {
            // usually opponents change often; optional: do not cache
            return _repository.LoadOpponentPlayer(player, battleID);
        }
    }
}
