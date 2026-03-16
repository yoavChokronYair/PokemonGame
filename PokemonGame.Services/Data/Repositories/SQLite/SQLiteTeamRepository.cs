using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.Data.Repositories.SQLite
{
    internal class SQLiteTeamRepository : SQLiteRepository<int, TeamData>
    {
        internal SQLiteTeamRepository(ISQLiteConnectionService db) : base(db) { }

        // Fetch all teams belonging to a specific User
        public List<TeamData> GetUserTeams(int userID) =>
            _db.Query<TeamData>("SELECT * FROM teams WHERE user_id = @uid", new { uid = userID }).ToList();

        // Get a specific team by its unique ID
        public TeamData? GetTeamById(int teamID) =>
            _db.QuerySingle<TeamData>("SELECT * FROM teams WHERE id = @tid", new { tid = teamID });

        // Create a new empty team
        public TeamData CreateTeam(string teamName, int userID)
        {
            _db.Execute(
                "INSERT INTO teams (team_name, user_id) VALUES (@name, @uid)",
                new { name = teamName, uid = userID });

            return _db.QuerySingle<TeamData>("SELECT * FROM teams WHERE id = last_insert_rowid()");
        }

        // Link an existing team to a BattlePlayer session
        public void AssignTeamToBattlePlayer(int teamID, int battlePlayerID)
        {
            _db.Execute(
                "UPDATE teams SET battle_player_id = @bpid WHERE id = @tid",
                new { bpid = battlePlayerID, tid = teamID });
        }

        // Remove the link when a battle session ends
        public void UnassignTeamFromBattlePlayer(int teamID)
        {
            _db.Execute(
                "UPDATE teams SET battle_player_id = NULL WHERE id = @tid",
                new { tid = teamID });
        }
    }
}