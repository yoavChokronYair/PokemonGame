using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Repositories
{
    internal class TrainerInfoRepository : DbRepository<int, TrainerInfoData>
    {
        internal TrainerInfoRepository(IDbConnectionService db) : base(db) { }

        public TrainerInfoData Load() =>
            GetCached(1, () => _db.QuerySingle<TrainerInfoData>(
                "SELECT * FROM TrainerInfo WHERE Id = 1"));

        public void Save(TrainerInfoData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO TrainerInfo 
                    (Id, TrainerID, Name, Money, TimePlayed, Gender, HallOfFameDebut,
                     FacingDirection, CurrentMap, LastMapVisited, PlayerLocX, PlayerLocY,
                     IsSurfing, HasRunningShoes)
                VALUES 
                    (@Id, @TrainerID, @Name, @Money, @TimePlayed, @Gender, @HallOfFameDebut,
                     @FacingDirection, @CurrentMap, @LastMapVisited, @PlayerLocX, @PlayerLocY,
                     @IsSurfing, @HasRunningShoes)",
                new
                {
                    data.Id,
                    data.TrainerID,
                    data.Name,
                    data.Money,
                    data.TimePlayed,
                    data.Gender,
                    data.HallOfFameDebut,
                    data.FacingDirection,
                    data.CurrentMap,
                    data.LastMapVisited,
                    data.PlayerLocX,
                    data.PlayerLocY,
                    data.IsSurfing,
                    data.HasRunningShoes
                });
    }

    internal class BadgeRepository : DbRepository<int, BadgeData>
    {
        internal BadgeRepository(IDbConnectionService db) : base(db) { }

        public List<BadgeData> LoadAll() =>
            GetAllCached(() => _db.Query<BadgeData>("SELECT * FROM Badges"), b => b.Id);

        public void Save(BadgeData badge)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO Badges (Id, IsObtained) VALUES (@Id, @IsObtained)",
                new { badge.Id, badge.IsObtained });
            StoreAndReturn(badge.Id, () => badge);
        }

        public void SaveAll(IEnumerable<BadgeData> badges)
        {
            foreach (var badge in badges)
                Save(badge);
        }
    }

    internal class StoryFlagRepository : DbRepository<int, StoryFlagData>
    {
        internal StoryFlagRepository(IDbConnectionService db) : base(db) { }

        public List<int> LoadAll() =>
            _db.QueryScalarList<int>("SELECT FlagId FROM StoryFlags");

        public void Add(int flagId) =>
            _db.Execute("INSERT OR IGNORE INTO StoryFlags (FlagId) VALUES (@FlagId)",
                new { FlagId = flagId });

        public void Remove(int flagId) =>
            _db.Execute("DELETE FROM StoryFlags WHERE FlagId = @FlagId",
                new { FlagId = flagId });
    }

    internal class DefeatedTrainerRepository : DbRepository<int, DefeatedTrainerData>
    {
        internal DefeatedTrainerRepository(IDbConnectionService db) : base(db) { }

        public List<int> LoadAll() =>
            _db.QueryScalarList<int>("SELECT TrainerId FROM DefeatedTrainers");

        public void Add(int trainerId) =>
            _db.Execute("INSERT OR IGNORE INTO DefeatedTrainers (TrainerId) VALUES (@TrainerId)",
                new { TrainerId = trainerId });
    }

    internal class ItemTakenRepository : DbRepository<int, ItemTakenData>
    {
        internal ItemTakenRepository(IDbConnectionService db) : base(db) { }

        public List<int> LoadAll() =>
            _db.QueryScalarList<int>("SELECT NpcId FROM ItemsTaken");

        public void Add(int npcId) =>
            _db.Execute("INSERT OR IGNORE INTO ItemsTaken (NpcId) VALUES (@NpcId)",
                new { NpcId = npcId });
    }

    internal class TradedPokemonRepository : DbRepository<int, TradedPokemonData>
    {
        internal TradedPokemonRepository(IDbConnectionService db) : base(db) { }

        public List<int> LoadAll() =>
            _db.QueryScalarList<int>("SELECT PokedexId FROM TradedPokemon");

        public void Add(int pokedexId) =>
            _db.Execute("INSERT OR IGNORE INTO TradedPokemon (PokedexId) VALUES (@PokedexId)",
                new { PokedexId = pokedexId });
    }

    internal class BagInventoryRepository : DbRepository<int, BagInventoryData>
    {
        internal BagInventoryRepository(IDbConnectionService db) : base(db) { }

        public List<BagInventoryData> LoadAll() =>
            GetAllCached(() => _db.Query<BagInventoryData>("SELECT * FROM BagInventory"),
                b => b.ItemId);

        public void Save(BagInventoryData item)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO BagInventory (ItemId, Quantity) VALUES (@ItemId, @Quantity)",
                new { item.ItemId, item.Quantity });
            StoreAndReturn(item.ItemId, () => item);
        }

        public void SaveAll(IEnumerable<BagInventoryData> items)
        {
            foreach (var item in items)
                Save(item);
        }
    }

    internal class PokedexRepository : DbRepository<int, PokedexData>
    {
        internal PokedexRepository(IDbConnectionService db) : base(db) { }

        public List<PokedexData> LoadAll() =>
            GetAllCached(() => _db.Query<PokedexData>("SELECT * FROM Pokedex"), p => p.PokedexId);

        public void Save(PokedexData entry)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO Pokedex (PokedexId, Seen, Caught) VALUES (@PokedexId, @Seen, @Caught)",
                new { entry.PokedexId, entry.Seen, entry.Caught });
            StoreAndReturn(entry.PokedexId, () => entry);
        }

        public void SaveAll(IEnumerable<PokedexData> entries)
        {
            foreach (var entry in entries)
                Save(entry);
        }
    }

    internal class PartyRepository : DbRepository<int, PartyData>
    {
        internal PartyRepository(IDbConnectionService db) : base(db) { }

        public List<PartyData> LoadAll() =>
            GetAllCached(() => _db.Query<PartyData>("SELECT * FROM Party ORDER BY Slot"),
                p => p.Slot);

        public void Save(PartyData slot)
        {
            _db.Execute(@"
                INSERT OR REPLACE INTO Party 
                    (Slot, PokedexId, Nickname, Level, CurrentHP, Experience, StatusId, IsShiny)
                VALUES 
                    (@Slot, @PokedexId, @Nickname, @Level, @CurrentHP, @Experience, @StatusId, @IsShiny)",
                new
                {
                    slot.Slot,
                    slot.PokedexId,
                    slot.Nickname,
                    slot.Level,
                    slot.CurrentHP,
                    slot.Experience,
                    slot.StatusId,
                    slot.IsShiny
                });
            StoreAndReturn(slot.Slot, () => slot);
        }

        public void SaveAll(IEnumerable<PartyData> slots)
        {
            foreach (var slot in slots)
                Save(slot);
        }

        public void Clear() =>
            _db.Execute("DELETE FROM Party");
    }
    internal class StoryPlayerRepository : DbRepository<int, StoryPlayerData>
    {
        internal StoryPlayerRepository(IDbConnectionService db) : base(db) { }

        public List<StoryPlayerData> LoadAll() =>
            GetAllCached(() => _db.Query<StoryPlayerData>("SELECT * FROM StoryPlayer"), p => p.PlayerID);

        public void Save(StoryPlayerData player)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO StoryPlayer (UserID) VALUES (@UserID)",
                new { player.UserID });
        }

        public void SaveAll(IEnumerable<StoryPlayerData> players)
        {
            foreach (var player in players)
                Save(player);
        }
        public StoryPlayerData? GetPlayerUserId(int userId) =>
            _db.QuerySingle<StoryPlayerData>("SELECT * FROM StoryPlayer WHERE UserID = @UserID",
            new { UserID = userId });
        public List<StoryPlayerData> GetPlayersUserId(int userId) =>
            _db.Query<StoryPlayerData>("SELECT * FROM StoryPlayer WHERE UserID = @UserID",
        new { UserID = userId });
    }
}
