using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class BattleRepository : DbRepository<int, BattleRecordData>
    {
        internal BattleRepository(IDbConnectionService db) : base(db) { }

        // Records a new battle and returns the assigned BattleID
        public int CreateBattle(string battleDate)
        {
            _db.Execute(
                "INSERT INTO Battle (BattleDate) VALUES (@date)",
                new { date = battleDate });

            return _db.QuerySingle<int>("SELECT last_insert_rowid()");
        }

        // Sets the winner after the battle concludes
        public void FinalizeBattle(int battleID, int winnerPlayerID)
        {
            _db.Execute(
                "UPDATE Battle SET WinnerBattlePlayerID = @winner WHERE BattleID = @bid",
                new { winner = winnerPlayerID, bid = battleID });
        }

        // Gets a history of all battles for a specific player
        public List<BattleRecordData> GetPlayerBattleHistory(int battlePlayerID)
        {
            return _db.Query<BattleRecordData>(@"
                SELECT b.* FROM Battle b
                JOIN BattleParticipants p ON b.BattleID = p.BattleID
                WHERE p.BattlePlayerID = @bpid
                ORDER BY b.BattleDate DESC",
                new { bpid = battlePlayerID }).ToList();
        }
    }
}