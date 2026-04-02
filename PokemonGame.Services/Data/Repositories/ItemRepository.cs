using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData;

namespace PokemonGame.Services.Data.Repositories
{
    internal class ItemRepository : DbRepository<int, ItemData>
    {
        internal ItemRepository(IDbConnectionService db) : base(db) { }

        public ItemData? GetById(int id) =>
            GetCached(id, () => _db.QuerySingle<ItemData?>(
                @"SELECT id AS Id, name AS Name, category AS Category, 
                     description AS Description, price AS Price, 
                     is_consumable AS Is_consumable, effect_id AS Effect_id, 
                     condition_id AS Condition_id
              FROM items WHERE id = @id", new { id }));

        public ItemData? GetByName(string name) =>
            _db.QuerySingle<ItemData?>(
                @"SELECT id AS Id, name AS Name, category AS Category, 
                     description AS Description, price AS Price, 
                     is_consumable AS Is_consumable, effect_id AS Effect_id, 
                     condition_id AS Condition_id
              FROM items WHERE name = @name", new { name });

        public List<ItemData> GetAllItems() =>
            GetAllCached(
                () => _db.Query<ItemData>("SELECT * FROM items").ToList(),
                i => i.Id);
        public List<ItemData> GetAllHeldItems() =>
            _db.Query<ItemData>(
                "SELECT * FROM items WHERE category = 'Held Item'").ToList();
    }
}
