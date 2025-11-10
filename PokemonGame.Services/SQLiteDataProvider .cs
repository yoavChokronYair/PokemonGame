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
            return db.QuerySingle<AbilityData>("SELECT * FROM Move WHERE AbilityID = @abilityID", new { abilityID = abilityID });
        }

        public override List<AbilityData> GetAllAbilities()
        {
            return db.Query<AbilityData>("SELECT * FROM Ability").ToList();
        }

        public override List<BaseStatsdata> GetAllBaseStats()
        {
            return db.Query<BaseStatsdata>("SELECT * FROM BaseStats").ToList();
        }

        public override List<EggMoveData> GetAllEggMoves()
        {
            return db.Query<EggMoveData>("SELECT * FROM EggMove").ToList();
        }

        public override List<EvolutionData> GetAllEvolution()
        {
            return db.Query<EvolutionData>("SELECT * FROM Evolution").ToList();
        }

        public override List<PokemonFormData> GetAllFormData()
        {
            return db.Query<PokemonFormData>("SELECT * FROM PokemonForm").ToList();
        }

        public override List<LevelUpMoveData> GetAllLevelUpMoves()
        {
            return db.Query<LevelUpMoveData>("SELECT * FROM LevelUpMove").ToList();
        }

        public override List<MoveData> GetAllMoves()
        {
            return db.Query<MoveData>("SELECT * FROM Move").ToList();
        }

        public override List<PokemonData> GetAllPokemon()
        {
            return db.Query<PokemonData>("SELECT * FROM Pokemon").ToList();
        }

        public override BaseStatsdata GetBaseStatsData(int pokemonID)
        {
            return db.QuerySingle<BaseStatsdata>("SELECT * FROM BaseStats WHERE PokemonID = @id", new { id = pokemonID });
        }

        public override EggMoveData GetEggMovesData(int pokemonID)
        {
            return db.QuerySingle<EggMoveData>("SELECT * FROM EggMove WHERE PokemonID = @id", new { id = pokemonID });
        }

        public override EvolutionData GetEvolutionData(int pokemonID)
        {
            return db.QuerySingle<EvolutionData>("SELECT * FROM Evolution WHERE PokemonID = @id", new { id = pokemonID });
        }

        public override PokemonFormData GetFormData(int pokemonID)
        {
            return db.QuerySingle<PokemonFormData>("SELECT * FROM PokemonForm WHERE PokemonID = @id", new { id = pokemonID });
        }

        public override LevelUpMoveData GetLevelUpMovesData(int pokemonID)
        {
            return db.QuerySingle<LevelUpMoveData>("SELECT * FROM LevelUpMove WHERE PokemonID = @id", new { id = pokemonID });
        }

        public override MoveData GetMoveData(string moveName)
        {
            return db.QuerySingle<MoveData>("SELECT * FROM Move WHERE MoveName = @MoveName", new { MoveName = moveName });
        }

        public override PokemonData GetPokemonData(int pokemonID)
        {
            return db.QuerySingle<PokemonData>("SELECT * FROM Pokemon WHERE PokemonID = @id", new { id = pokemonID });
        }
    }
}
