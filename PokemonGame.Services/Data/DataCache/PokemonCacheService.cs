using PokemonGame.Services.Data.DataProvider;
using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace PokemonGame.Services.Data.DataCache
{
    public class PokemonCacheService
    {
        private readonly IPokemonRepository _provider;

        // --- Caches ---
        private readonly Dictionary<int, PokemonData> _pokemonCache = new();
        private readonly Dictionary<int, PokemonFormData> _formCache = new();
        private readonly Dictionary<int, BaseStatsData> _baseStatsCache = new();
        private readonly Dictionary<int, EvolutionData> _evolutionCache = new();
        private readonly Dictionary<int, EggMoveData> _eggMoveCache = new();
        private readonly Dictionary<int, LevelUpMoveData> _levelUpMoveCache = new();

        // --- Constructor ---
        internal PokemonCacheService(IPokemonRepository provider)
        {
            _provider = provider;
        }


        public PokemonData GetPokemonData(int id, bool useCache = true)
        {
            if (useCache && _pokemonCache.TryGetValue(id, out var data)) return data;
            data = _provider.LoadPokemonData(id);
            if (useCache) _pokemonCache[id] = data;
            return data;
        }

        public List<PokemonData> GetAllPokemon() => _provider.GetAllPokemon();

        public PokemonFormData GetFormData(int id, bool useCache = true)
        {
            if (useCache && _formCache.TryGetValue(id, out var data)) return data;
            data = _provider.LoadFormData(id);
            if (useCache) _formCache[id] = data;
            return data;
        }

        public List<PokemonFormData> GetAllFormData() => _provider.GetAllFormData();

        public BaseStatsData GetBaseStats(int id, bool useCache = true)
        {
            if (useCache && _baseStatsCache.TryGetValue(id, out var data)) return data;
            data = _provider.LoadBaseStatsData(id);
            if (useCache) _baseStatsCache[id] = data;
            return data;
        }

        public List<BaseStatsData> GetAllBaseStats() => _provider.GetAllBaseStats();

        public EvolutionData GetEvolutionData(int id, bool useCache = true)
        {
            if (useCache && _evolutionCache.TryGetValue(id, out var data)) return data;
            data = _provider.LoadEvolutionData(id);
            if (useCache) _evolutionCache[id] = data;
            return data;
        }

        public List<EvolutionData> GetAllEvolution() => _provider.GetAllEvolution();

        public EggMoveData GetEggMoves(int id, bool useCache = true)
        {
            if (useCache && _eggMoveCache.TryGetValue(id, out var data)) return data;
            data = _provider.LoadEggMovesData(id);
            if (useCache) _eggMoveCache[id] = data;
            return data;
        }

        public List<EggMoveData> GetAllEggMoves() => _provider.GetAllEggMoves();

        public LevelUpMoveData GetLevelUpMoves(int id, bool useCache = true)
        {
            if (useCache && _levelUpMoveCache.TryGetValue(id, out var data)) return data;
            data = _provider.LoadLevelUpMovesData(id);
            if (useCache) _levelUpMoveCache[id] = data;
            return data;
        }

        public List<LevelUpMoveData> GetAllLevelUpMoves() => _provider.GetAllLevelUpMoves();

    }
}
