using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.GameData.User.OnlinePlayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Services.Data.DataProvider
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
        private readonly Dictionary<int, BaseStatsData> _baseStatsCache = new();
        private readonly Dictionary<int, EvolutionData> _evolutionCache = new();
        private readonly Dictionary<int, EggMoveData> _eggMoveCache = new();
        private readonly Dictionary<int, LevelUpMoveData> _levelUpMoveCache = new();

        private readonly Dictionary<string, MoveData> _moveCache = new();
        private readonly Dictionary<string, AbilityData> _abilityCache = new();

        #endregion

        #region Pokémon

        public PokemonData GetPokemonData(int pokemonID, bool useCache = true)
        {
            if (useCache && _pokemonCache.TryGetValue(pokemonID, out var data)) return data;
            data = LoadPokemonData(pokemonID);
            if (useCache) _pokemonCache[pokemonID] = data;
            return data;
        }

        public abstract PokemonData LoadPokemonData(int pokemonID);
        public abstract List<PokemonData> GetAllPokemon();

        public PokemonFormData GetFormData(int pokemonID, bool useCache = true)
        {
            if (useCache && _formCache.TryGetValue(pokemonID, out var data)) return data;
            data = LoadFormData(pokemonID);
            if (useCache) _formCache[pokemonID] = data;
            return data;
        }

        public abstract PokemonFormData LoadFormData(int pokemonID);
        public abstract List<PokemonFormData> GetAllFormData();

        public BaseStatsData GetBaseStatsData(int pokemonID, bool useCache = true)
        {
            if (useCache && _baseStatsCache.TryGetValue(pokemonID, out var data)) return data;
            data = LoadBaseStatsData(pokemonID);
            if (useCache) _baseStatsCache[pokemonID] = data;
            return data;
        }

        public abstract BaseStatsData LoadBaseStatsData(int pokemonID);
        public abstract List<BaseStatsData  > GetAllBaseStats();

        public EvolutionData GetEvolutionData(int pokemonID, bool useCache = true)
        {
            if (useCache && _evolutionCache.TryGetValue(pokemonID, out var data)) return data;
            data = LoadEvolutionData(pokemonID);
            if (useCache) _evolutionCache[pokemonID] = data;
            return data;
        }

        public abstract EvolutionData LoadEvolutionData(int pokemonID);
        public abstract List<EvolutionData> GetAllEvolution();

        public EggMoveData GetEggMovesData(int pokemonID, bool useCache = true)
        {
            if (useCache && _eggMoveCache.TryGetValue(pokemonID, out var data)) return data;
            data = LoadEggMovesData(pokemonID);
            if (useCache) _eggMoveCache[pokemonID] = data;
            return data;
        }

        public abstract EggMoveData LoadEggMovesData(int pokemonID);
        public abstract List<EggMoveData> GetAllEggMoves();

        public LevelUpMoveData GetLevelUpMovesData(int pokemonID, bool useCache = true)
        {
            if (useCache && _levelUpMoveCache.TryGetValue(pokemonID, out var data)) return data;
            data = LoadLevelUpMovesData(pokemonID);
            if (useCache) _levelUpMoveCache[pokemonID] = data;
            return data;
        }

        public abstract LevelUpMoveData LoadLevelUpMovesData(int pokemonID);
        public abstract List<LevelUpMoveData> GetAllLevelUpMoves();

        #endregion

        #region Moves & Abilities

        public MoveData GetMoveData(string moveName, bool useCache = true)
        {
            if (useCache && _moveCache.TryGetValue(moveName, out var data)) return data;
            data = LoadMoveData(moveName);
            if (useCache) _moveCache[moveName] = data;
            return data;
        }

        public abstract MoveData LoadMoveData(string moveName);
        public abstract List<MoveData> GetAllMoves();

        public AbilityData GetAbilityData(string abilityName, bool useCache = true)
        {
            if (useCache && _abilityCache.TryGetValue(abilityName, out var data)) return data;
            data = LoadAbilityData(abilityName);
            if (useCache) _abilityCache[abilityName] = data;
            return data;
        }

        public abstract AbilityData LoadAbilityData(string abilityName);
        public abstract List<AbilityData> GetAllAbilities();

        #endregion

        #region Users

        public abstract UserData? LoadUserByName(string username);
        public abstract bool UserExists(string username);
        public abstract UserData CreateUser(string username, int passwordHash);
        public abstract List<UserData> GetAllUsers();

        #endregion

        #region Online Players (BattlePlayer)

        public abstract BattlePlayerData CreateOnlinePlayer(string username, UserData user);
        public abstract bool OnlinePlayerExists(string username, UserData user);
        public abstract BattlePlayerData? LoadOnlinePlayerByName(string username, UserData user);
        public abstract List<BattlePlayerData> GetAllOnlinePlayers(UserData user);
        public abstract BattlePlayerData? LoadOpponentPlayer(BattlePlayerData player, int battleID);


        #endregion

        #region Battles

        public abstract List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player);
        public abstract List<PokemonData> GetBattleTeamPokemonForPlayer(int battleID, int battlePlayerID);
        public abstract BattlePlayerData? GetOpponentPlayer(int battleID, int playerID);



        #endregion
    }
}
