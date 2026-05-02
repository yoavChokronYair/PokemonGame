using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.Data.Repositories
{
    namespace PokemonGame.Services.Data.Repositories
    {
        internal class ParticipantRepository : DbRepository<int, BattleParticipantData>
        {
            internal ParticipantRepository(IDbConnectionService db) : base(db) { }

            // Updated to include the team used in this specific battle
            public void AddParticipant(int battleID, int battlePlayerID, int teamID)
            {
                _db.Execute(@"
                INSERT INTO BattleParticipants (BattleID, BattlePlayerID, TeamID, IsWinner) 
                VALUES (@bid, @bpid, @tid, 0);",
                    new { bid = battleID, bpid = battlePlayerID, tid = teamID });
            }

            public void UpdateParticipantResult(int battleID, int battlePlayerID, bool isWinner)
            {
                _db.Execute(@"
                UPDATE BattleParticipants 
                SET IsWinner = @winner 
                WHERE BattleID = @bid AND BattlePlayerID = @bpid",
                    new { winner = isWinner ? 1 : 0, bid = battleID, bpid = battlePlayerID });
            }

            public List<BattleParticipantData> GetParticipantsForBattle(int battleID) =>
                _db.Query<BattleParticipantData>(
                    "SELECT * FROM BattleParticipants WHERE BattleID = @bid",
                    new { bid = battleID }).ToList();
            public void SaveParticipant(BattleParticipantData participant)
            {
                _db.Execute(@"
                    INSERT INTO BattleParticipants (BattleID, BattlePlayerID, TeamID, IsWinner)
                    VALUES (@BattleID, @BattlePlayerID, @TeamID, @IsWinner)",
                    new
                    {
                        participant.BattleID,
                        participant.BattlePlayerID,
                        participant.TeamID,
                        participant.IsWinner
                    });
            }
        }
    }
}