using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.Data.Repositories
{
    internal class TeamRepository : DbRepository<int, TeamData>
    {
        internal TeamRepository(IDbConnectionService db) : base(db) { }
        public string ConnectionString => _db.ConnectionString;

        public bool CanCreateTeam(int battlePlayerId) =>
            GetTeamsByBattlePlayer(battlePlayerId).Count < 3;

        // Removed user_id from the SELECT statement
        private const string TeamSelect =
                @"SELECT id AS Id, 
                         team_name AS TeamName, 
                         battle_player_id AS BattlePlayerId
                  FROM teams";

        // This is now your primary way to get teams
        public List<TeamData> GetTeamsByBattlePlayer(int battlePlayerId) =>
            _db.Query<TeamData>($"{TeamSelect} WHERE battle_player_id = @bpid", new { bpid = battlePlayerId }).ToList();

        public TeamData? GetTeamById(int teamID) =>
            _db.QuerySingle<TeamData>($"{TeamSelect} WHERE id = @tid", new { tid = teamID });

        public TeamData? GetTeamByBattlePlayer(int battlePlayerId)
        {
            var id = _db.QueryScalar<int>(
                "SELECT id FROM teams WHERE battle_player_id = @bpid ORDER BY id DESC LIMIT 1",
                new { bpid = battlePlayerId });

            if (id == 0) return null;

            return _db.QuerySingle<TeamData>(
                $"{TeamSelect} WHERE id = @tid",
                new { tid = id });
        }

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
        public bool IsFavoriteTeam(int teamId)
        {
            int count = _db.QueryScalar<int>(
                "SELECT COUNT(1) FROM BattlePlayerStats WHERE FaveTeamID = @tid",
                new { tid = teamId }
            );

            return count > 0;
        }
        public void DeleteTeam(int teamId)
        {
            // Get all pokemon IDs for this team
            var pokemonIds = _db.QueryScalarList<int>(
                "SELECT pokemonID FROM team_members WHERE team_id = @tid",
                new { tid = teamId }).ToList();

            // Delete each battler_pokemon instance
            foreach (var pid in pokemonIds)
                _db.Execute("DELETE FROM battler_pokemon WHERE pokemonID = @pid",
                    new { pid = pid });

            // Delete team members
            _db.Execute("DELETE FROM team_members WHERE team_id = @tid",
                new { tid = teamId });

            // Delete the team
            _db.Execute("DELETE FROM teams WHERE id = @tid",
                new { tid = teamId });
        }
        public void Upsert(TeamData r)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO teams (id, team_name, battle_player_id) VALUES (@id, @name, @bpid)",
                new { id = r.Id, name = r.TeamName, bpid = r.BattlePlayerId });
        }
    }
}