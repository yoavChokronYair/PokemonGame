using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class BattleTeamSnapshotRepository : DbRepository<int, BattleTeamSnapshotData>
    {
        internal BattleTeamSnapshotRepository(IDbConnectionService db) : base(db) { }

        private const string SnapshotSelect =
    @"SELECT BattleID AS BattleID,
             BattlePlayerID AS BattlePlayerID,
             Slot AS Slot,
             PokemonID AS PokemonID
      FROM BattleTeamSnapshot";

        public List<BattleTeamSnapshotData> GetByBattle(int battleId) =>
            _db.Query<BattleTeamSnapshotData>($"{SnapshotSelect} WHERE BattleID = @bid", new { bid = battleId }).ToList();

        public List<BattleTeamSnapshotData> GetByBattleAndPlayer(int battleId, int battlePlayerId) =>
            _db.Query<BattleTeamSnapshotData>($"{SnapshotSelect} WHERE BattleID = @bid AND BattlePlayerID = @bpid",
                new { bid = battleId, bpid = battlePlayerId }).ToList();

        public void SaveSnapshot(int battleId, int battlePlayerId, int teamId)
        {
            var members = _db.Query<(int Slot, int PokemonId)>(
                "SELECT slot_number AS Slot, pokemonID AS PokemonId FROM team_members WHERE team_id = @tid",
                new { tid = teamId }).ToList();

            foreach (var member in members)
            {
                _db.Execute(
                    @"INSERT OR IGNORE INTO BattleTeamSnapshot (BattleID, BattlePlayerID, Slot, PokemonID)
              VALUES (@bid, @bpid, @slot, @pid)",
                    new { bid = battleId, bpid = battlePlayerId, slot = member.Slot, pid = member.PokemonId });
            }
        }

        public void DeleteByBattle(int battleId) =>
            _db.Execute("DELETE FROM BattleTeamSnapshot WHERE BattleID = @bid", new { bid = battleId });

        public void Upsert(BattleTeamSnapshotData r)
        {
            _db.Execute(
                @"INSERT OR REPLACE INTO BattleTeamSnapshot (BattleID, BattlePlayerID, Slot, PokemonID)
                    VALUES (@BattleID, @BattlePlayerID, @Slot, @PokemonID)", r);
        }
    }
}