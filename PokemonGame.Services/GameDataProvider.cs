using PokemonGame.Services.Data;
using PokemonGame.Services.Data.Move;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services
{
    public abstract class GameDataProvider
    {
        public static GameDataProvider Instance { get; private set; } = null!;

        protected GameDataProvider()
        {
            Instance = this;
        }

        // --- Pokémon ---
        public abstract PokemonData GetPokemonData(int pokemonID);
        public abstract List<PokemonData> GetAllPokemon();

        public abstract PokemonFormData GetFormData(int pokemonID);
        public abstract List<PokemonFormData> GetAllFormData();

        public abstract BaseStatsdata GetBaseStatsData(int pokemonID);
        public abstract List<BaseStatsdata> GetAllBaseStats();
        
        public abstract EvolutionData GetEvolutionData(int pokemonID);
        public abstract List<EvolutionData> GetAllEvolution();
        
        public abstract EggMoveData GetEggMovesData(int pokemonID);
        public abstract List<EggMoveData> GetAllEggMoves();

        public abstract LevelUpMoveData GetLevelUpMovesData(int pokemonID);
        public abstract List<LevelUpMoveData> GetAllLevelUpMoves();
        
        // --- Moves ---
        public abstract MoveData GetMoveData(string moveName);
        public abstract List<MoveData> GetAllMoves();

        // --- Abilities ---
        public abstract AbilityData GetAbilityData(string abilityID);
        public abstract List<AbilityData> GetAllAbilities();


    }
}
