using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.GameData.PokemonData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class AbilityRepository : DbRepository<int, AbilityData>
    {

        internal AbilityRepository(IDbConnectionService db) : base(db) { }

        // ─── AbilityData ────────────────────────────────────────────────────────

        public AbilityData? GetAbilityById(int id) =>
            _db.QuerySingle<AbilityData>(
                "SELECT * FROM abilities WHERE id = @id",
                new { id });

        public AbilityData? GetAbilityByName(string name) =>
            _db.QuerySingle<AbilityData>(
                "SELECT * FROM abilities WHERE name = @name",
                new { name });

        public List<AbilityData> GetAllAbilities() =>
            _db.Query<AbilityData>("SELECT * FROM abilities ORDER BY id ASC").ToList();

        public List<AbilityData> GetAbilitiesByTrigger(string trigger) =>
            _db.Query<AbilityData>(
                "SELECT * FROM abilities WHERE trigger = @trigger",
                new { trigger }).ToList();

        public List<AbilityData> GetAbilitiesByEffectId(int effectId) =>
            _db.Query<AbilityData>(
                "SELECT * FROM abilities WHERE effect_id = @effectId",
                new { effectId }).ToList();
    }
}