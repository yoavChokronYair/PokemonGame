using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.Interfaces;

namespace PokemonGame.Services.Data.DataProvider.SQLite
{
    internal class SQLitePokemonRepository : IPokemonRepository
    {
        private readonly ISQLiteConnectionService _db;

        public SQLitePokemonRepository(ISQLiteConnectionService dbService)
        {
            _db = dbService;
        }

        public PokemonData LoadPokemonData(int pokemonID) =>
           _db.QuerySingle<PokemonData>(
               "SELECT * FROM Pokemon WHERE PokemonID = @id",
               new { id = pokemonID });

        public PokemonFormData LoadFormData(int pokemonID) =>
            _db.QuerySingle<PokemonFormData>(
                "SELECT * FROM PokemonForm WHERE PokemonID = @id",
                new { id = pokemonID });

        public BaseStatsData LoadBaseStatsData(int pokemonID) =>
            _db.QuerySingle<BaseStatsData>(
                "SELECT * FROM BaseStats WHERE PokemonID = @id",
                new { id = pokemonID });

        public EvolutionData LoadEvolutionData(int pokemonID) =>
            _db.QuerySingle<EvolutionData>(
                "SELECT * FROM Evolution WHERE PokemonID = @id",
                new { id = pokemonID });

        public EggMoveData LoadEggMovesData(int pokemonID) =>
            _db.QuerySingle<EggMoveData>(
                "SELECT * FROM EggMove WHERE PokemonID = @id",
                new { id = pokemonID });

        public LevelUpMoveData LoadLevelUpMovesData(int pokemonID) =>
            _db.QuerySingle<LevelUpMoveData>(
                "SELECT * FROM LevelUpMove WHERE PokemonID = @id",
                new { id = pokemonID });

        public List<PokemonData> GetAllPokemon() =>
            _db.Query<PokemonData>("SELECT * FROM Pokemon").ToList();

        public List<PokemonFormData> GetAllFormData() =>
            _db.Query<PokemonFormData>("SELECT * FROM PokemonForm").ToList();

        public List<BaseStatsData> GetAllBaseStats() =>
            _db.Query<BaseStatsData>("SELECT * FROM BaseStats").ToList();

        public List<EvolutionData> GetAllEvolution() =>
            _db.Query<EvolutionData>("SELECT * FROM Evolution").ToList();

        public List<EggMoveData> GetAllEggMoves() =>
            _db.Query<EggMoveData>("SELECT * FROM EggMove").ToList();

        public List<LevelUpMoveData> GetAllLevelUpMoves() =>
            _db.Query<LevelUpMoveData>("SELECT * FROM LevelUpMove").ToList();
    }
}
