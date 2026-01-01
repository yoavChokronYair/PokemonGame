using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.DataProvider.SQLite
{
    internal class SQLitePokemonRepository : IPokemonRepository
    {
        private readonly ISQLiteConnectionService db;

        public SQLitePokemonRepository(ISQLiteConnectionService dbService)
        {
            db = dbService;
        }

        public PokemonData LoadPokemonData(int pokemonID) =>
           db.QuerySingle<PokemonData>(
               "SELECT * FROM Pokemon WHERE PokemonID = @id",
               new { id = pokemonID });

        public PokemonFormData LoadFormData(int pokemonID) =>
            db.QuerySingle<PokemonFormData>(
                "SELECT * FROM PokemonForm WHERE PokemonID = @id",
                new { id = pokemonID });

        public BaseStatsData LoadBaseStatsData(int pokemonID) =>
            db.QuerySingle<BaseStatsData>(
                "SELECT * FROM BaseStats WHERE PokemonID = @id",
                new { id = pokemonID });

        public EvolutionData LoadEvolutionData(int pokemonID) =>
            db.QuerySingle<EvolutionData>(
                "SELECT * FROM Evolution WHERE PokemonID = @id",
                new { id = pokemonID });

        public EggMoveData LoadEggMovesData(int pokemonID) =>
            db.QuerySingle<EggMoveData>(
                "SELECT * FROM EggMove WHERE PokemonID = @id",
                new { id = pokemonID });

        public LevelUpMoveData LoadLevelUpMovesData(int pokemonID) =>
            db.QuerySingle<LevelUpMoveData>(
                "SELECT * FROM LevelUpMove WHERE PokemonID = @id",
                new { id = pokemonID });

        public List<PokemonData> GetAllPokemon() =>
            db.Query<PokemonData>("SELECT * FROM Pokemon").ToList();

        public  List<PokemonFormData> GetAllFormData() =>
            db.Query<PokemonFormData>("SELECT * FROM PokemonForm").ToList();

        public List<BaseStatsData> GetAllBaseStats() =>
            db.Query<BaseStatsData>("SELECT * FROM BaseStats").ToList();

        public List<EvolutionData> GetAllEvolution() =>
            db.Query<EvolutionData>("SELECT * FROM Evolution").ToList();

        public List<EggMoveData> GetAllEggMoves() =>
            db.Query<EggMoveData>("SELECT * FROM EggMove").ToList();

        public List<LevelUpMoveData> GetAllLevelUpMoves() =>
            db.Query<LevelUpMoveData>("SELECT * FROM LevelUpMove").ToList();
    }
}
