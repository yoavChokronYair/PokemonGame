using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Repositories
{
    // Moved OUTSIDE of the Settings repository
    internal class BattlePlayerStatsRepository : DbRepository<int, BattlePlayerStatsData>
    {
        internal BattlePlayerStatsRepository(IDbConnectionService db) : base(db) { }

        public BattlePlayerStatsData GetStats(int battlePlayerId)
        {
            return GetCached(battlePlayerId, () =>
            {
                // Changed QuerySingle to QuerySingleOrDefault to avoid crashes if row is missing
                var stats = _db.QuerySingle<BattlePlayerStatsData>(
                    "SELECT * FROM BattlePlayerStats WHERE BattlePlayerID = @id",
                    new { id = battlePlayerId });

                return stats ?? CreateDefaultStats(battlePlayerId);
            })!;
        }

        private BattlePlayerStatsData CreateDefaultStats(int battlePlayerId)
        {
            return StoreAndReturn(battlePlayerId, () =>
            {
                _db.Execute(@"
                INSERT INTO BattlePlayerStats (BattlePlayerID, CurrentElo1v1, PeakElo1v1, CurrentElo2v2, PeakElo2v2)
                VALUES (@id, 1000, 1000, 1000, 1000);",
                    new { id = battlePlayerId });

                return _db.QuerySingle<BattlePlayerStatsData>(
                    "SELECT * FROM BattlePlayerStats WHERE BattlePlayerID = @id",
                    new { id = battlePlayerId });
            });
        }

        public void UpdateElo(int battlePlayerId, int newElo, bool isSingles)
        {
            string eloCol = isSingles ? "CurrentElo1v1" : "CurrentElo2v2";
            string peakCol = isSingles ? "PeakElo1v1" : "PeakElo2v2";

            _db.Execute($@"
                UPDATE BattlePlayerStats 
                SET {eloCol} = @elo,
                    {peakCol} = MAX({peakCol}, @elo)
                WHERE BattlePlayerID = @id",
                new { elo = newElo, id = battlePlayerId });
        }

        public void RegisterWin(int battlePlayerId, bool isSingles)
        {
            string winCol = isSingles ? "Wins1v1" : "Wins2v2";
            string streakCol = isSingles ? "CurrentStreak1v1" : "CurrentStreak2v2";
            string bestCol = isSingles ? "BestStreak1v1" : "BestStreak2v2";

            _db.Execute($@"
                UPDATE BattlePlayerStats 
                SET {winCol} = {winCol} + 1,
                    {streakCol} = {streakCol} + 1,
                    {bestCol} = MAX({bestCol}, {streakCol} + 1)
                WHERE BattlePlayerID = @id",
                new { id = battlePlayerId });
        }

        public void RegisterLoss(int battlePlayerId, bool isSingles)
        {
            string streakCol = isSingles ? "CurrentStreak1v1" : "CurrentStreak2v2";
            _db.Execute($"UPDATE BattlePlayerStats SET {streakCol} = 0 WHERE BattlePlayerID = @id",
                new { id = battlePlayerId });
        }

        public void SaveFaveTeam(int battlePlayerId, int teamId)
        {
            _db.Execute("UPDATE BattlePlayerStats SET FaveTeamID = @teamId WHERE BattlePlayerID = @id",
                new { teamId, id = battlePlayerId });
        }
    }
}