using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.DataProvider.SQLite
{
    internal class SQLiteMoveRepository : IMoveRepository
    {
        private readonly ISQLiteConnectionService db;

        public SQLiteMoveRepository(ISQLiteConnectionService dbService)
        {
            db = dbService;
        }

        public MoveData LoadMoveData(string moveName) =>
           db.QuerySingle<MoveData>(
               "SELECT * FROM Move WHERE MoveName = @moveName",
               new { moveName });

        public AbilityData LoadAbilityData(string abilityName) =>
            db.QuerySingle<AbilityData>(
                "SELECT * FROM Ability WHERE AbilityName = @abilityName",
                new { abilityName });

        public List<MoveData> GetAllMoves() =>
            db.Query<MoveData>("SELECT * FROM Move").ToList();

        public  List<AbilityData> GetAllAbilities() =>
            db.Query<AbilityData>("SELECT * FROM Ability").ToList();

    }
}
