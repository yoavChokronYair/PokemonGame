using PokemonGame.Services.Data;
using PokemonGame.Services.Data.Move;
using PokemonGame.Services.Data.Pokemon;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services
{
    public sealed class SQLiteDataProvider : GameDataProvider
    {
        private readonly SQLiteConnectionService db;

        public SQLiteDataProvider(string dbPath)
        {
            db = new SQLiteConnectionService(dbPath);
        }
        public override AbilityData GetAbilityData(string abilityID)
        {
            throw new NotImplementedException();
        }

        public override List<AbilityData> GetAllAbilities()
        {
            throw new NotImplementedException();
        }

        public override List<BaseStatsdata> GetAllBaseStats()
        {
            throw new NotImplementedException();
        }

        public override List<EggMovesData> GetAllEggMoves()
        {
            throw new NotImplementedException();
        }

        public override List<EvolutionData> GetAllEvolution()
        {
            throw new NotImplementedException();
        }

        public override List<FormData> GetAllFormData()
        {
            throw new NotImplementedException();
        }

        public override List<LevelUpMovesData> GetAllLevelUpMoves()
        {
            throw new NotImplementedException();
        }

        public override List<MoveData> GetAllMoves()
        {
            throw new NotImplementedException();
        }

        public override List<PokemonData> GetAllPokemon()
        {
            throw new NotImplementedException();
        }

        public override BaseStatsdata GetBaseStatsData(int pokemonID)
        {
            throw new NotImplementedException();
        }

        public override EggMovesData GetEggMovesData(int pokemonID)
        {
            throw new NotImplementedException();
        }

        public override EvolutionData GetEvolutionData(int pokemonID)
        {
            throw new NotImplementedException();
        }

        public override FormData GetFormData(int pokemonID)
        {
            throw new NotImplementedException();
        }

        public override LevelUpMovesData GetLevelUpMovesData(int pokemonID)
        {
            throw new NotImplementedException();
        }

        public override MoveData GetMoveData(string moveName)
        {
            throw new NotImplementedException();
        }

        public override PokemonData GetPokemonData(int pokemonID)
        {
            throw new NotImplementedException();
        }
    }
}
