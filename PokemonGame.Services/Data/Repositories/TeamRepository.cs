using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.Data.Repositories
{
    internal class TeamRepository : DbRepository<int, TeamData>
    {
        internal TeamRepository(IDbConnectionService db) : base(db) { }

        public bool CanCreateTeam(int battlePlayerId) =>
            GetTeamsByBattlePlayer(battlePlayerId).Count < 3;

        // Removed user_id from the SELECT statement
        private const string TeamSelect =
            @"SELECT id AS Id, 
                     team_name AS TeamName, 
                     battle_player_id AS Battle_player_id 
              FROM teams";

        // This is now your primary way to get teams
        public List<TeamData> GetTeamsByBattlePlayer(int battlePlayerId) =>
            _db.Query<TeamData>($"{TeamSelect} WHERE battle_player_id = @bpid", new { bpid = battlePlayerId }).ToList();

        public TeamData? GetTeamById(int teamID) =>
            _db.QuerySingle<TeamData>($"{TeamSelect} WHERE id = @tid", new { tid = teamID });

        public TeamData? GetTeamByBattlePlayer(int battlePlayerId) =>
            _db.QuerySingle<TeamData>($"{TeamSelect} WHERE battle_player_id = @bpid ORDER BY id DESC LIMIT 1", new { bpid = battlePlayerId });

        // Removed userID parameter and from the INSERT statement
        public TeamData CreateTeam(string teamName, int battlePlayerId)
        {
            if (!CanCreateTeam(battlePlayerId))
            {
                throw new InvalidOperationException("Max 3 teams per battle player.");
            }

            _db.ExecuteAndGetLastId(
                "INSERT INTO teams (team_name, battle_player_id) VALUES (@name, @bpid)",
                new { name = teamName, bpid = battlePlayerId });

            var team = _db.QuerySingle<TeamData>(
                $"{TeamSelect} WHERE team_name = @name AND battle_player_id = @bpid ORDER BY id DESC LIMIT 1",
                new { name = teamName, bpid = battlePlayerId });

            return team;
        }

        public void UpdateTeamName(int teamId, string newName) =>
            _db.Execute("UPDATE teams SET team_name = @name WHERE id = @tid",
                new { name = newName, tid = teamId });

        public void DeleteTeam(int teamId) =>
            _db.Execute("DELETE FROM teams WHERE id = @tid", new { tid = teamId });
        
    }
}