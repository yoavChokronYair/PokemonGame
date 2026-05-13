using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Repositories
{
    internal class TrainerInfoRepository : DbRepository<int, TrainerInfoData>
    {
        internal TrainerInfoRepository(IDbConnectionService db) : base(db) { }

        public TrainerInfoData Load(int playerID) =>
            GetCached(playerID, () => _db.QuerySingle<TrainerInfoData>(
                "SELECT * FROM TrainerInfo WHERE PlayerID = @PlayerID",
                new { PlayerID = playerID }));

        public void Save(TrainerInfoData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO TrainerInfo
                    (PlayerID, Id, TrainerID, Name, Money, TimePlayed, Gender, HallOfFameDebut,
                     FacingDirection, CurrentMap, LastMapVisited, PlayerLocX, PlayerLocY,
                     IsSurfing, HasRunningShoes)
                VALUES
                    (@PlayerID, @Id, @TrainerID, @Name, @Money, @TimePlayed, @Gender, @HallOfFameDebut,
                     @FacingDirection, @CurrentMap, @LastMapVisited, @PlayerLocX, @PlayerLocY,
                     @IsSurfing, @HasRunningShoes)",
                new
                {
                    data.PlayerID,
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

        public List<BadgeData> LoadAll(int playerID) =>
            GetAllCached(() =>
                _db.Query<BadgeData>(
                    "SELECT * FROM Badges WHERE PlayerID = @PlayerID",
                    new { PlayerID = playerID }),
                b => b.Id);

        public void Save(BadgeData badge)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO Badges (PlayerID, Id, IsObtained) VALUES (@PlayerID, @Id, @IsObtained)",
                new { badge.PlayerID, badge.Id, badge.IsObtained });
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

        public List<int> LoadAll(int playerID) =>
            _db.QueryScalarList<int>(
                "SELECT FlagId FROM StoryFlags WHERE PlayerID = @PlayerID",
                new { PlayerID = playerID });

        public void Add(int playerID, int flagId) =>
            _db.Execute(
                "INSERT OR IGNORE INTO StoryFlags (PlayerID, FlagId) VALUES (@PlayerID, @FlagId)",
                new { PlayerID = playerID, FlagId = flagId });

        public void Remove(int playerID, int flagId) =>
            _db.Execute(
                "DELETE FROM StoryFlags WHERE PlayerID = @PlayerID AND FlagId = @FlagId",
                new { PlayerID = playerID, FlagId = flagId });
    }

    internal class DefeatedTrainerRepository : DbRepository<int, DefeatedTrainerData>
    {
        internal DefeatedTrainerRepository(IDbConnectionService db) : base(db) { }

        public List<int> LoadAll(int playerID) =>
            _db.QueryScalarList<int>(
                "SELECT TrainerId FROM DefeatedTrainers WHERE PlayerID = @PlayerID",
                new { PlayerID = playerID });

        public void Add(int playerID, int trainerId) =>
            _db.Execute(
                "INSERT OR IGNORE INTO DefeatedTrainers (PlayerID, TrainerId) VALUES (@PlayerID, @TrainerId)",
                new { PlayerID = playerID, TrainerId = trainerId });
    }

    internal class ItemTakenRepository : DbRepository<int, ItemTakenData>
    {
        internal ItemTakenRepository(IDbConnectionService db) : base(db) { }

        public List<int> LoadAll(int playerID) =>
            _db.QueryScalarList<int>(
                "SELECT NpcId FROM ItemsTaken WHERE PlayerID = @PlayerID",
                new { PlayerID = playerID });

        public void Add(int playerID, int npcId) =>
            _db.Execute(
                "INSERT OR IGNORE INTO ItemsTaken (PlayerID, NpcId) VALUES (@PlayerID, @NpcId)",
                new { PlayerID = playerID, NpcId = npcId });
    }

    internal class TradedPokemonRepository : DbRepository<int, TradedPokemonData>
    {
        internal TradedPokemonRepository(IDbConnectionService db) : base(db) { }

        public List<int> LoadAll(int playerID) =>
            _db.QueryScalarList<int>(
                "SELECT PokedexId FROM TradedPokemon WHERE PlayerID = @PlayerID",
                new { PlayerID = playerID });

        public void Add(int playerID, int pokedexId) =>
            _db.Execute(
                "INSERT OR IGNORE INTO TradedPokemon (PlayerID, PokedexId) VALUES (@PlayerID, @PokedexId)",
                new { PlayerID = playerID, PokedexId = pokedexId });
    }

    internal class BagInventoryRepository : DbRepository<int, BagInventoryData>
    {
        internal BagInventoryRepository(IDbConnectionService db) : base(db) { }

        public List<BagInventoryData> LoadAll(int playerID) =>
            GetAllCached(() =>
                _db.Query<BagInventoryData>(
                    "SELECT * FROM BagInventory WHERE PlayerID = @PlayerID",
                    new { PlayerID = playerID }),
                b => b.ItemId);

        public void Save(BagInventoryData item)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO BagInventory (PlayerID, ItemId, Quantity) VALUES (@PlayerID, @ItemId, @Quantity)",
                new { item.PlayerID, item.ItemId, item.Quantity });
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

        public List<PokedexData> LoadAll(int playerID) =>
            GetAllCached(() =>
                _db.Query<PokedexData>(
                    "SELECT * FROM Pokedex WHERE PlayerID = @PlayerID",
                    new { PlayerID = playerID }),
                p => p.PokedexId);

        public void Save(PokedexData entry)
        {
            _db.Execute(
                "INSERT OR REPLACE INTO Pokedex (PlayerID, PokedexId, Seen, Caught) VALUES (@PlayerID, @PokedexId, @Seen, @Caught)",
                new { entry.PlayerID, entry.PokedexId, entry.Seen, entry.Caught });
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

        public List<PartyData> LoadAll(int playerID) =>
            GetAllCached(() =>
                _db.Query<PartyData>(
                    "SELECT * FROM Party WHERE PlayerID = @PlayerID ORDER BY Slot",
                    new { PlayerID = playerID }),
                p => p.Slot);

        public void Save(PartyData slot)
        {
            _db.Execute(@"
                INSERT OR REPLACE INTO Party
                    (PlayerID, Slot, PokedexId, Nickname, Level, CurrentHP, Experience, StatusId, IsShiny)
                VALUES
                    (@PlayerID, @Slot, @PokedexId, @Nickname, @Level, @CurrentHP, @Experience, @StatusId, @IsShiny)",
                new
                {
                    slot.PlayerID,
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

        public void Clear(int playerID) =>
            _db.Execute(
                "DELETE FROM Party WHERE PlayerID = @PlayerID",
                new { PlayerID = playerID });
    }

    internal class StoryPlayerRepository : DbRepository<int, StoryPlayerData>
    {
        internal StoryPlayerRepository(IDbConnectionService db) : base(db) { }

        public List<StoryPlayerData> LoadAll() =>
            GetAllCached(() =>
                _db.Query<StoryPlayerData>("SELECT * FROM StoryPlayer"),
                p => p.PlayerID);

        public void Save(StoryPlayerData player) =>
            _db.Execute(
                "INSERT OR REPLACE INTO StoryPlayer (UserID) VALUES (@UserID)",
                new { player.UserID });

        public void SaveAll(IEnumerable<StoryPlayerData> players)
        {
            foreach (var player in players)
                Save(player);
        }

        public StoryPlayerData? GetPlayerUserId(int userId) =>
            _db.QuerySingle<StoryPlayerData>(
                "SELECT * FROM StoryPlayer WHERE UserID = @UserID",
                new { UserID = userId });

        public List<StoryPlayerData> GetPlayersUserId(int userId) =>
            _db.Query<StoryPlayerData>(
                "SELECT * FROM StoryPlayer WHERE UserID = @UserID",
                new { UserID = userId });
    }
}