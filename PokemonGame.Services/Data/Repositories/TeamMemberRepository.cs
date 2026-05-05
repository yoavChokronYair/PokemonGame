using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class TeamMemberRepository : DbRepository<int, TeamMemberData>
    {
        internal TeamMemberRepository(IDbConnectionService db) : base(db) { }

        // Fetch all members for a specific team
        public List<TeamMemberData> GetTeamMembers(int teamID) =>
            _db.Query<TeamMemberData>(
                "SELECT * FROM team_members WHERE team_id = @tid ORDER BY slot_number ASC",
                new { tid = teamID }).ToList();

        // Add or Replace a Pokemon in a specific slot
        public void SetPokemonInSlot(int teamID, int pokemonID, int slotNumber)
        {
            // We use REPLACE to handle updating an existing slot without needing a separate update/insert check
            _db.Execute(@"
                REPLACE INTO team_members (team_id, pokemonID, slot_number) 
                VALUES (@tid, @pid, @slot);",
                new { tid = teamID, pid = pokemonID, slot = slotNumber });
        }

        // Remove a Pokemon from a team (e.g., when releasing or swapping)
        public void RemovePokemonFromTeam(int teamID, int pokemonID)
        {
            _db.Execute(
                "DELETE FROM team_members WHERE team_id = @tid AND pokemonID = @pid",
                new { tid = teamID, pid = pokemonID });
        }

        // Clear an entire team
        public void ClearTeam(int teamID)
        {
            _db.Execute("DELETE FROM team_members WHERE team_id = @tid", new { tid = teamID });
        }
        public void Upsert(TeamMemberData r)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO team_members (team_id, pokemonID, slot_number) VALUES (@tid, @pid, @slot)",
                new { tid = r.Team_id, pid = r.PokemonID, slot = r.Slot_number });
        }

    }
}