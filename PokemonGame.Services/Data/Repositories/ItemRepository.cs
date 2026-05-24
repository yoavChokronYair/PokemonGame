using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData;

namespace PokemonGame.Services.Data.Repositories
{
    // =========================================================
    // items
    // =========================================================

    internal class ItemRepository : DbRepository<int, ItemData>
    {
        internal ItemRepository(IDbConnectionService db) : base(db) { }

        public ItemData? GetById(int id) =>
            GetCached(id, () => _db.QuerySingle<ItemData?>(
                @"SELECT
                    id AS Id,
                    name AS Name,
                    item_type AS Item_type,
                    category AS Category,
                    description AS Description,
                    price AS Price,
                    is_consumable AS Is_consumable,
                    usable_in_battle AS Usable_in_battle,
                    usable_in_field AS Usable_in_field,
                    effect_id AS Effect_id,
                    condition_id AS Condition_id
                  FROM items
                  WHERE id = @id",
                new { id }));

        public ItemData? GetByName(string name) =>
            _db.QuerySingle<ItemData?>(
                @"SELECT
                    id AS Id,
                    name AS Name,
                    item_type AS Item_type,
                    category AS Category,
                    description AS Description,
                    price AS Price,
                    is_consumable AS Is_consumable,
                    usable_in_battle AS Usable_in_battle,
                    usable_in_field AS Usable_in_field,
                    effect_id AS Effect_id,
                    condition_id AS Condition_id
                  FROM items
                  WHERE name = @name",
                new { name });

        public List<ItemData> GetAllItems() =>
            GetAllCached(
                () => _db.Query<ItemData>(
                    @"SELECT
                        id AS Id,
                        name AS Name,
                        item_type AS Item_type,
                        category AS Category,
                        description AS Description,
                        price AS Price,
                        is_consumable AS Is_consumable,
                        usable_in_battle AS Usable_in_battle,
                        usable_in_field AS Usable_in_field,
                        effect_id AS Effect_id,
                        condition_id AS Condition_id
                      FROM items").ToList(),
                i => i.Id);

        public List<ItemData> GetByType(string itemType) =>
            _db.Query<ItemData>(
                @"SELECT
                    id AS Id,
                    name AS Name,
                    item_type AS Item_type,
                    category AS Category,
                    description AS Description,
                    price AS Price,
                    is_consumable AS Is_consumable,
                    usable_in_battle AS Usable_in_battle,
                    usable_in_field AS Usable_in_field,
                    effect_id AS Effect_id,
                    condition_id AS Condition_id
                  FROM items
                  WHERE item_type = @itemType",
                new { itemType }).ToList();
    }


    // =========================================================
    // held_items
    // =========================================================

    internal class HeldItemRepository : DbRepository<int, HeldItemData>
    {
        internal HeldItemRepository(IDbConnectionService db) : base(db) { }

        public HeldItemData? GetById(int id) =>
            GetCached(id, () => _db.QuerySingle<HeldItemData?>(
                @"SELECT
                    id AS Id,
                    item_id AS Item_id,
                    effect_id AS Effect_id,
                    condition_id AS Condition_id,
                    is_one_time_use AS Is_one_time_use,
                    trigger AS Trigger
                  FROM held_items
                  WHERE id = @id",
                new { id }));

        public HeldItemData? GetByItemId(int itemId) =>
            _db.QuerySingle<HeldItemData?>(
                @"SELECT
                    id AS Id,
                    item_id AS Item_id,
                    effect_id AS Effect_id,
                    condition_id AS Condition_id,
                    is_one_time_use AS Is_one_time_use,
                    trigger AS Trigger
                  FROM held_items
                  WHERE item_id = @itemId",
                new { itemId });

        public List<HeldItemData> GetAll() =>
            GetAllCached(
                () => _db.Query<HeldItemData>(
                    @"SELECT
                        id AS Id,
                        item_id AS Item_id,
                        effect_id AS Effect_id,
                        condition_id AS Condition_id,
                        is_one_time_use AS Is_one_time_use,
                        trigger AS Trigger
                      FROM held_items").ToList(),
                i => i.Id);
    }


    // =========================================================
    // keyitems
    // =========================================================

    internal class KeyItemRepository : DbRepository<int, KeyItemData>
    {
        internal KeyItemRepository(IDbConnectionService db) : base(db) { }

        public KeyItemData? GetById(int id) =>
            GetCached(id, () => _db.QuerySingle<KeyItemData?>(
                @"SELECT
                    id AS Id,
                    item_id AS Item_id,
                    usage_effect_id AS Usage_effect_id,
                    condition_id AS Condition_id,
                    registerable AS Registerable
                  FROM keyitems
                  WHERE id = @id",
                new { id }));

        public KeyItemData? GetByItemId(int itemId) =>
            _db.QuerySingle<KeyItemData?>(
                @"SELECT
                    id AS Id,
                    item_id AS Item_id,
                    usage_effect_id AS Usage_effect_id,
                    condition_id AS Condition_id,
                    registerable AS Registerable
                  FROM keyitems
                  WHERE item_id = @itemId",
                new { itemId });

        public List<KeyItemData> GetAll() =>
            GetAllCached(
                () => _db.Query<KeyItemData>(
                    @"SELECT
                        id AS Id,
                        item_id AS Item_id,
                        usage_effect_id AS Usage_effect_id,
                        condition_id AS Condition_id,
                        registerable AS Registerable
                      FROM keyitems").ToList(),
                i => i.Id);
    }


    // =========================================================
    // pokeballs
    // =========================================================

    internal class PokeballRepository : DbRepository<int, PokeballData>
    {
        internal PokeballRepository(IDbConnectionService db) : base(db) { }

        public PokeballData? GetById(int id) =>
            GetCached(id, () => _db.QuerySingle<PokeballData?>(
                @"SELECT
                    id AS Id,
                    item_id AS Item_id,
                    caught_effect_id AS Caught_effect_id,
                    condition_id AS Condition_id,
                    multiplier AS Multiplier,
                    ball_type AS Ball_type
                  FROM pokeballs
                  WHERE id = @id",
                new { id }));

        public PokeballData? GetByItemId(int itemId) =>
            _db.QuerySingle<PokeballData?>(
                @"SELECT
                    id AS Id,
                    item_id AS Item_id,
                    caught_effect_id AS Caught_effect_id,
                    condition_id AS Condition_id,
                    multiplier AS Multiplier,
                    ball_type AS Ball_type
                  FROM pokeballs
                  WHERE item_id = @itemId",
                new { itemId });

        public List<PokeballData> GetAll() =>
            GetAllCached(
                () => _db.Query<PokeballData>(
                    @"SELECT
                        id AS Id,
                        item_id AS Item_id,
                        caught_effect_id AS Caught_effect_id,
                        condition_id AS Condition_id,
                        multiplier AS Multiplier,
                        ball_type AS Ball_type
                      FROM pokeballs").ToList(),
                i => i.Id);
    }


    // =========================================================
    // tms_hms
    // =========================================================

    internal class TmHmRepository : DbRepository<int, TmHmData>
    {
        internal TmHmRepository(IDbConnectionService db) : base(db) { }

        public TmHmData? GetById(int id) =>
            GetCached(id, () => _db.QuerySingle<TmHmData?>(
                @"SELECT
                    id AS Id,
                    item_id AS Item_id,
                    move_id AS Move_id,
                    is_hm AS Is_hm,
                    machine_id AS Machine_id
                  FROM tms_hms
                  WHERE id = @id",
                new { id }));

        public TmHmData? GetByItemId(int itemId) =>
            _db.QuerySingle<TmHmData?>(
                @"SELECT
                    id AS Id,
                    item_id AS Item_id,
                    move_id AS Move_id,
                    is_hm AS Is_hm,
                    machine_id AS Machine_id
                  FROM tms_hms
                  WHERE item_id = @itemId",
                new { itemId });

        public List<TmHmData> GetAll() =>
            GetAllCached(
                () => _db.Query<TmHmData>(
                    @"SELECT
                        id AS Id,
                        item_id AS Item_id,
                        move_id AS Move_id,
                        is_hm AS Is_hm,
                        machine_id AS Machine_id
                      FROM tms_hms").ToList(),
                i => i.Id);
    }
}