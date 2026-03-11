using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.Interfaces;

namespace PokemonGame.Services.Data.DataProvider.SQLite
{
    internal class SQLiteMoveRepository : IMoveRepository
    {
        private readonly ISQLiteConnectionService _db;

        public SQLiteMoveRepository(ISQLiteConnectionService dbService)
        {
            _db = dbService;
        }

        public MoveData LoadMoveData(string moveName) =>
           _db.QuerySingle<MoveData>(
               "SELECT * FROM Move WHERE MoveName = @moveName",
               new { moveName });

        public AbilityData LoadAbilityData(string abilityName) =>
            _db.QuerySingle<AbilityData>(
                "SELECT * FROM Ability WHERE AbilityName = @abilityName",
                new { abilityName });

        public List<MoveData> GetAllMoves() =>
            _db.Query<MoveData>("SELECT * FROM Move").ToList();

        public List<AbilityData> GetAllAbilities() =>
            _db.Query<AbilityData>("SELECT * FROM Ability").ToList();

    }
}
