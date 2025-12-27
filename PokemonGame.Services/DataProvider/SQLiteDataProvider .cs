using PokemonGame.Services.Data;
using PokemonGame.Services.Data.Move;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.Data.User;
using PokemonGame.Services.Data.User.OnlinePlayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Services.DataProvider
{
    public sealed class SQLiteDataProvider : GameDataProvider
    {
        private readonly ISQLiteConnectionService db;

        public SQLiteDataProvider(ISQLiteConnectionService dbService)
        {
            db = dbService;
        }

        #region Pokémon Loaders

        public override PokemonData LoadPokemonData(int pokemonID) =>
            db.QuerySingle<PokemonData>(
                "SELECT * FROM Pokemon WHERE PokemonID = @id",
                new { id = pokemonID });

        public override PokemonFormData LoadFormData(int pokemonID) =>
            db.QuerySingle<PokemonFormData>(
                "SELECT * FROM PokemonForm WHERE PokemonID = @id",
                new { id = pokemonID });

        public override BaseStatsData LoadBaseStatsData(int pokemonID) =>
            db.QuerySingle<BaseStatsData>(
                "SELECT * FROM BaseStats WHERE PokemonID = @id",
                new { id = pokemonID });

        public override EvolutionData LoadEvolutionData(int pokemonID) =>
            db.QuerySingle<EvolutionData>(
                "SELECT * FROM Evolution WHERE PokemonID = @id",
                new { id = pokemonID });

        public override EggMoveData LoadEggMovesData(int pokemonID) =>
            db.QuerySingle<EggMoveData>(
                "SELECT * FROM EggMove WHERE PokemonID = @id",
                new { id = pokemonID });

        public override LevelUpMoveData LoadLevelUpMovesData(int pokemonID) =>
            db.QuerySingle<LevelUpMoveData>(
                "SELECT * FROM LevelUpMove WHERE PokemonID = @id",
                new { id = pokemonID });

        public override List<PokemonData> GetAllPokemon() =>
            db.Query<PokemonData>("SELECT * FROM Pokemon").ToList();

        public override List<PokemonFormData> GetAllFormData() =>
            db.Query<PokemonFormData>("SELECT * FROM PokemonForm").ToList();

        public override List<BaseStatsData> GetAllBaseStats() =>
            db.Query<BaseStatsData>("SELECT * FROM BaseStats").ToList();

        public override List<EvolutionData> GetAllEvolution() =>
            db.Query<EvolutionData>("SELECT * FROM Evolution").ToList();

        public override List<EggMoveData> GetAllEggMoves() =>
            db.Query<EggMoveData>("SELECT * FROM EggMove").ToList();

        public override List<LevelUpMoveData> GetAllLevelUpMoves() =>
            db.Query<LevelUpMoveData>("SELECT * FROM LevelUpMove").ToList();

        #endregion

        #region Moves & Abilities

        public override MoveData LoadMoveData(string moveName) =>
            db.QuerySingle<MoveData>(
                "SELECT * FROM Move WHERE MoveName = @moveName",
                new { moveName });

        public override AbilityData LoadAbilityData(string abilityName) =>
            db.QuerySingle<AbilityData>(
                "SELECT * FROM Ability WHERE AbilityName = @abilityName",
                new { abilityName });

        public override List<MoveData> GetAllMoves() =>
            db.Query<MoveData>("SELECT * FROM Move").ToList();

        public override List<AbilityData> GetAllAbilities() =>
            db.Query<AbilityData>("SELECT * FROM Ability").ToList();

        #endregion

        #region User Methods

        public override UserData? LoadUserByName(string username) =>
            db.QuerySingle<UserData?>(
                "SELECT * FROM Users WHERE UserName = @UserName",
                new { UserName = username });

        public override bool UserExists(string username) => LoadUserByName(username) != null;

        public override UserData CreateUser(string username, int passwordHash)
        {
            db.Execute(
                "INSERT INTO Users (UserName, Password) VALUES (@UserName, @Password);",
                new { UserName = username, Password = passwordHash });

            return db.QuerySingle<UserData>(
                "SELECT * FROM Users WHERE UserID = last_insert_rowid();");
        }

        public override List<UserData> GetAllUsers() =>
            db.Query<UserData>("SELECT * FROM Users").ToList();

        #endregion

        #region Online Player (BattlePlayer)

        public override bool OnlinePlayerExists(string username, UserData user)
        {
            const string sql = @"SELECT COUNT(*) FROM BattlePlayer 
                                 WHERE Name = @name AND UserID = @uid;";
            int count = db.QuerySingle<int>(sql, new { name = username, uid = user.UserID });
            return count > 0;
        }

        public override BattlePlayerData? LoadOnlinePlayerByName(string username, UserData user)
        {
            const string sql = @"SELECT * FROM BattlePlayer 
                                 WHERE Name = @name AND UserID = @uid;";
            return db.QuerySingle<BattlePlayerData?>(sql, new { name = username, uid = user.UserID });
        }

        public override BattlePlayerData CreateOnlinePlayer(string username, UserData user)
        {
            db.Execute(
                "INSERT INTO BattlePlayer (UserID, Name, Level) VALUES (@uid, @name, 1);",
                new { uid = user.UserID, name = username });

            return db.QuerySingle<BattlePlayerData>(
                "SELECT * FROM BattlePlayer WHERE BattlePlayerID = last_insert_rowid();");
        }

        public override List<BattlePlayerData> GetAllOnlinePlayers(UserData user) =>
            db.Query<BattlePlayerData>(
                "SELECT * FROM BattlePlayer WHERE UserID = @uid;",
                new { uid = user.UserID }).ToList();
        public override BattlePlayerData? LoadOpponentPlayer(BattlePlayerData player, int battleID)
        {
            const string sql = @"
        SELECT bp.*
        FROM BattleTeam bt
        JOIN BattlePlayer bp ON bp.BattlePlayerID = bt.BattlePlayerID
        WHERE bt.BattleID = @battleID
          AND bt.BattlePlayerID != @playerID
        LIMIT 1;";

            return db.QuerySingle<BattlePlayerData?>(
                sql,
                new { battleID, playerID = player.BattlePlayerID }
            );
        }

        #endregion

        #region Battles

        public override List<BattleHistoryEntryData> GetBattleHistory(BattlePlayerData player)
        {
            const string sql = @"
                                SELECT
                                    b.BattleID,
                                    b.BattleDate,
                                    opp.Name AS OpponentName,
                                    CASE 
                                        WHEN b.WinnerBattlePlayerID = @pid THEN 1
                                        ELSE 0
                                    END AS IsWin
                                FROM Battle b
                                JOIN BattleTeam myTeam
                                    ON myTeam.BattleID = b.BattleID
                                   AND myTeam.BattlePlayerID = @pid
                                JOIN BattleTeam oppTeam
                                    ON oppTeam.BattleID = b.BattleID
                                   AND oppTeam.BattlePlayerID != @pid
                                JOIN BattlePlayer opp
                                    ON opp.BattlePlayerID = oppTeam.BattlePlayerID
                                ORDER BY b.BattleDate DESC;
                            ";

            return db.Query<BattleHistoryEntryData>(
                sql,
                new { pid = player.BattlePlayerID }
            ).ToList();
        }
        // Pokémon used by a BattlePlayer in a battle
        public override List<PokemonData> GetBattleTeamPokemonForPlayer(int battleID, int battlePlayerID)
        {
            const string sql = @"
                                SELECT p.PokemonID, p.SpeciesName
                                FROM BattleTeamPokemon btp
                                JOIN BattleTeam bt ON bt.BattleTeamID = btp.BattleTeamID
                                JOIN Pokemon p ON p.PokemonID = btp.PokemonID
                                WHERE bt.BattleID = @bid AND bt.BattlePlayerID = @pid;
                            ";

            return db.Query<PokemonData>(sql, new { bid = battleID, pid = battlePlayerID }).ToList();
        }

        // Get the opponent BattlePlayer
        public override BattlePlayerData? GetOpponentPlayer(int battleID, int playerID)
        {
            const string sql = @"
                                SELECT * FROM BattlePlayer
                                WHERE BattleID = @bid AND BattlePlayerID != @pid;
                            ";

            return db.QuerySingle<BattlePlayerData?>(sql, new { bid = battleID, pid = playerID });
        }

        #endregion


    }
}
