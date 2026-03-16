using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.Data.Repositories.SQLite
{
    internal class SQLiteParticipantRepository : SQLiteRepository<int, BattleParticipantData>
    {
        internal SQLiteParticipantRepository(ISQLiteConnectionService db) : base(db) { }

        // Records a participant's entry into a battle
        public void AddParticipant(int battleID, int battlePlayerID)
        {
            _db.Execute(@"
                INSERT INTO BattleParticipants (BattleID, BattlePlayerID, IsWinner, Score) 
                VALUES (@bid, @bpid, 0, 0);",
                new { bid = battleID, bpid = battlePlayerID });
        }

        // Updates the results for a specific participant after the battle concludes
        public void UpdateParticipantResult(int battleID, int battlePlayerID, bool isWinner, int score)
        {
            _db.Execute(@"
                UPDATE BattleParticipants 
                SET IsWinner = @winner, Score = @score 
                WHERE BattleID = @bid AND BattlePlayerID = @bpid",
                new { winner = isWinner ? 1 : 0, score, bid = battleID, bpid = battlePlayerID });
        }

        // Gets all participants for a single battle (useful for post-game summaries)
        public List<BattleParticipantData> GetParticipantsForBattle(int battleID) =>
            _db.Query<BattleParticipantData>(
                "SELECT * FROM BattleParticipants WHERE BattleID = @bid",
                new { bid = battleID }).ToList();
    }
}