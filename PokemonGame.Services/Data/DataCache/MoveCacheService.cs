using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.Interfaces;

namespace PokemonGame.Services.Data.DataCache
{
    public class MoveCacheService
    {
        private readonly IMoveRepository _provider;

        // --- Caches ---
        private readonly Dictionary<string, MoveData> _moveCache = new();
        private readonly Dictionary<string, AbilityData> _abilityCache = new();

        // --- Constructor ---
        internal MoveCacheService(IMoveRepository provider)
        {
            _provider = provider;
        }
        public MoveData GetMove(string moveName, bool useCache = true)
        {
            if (useCache && _moveCache.TryGetValue(moveName, out var data))
            {
                return data;
            }

            data = _provider.LoadMoveData(moveName);
            if (useCache)
            {
                _moveCache[moveName] = data;
            }

            return data;
        }

        public List<MoveData> GetAllMoves() => _provider.GetAllMoves();

        public AbilityData GetAbility(string abilityName, bool useCache = true)
        {
            if (useCache && _abilityCache.TryGetValue(abilityName, out var data))
            {
                return data;
            }

            data = _provider.LoadAbilityData(abilityName);
            if (useCache)
            {
                _abilityCache[abilityName] = data;
            }

            return data;
        }

        public List<AbilityData> GetAllAbilities() => _provider.GetAllAbilities();

    }
}
