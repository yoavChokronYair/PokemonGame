using PokemonGame.Services.Data.Move;
using PokemonGame.Services.Data.Pokemon;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.DataProvider
{
    public sealed class SQLiteDataProvider : GameDataProvider
    {
        private readonly ISQLiteConnectionService db;

        public SQLiteDataProvider(ISQLiteConnectionService dbService)
        {
            db = dbService;
        }

        // --- Pokémon ---
        public override PokemonData LoadPokemonData(int pokemonID)
        {
            return db.QuerySingle<PokemonData>(
                "SELECT * FROM Pokemon WHERE PokemonID = @id",
                new { id = pokemonID }
            );
        }

        public override PokemonFormData LoadFormData(int pokemonID)
        {
            return db.QuerySingle<PokemonFormData>(
                "SELECT * FROM PokemonForm WHERE PokemonID = @id",
                new { id = pokemonID }
            );
        }

        public override BaseStatsdata LoadBaseStatsData(int pokemonID)
        {
            return db.QuerySingle<BaseStatsdata>(
                "SELECT * FROM BaseStats WHERE PokemonID = @id",
                new { id = pokemonID }
            );
        }

        public override EvolutionData LoadEvolutionData(int pokemonID)
        {
            return db.QuerySingle<EvolutionData>(
                "SELECT * FROM Evolution WHERE PokemonID = @id",
                new { id = pokemonID }
            );
        }

        public override EggMoveData LoadEggMovesData(int pokemonID)
        {
            return db.QuerySingle<EggMoveData>(
                "SELECT * FROM EggMove WHERE PokemonID = @id",
                new { id = pokemonID }
            );
        }

        public override LevelUpMoveData LoadLevelUpMovesData(int pokemonID)
        {
            return db.QuerySingle<LevelUpMoveData>(
                "SELECT * FROM LevelUpMove WHERE PokemonID = @id",
                new { id = pokemonID }
            );
        }

        // --- Moves & Abilities ---
        public override MoveData LoadMoveData(string moveName)
        {
            return db.QuerySingle<MoveData>(
                "SELECT * FROM Move WHERE MoveName = @MoveName",
                new { MoveName = moveName }
            );
        }

        public override AbilityData LoadAbilityData(string abilityName)
        {
            return db.QuerySingle<AbilityData>(
                "SELECT * FROM Ability WHERE AbilityID = @abilityName",
                new { abilityName = abilityName }
            );
        }

        // --- “GetAll” methods ---
        public override List<PokemonData> GetAllPokemon() =>
            db.Query<PokemonData>("SELECT * FROM Pokemon").ToList();

        public override List<PokemonFormData> GetAllFormData() =>
            db.Query<PokemonFormData>("SELECT * FROM PokemonForm").ToList();

        public override List<BaseStatsdata> GetAllBaseStats() =>
            db.Query<BaseStatsdata>("SELECT * FROM BaseStats").ToList();

        public override List<EvolutionData> GetAllEvolution() =>
            db.Query<EvolutionData>("SELECT * FROM Evolution").ToList();

        public override List<EggMoveData> GetAllEggMoves() =>
            db.Query<EggMoveData>("SELECT * FROM EggMove").ToList();

        public override List<LevelUpMoveData> GetAllLevelUpMoves() =>
            db.Query<LevelUpMoveData>("SELECT * FROM LevelUpMove").ToList();

        public override List<MoveData> GetAllMoves() =>
            db.Query<MoveData>("SELECT * FROM Move").ToList();

        public override List<AbilityData> GetAllAbilities() =>
            db.Query<AbilityData>("SELECT * FROM Ability").ToList();
    }

}
