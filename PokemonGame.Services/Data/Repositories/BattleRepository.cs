using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class BattleRepository : DbRepository<int, BattleRecordData>
    {
        internal BattleRepository(IDbConnectionService db) : base(db) { }

        // Records a new battle and returns the assigned BattleID
        public int CreateBattle()
        {
            // Using SQLite's datetime('now') directly is safer for consistency
            _db.Execute("INSERT INTO Battle (BattleDate) VALUES (datetime('now'))");

            return _db.QuerySingle<int>("SELECT last_insert_rowid()");
        }

        // Note: FinalizeBattle is removed here because winners are now updated 
        // in the BattleParticipants table via ParticipantRepository.UpdateParticipantResult

        // Gets a history of all battles for a specific player
        public List<BattleRecordData> GetPlayerBattleHistory(int battlePlayerID)
        {
            return _db.Query<BattleRecordData>(@"
                SELECT b.BattleID, b.BattleDate 
                FROM Battle b
                INNER JOIN BattleParticipants p ON b.BattleID = p.BattleID
                WHERE p.BattlePlayerID = @bpid
                ORDER BY b.BattleDate DESC",
                new { bpid = battlePlayerID }).ToList();
        }
        public void Upsert(BattleRecordData r)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO Battle (BattleID, BattleDate) VALUES (@id, @date)",
                new { id = r.BattleID, date = r.BattleDate });
        }
    }
}