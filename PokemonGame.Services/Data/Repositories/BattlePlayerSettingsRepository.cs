using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User; // Ensure this matches your Settings Data model namespace

namespace PokemonGame.Services.Data.Repositories
{
    internal class BattlePlayerSettingsRepository : DbRepository<int, BattlePlayerSettingsData>
    {
        // This was missing! You need this to pass the db to the base class.
        internal BattlePlayerSettingsRepository(IDbConnectionService db) : base(db) { }

        public BattlePlayerSettingsData GetSettings(int battlePlayerId)
        {
            return GetCached(battlePlayerId, () =>
            {
                var settings = _db.QuerySingle<BattlePlayerSettingsData>(
                    "SELECT * FROM BattlePlayerSettings WHERE BattlePlayerID = @id",
                    new { id = battlePlayerId });

                return settings ?? CreateDefaultSettings(battlePlayerId);
            })!;
        }

        private BattlePlayerSettingsData CreateDefaultSettings(int battlePlayerId)
        {
            return StoreAndReturn(battlePlayerId, () =>
            {
                _db.Execute(@"
                    INSERT INTO BattlePlayerSettings (BattlePlayerID, AnimationsEnabled, TextSpeedID, BackgroundID, ShowTypeEffectiveness)
                    VALUES (@id, 1, 2, 1, 1);",
                    new { id = battlePlayerId });

                return _db.QuerySingle<BattlePlayerSettingsData>(
                    "SELECT * FROM BattlePlayerSettings WHERE BattlePlayerID = @id",
                    new { id = battlePlayerId });
            });
        }

        public void SaveSetting(int battlePlayerId, string column, int value)
        {
            _db.Execute($"UPDATE BattlePlayerSettings SET {column} = @val, UpdatedAt = datetime('now') WHERE BattlePlayerID = @id",
                new { val = value, id = battlePlayerId });
        }
    }
}