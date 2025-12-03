using PokemonGame.Services.Data;
using PokemonGame.Services.Data.Move;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.Data.User;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PokemonGame.Services.DataProvider
{
    public abstract class GameDataProvider
    {
        public static GameDataProvider Instance { get; private set; } = null!;

        protected GameDataProvider()
        {
            Instance = this;
        }

        #region Caches

        private readonly Dictionary<int, PokemonData> _pokemonCache = new();
        private readonly Dictionary<int, PokemonFormData> _formCache = new();
        private readonly Dictionary<int, BaseStatsdata> _baseStatsCache = new();
        private readonly Dictionary<int, EvolutionData> _evolutionCache = new();
        private readonly Dictionary<int, EggMoveData> _eggMoveCache = new();
        private readonly Dictionary<int, LevelUpMoveData> _levelUpMoveCache = new();

        private readonly Dictionary<string, MoveData> _moveCache = new();
        private readonly Dictionary<string, AbilityData> _abilityCache = new();

        #endregion

        #region Pokémon
        public PokemonData GetPokemonData(int pokemonID, bool cache = true)
        {
            if (cache && _pokemonCache.TryGetValue(pokemonID, out var data))
                return data;

            data = LoadPokemonData(pokemonID); // abstract loader
            if (cache)
                _pokemonCache[pokemonID] = data;

            return data;
        }

        public abstract PokemonData LoadPokemonData(int pokemonID);
        public abstract List<PokemonData> GetAllPokemon();

        public PokemonFormData GetFormData(int pokemonID, bool cache = true)
        {
            if (cache && _formCache.TryGetValue(pokemonID, out var data))
                return data;

            data = LoadFormData(pokemonID);
            if (cache)
                _formCache[pokemonID] = data;

            return data;
        }

        public abstract PokemonFormData LoadFormData(int pokemonID);
        public abstract List<PokemonFormData> GetAllFormData();

        public BaseStatsdata GetBaseStatsData(int pokemonID, bool cache = true)
        {
            if (cache && _baseStatsCache.TryGetValue(pokemonID, out var data))
                return data;

            data = LoadBaseStatsData(pokemonID);
            if (cache)
                _baseStatsCache[pokemonID] = data;

            return data;
        }

        public abstract BaseStatsdata LoadBaseStatsData(int pokemonID);
        public abstract List<BaseStatsdata> GetAllBaseStats();

        public EvolutionData GetEvolutionData(int pokemonID, bool cache = true)
        {
            if (cache && _evolutionCache.TryGetValue(pokemonID, out var data))
                return data;

            data = LoadEvolutionData(pokemonID);
            if (cache)
                _evolutionCache[pokemonID] = data;

            return data;
        }

        public abstract EvolutionData LoadEvolutionData(int pokemonID);
        public abstract List<EvolutionData> GetAllEvolution();

        public EggMoveData GetEggMovesData(int pokemonID, bool cache = true)
        {
            if (cache && _eggMoveCache.TryGetValue(pokemonID, out var data))
                return data;

            data = LoadEggMovesData(pokemonID);
            if (cache)
                _eggMoveCache[pokemonID] = data;

            return data;
        }

        public abstract EggMoveData LoadEggMovesData(int pokemonID);
        public abstract List<EggMoveData> GetAllEggMoves();

        public LevelUpMoveData GetLevelUpMovesData(int pokemonID, bool cache = true)
        {
            if (cache && _levelUpMoveCache.TryGetValue(pokemonID, out var data))
                return data;

            data = LoadLevelUpMovesData(pokemonID);
            if (cache)
                _levelUpMoveCache[pokemonID] = data;

            return data;
        }

        public abstract LevelUpMoveData LoadLevelUpMovesData(int pokemonID);
        public abstract List<LevelUpMoveData> GetAllLevelUpMoves();

        #endregion

        #region Moves & Abilities

        public MoveData GetMoveData(string moveName, bool cache = true)
        {
            if (cache && _moveCache.TryGetValue(moveName, out var data))
                return data;

            data = LoadMoveData(moveName);
            if (cache)
                _moveCache[moveName] = data;

            return data;
        }

        public abstract MoveData LoadMoveData(string moveName);
        public abstract List<MoveData> GetAllMoves();

        public AbilityData GetAbilityData(string abilityName, bool cache = true)
        {
            if (cache && _abilityCache.TryGetValue(abilityName, out var data))
                return data;

            data = LoadAbilityData(abilityName);
            if (cache)
                _abilityCache[abilityName] = data;

            return data;
        }

        public abstract AbilityData LoadAbilityData(string abilityName);
        public abstract List<AbilityData> GetAllAbilities();

        #endregion
      
        #region User

        public abstract UserData? LoadUserByName(string username);
        public abstract bool UserExists(string username);
        public abstract UserData CreateUser(string username, string passwordHash);
        public abstract List<UserData> GetAllUsers();
        #endregion
    }
}
