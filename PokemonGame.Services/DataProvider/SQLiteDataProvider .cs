using PokemonGame.Services.Data;
using PokemonGame.Services.Data.Move;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.Data.User;
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
        // ---------------------------
        // USER METHODS
        // ---------------------------

        public override UserData? LoadUserByName(string username)
        {

            var user = db.QuerySingle<UserData>(
                "SELECT UserName, Password FROM Users WHERE UserName = @UserName",
                new { UserName = username }
            );
            return user;
        }

        public override bool UserExists(string username)
        {
            // Simply use LoadUserByName
            return LoadUserByName(username) != null;
        }

        public override UserData CreateUser(string username, int passwordHash)
        {
            // 1️⃣ Insert the user
            const string insertSql = @"
            INSERT INTO Users (UserName, Password)
            VALUES (@UserName, @Password);";

            db.Execute(insertSql, new { UserName = username, Password = passwordHash });

            // 2️⃣ Get the last inserted row ID
            const string selectSql = @"
            SELECT UserID, UserName, Password
            FROM Users
            WHERE UserID = last_insert_rowid();";

            return db.QuerySingle<UserData>(selectSql);
        }


        public override List<UserData> GetAllUsers()
        {
            return db.Query<UserData>("SELECT * FROM Users");
        }
        // ---------------------------
        // ONLINE PLAYER METHODS
        // ---------------------------

        public override bool OnlinePlayerExists(string username, UserData user)
        {
            const string sql = @"SELECT COUNT(*) 
                                FROM BattlePlayer 
                                WHERE Name = @name AND UserID = @uid;";

            int count = db.QuerySingle<int>(sql, new { name = username, uid = user.UserID });
            return count > 0;
        }


        public override BattlePlayerData? LoadOnlinePlayerByName(string username, UserData user)
        {
            const string sql = @"SELECT ID, UserID, Name, Level
                                FROM BattlePlayer
                                WHERE Name = @name AND UserID = @uid;";

            return db.QuerySingle<BattlePlayerData>(sql,
                new { name = username, uid = user.UserID });
        }


        public override BattlePlayerData CreateOnlinePlayer(string username, UserData user)
        {
            const string insertSql = @"INSERT INTO BattlePlayer (UserID, Name, Level)
                                     VALUES (@uid, @name, 1);";

            db.Execute(insertSql, new { uid = user.UserID, name = username });

            const string selectSql = @"SELECT ID, UserID, Name, Level
                                        FROM BattlePlayer
                                        WHERE ID = last_insert_rowid();";

            return db.QuerySingle<BattlePlayerData>(selectSql);
        }
        public override List<BattlePlayerData> GetAllOnlinePlayers(UserData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Query all BattlePlayer rows
            const string sql = @"SELECT * FROM BattlePlayer";
            var list = db.Query<BattlePlayerData>(sql).ToList();

            // Filter by UserID
            var userPlayers = list.Where(m => m.UserID == data.UserID).ToList();

            return userPlayers;
        }
    }

}
