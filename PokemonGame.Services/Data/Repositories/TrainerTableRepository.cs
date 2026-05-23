using System;
using System.Collections.Generic;
using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Map;

namespace PokemonGame.Services.Data.Repositories
{
    // ─────────────────────────────────────────────────────────────
    // TrainerData
    // ─────────────────────────────────────────────────────────────
    internal class TrainerTableRepository : DbRepository<int, TrainerTableData>
    {
        internal TrainerTableRepository(IDbConnectionService db) : base(db) { }

        public TrainerTableData Load(int id) =>
            GetCached(id, () => _db.QuerySingle<TrainerTableData>(
                "SELECT * FROM TrainerData WHERE Id = @Id",
                new { Id = id }));

        public void Save(TrainerTableData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO TrainerData
                    (Id, BaseMoney, AiType, TrainerClass)
                VALUES
                    (@Id, @BaseMoney, @AiType, @TrainerClass)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // NpcDefinitions
    // ─────────────────────────────────────────────────────────────
    internal class NpcDefinitionsRepository : DbRepository<int, NpcDefinitionsData>
    {
        internal NpcDefinitionsRepository(IDbConnectionService db) : base(db) { }

        public NpcDefinitionsData Load(int id) =>
            GetCached(id, () => _db.QuerySingle<NpcDefinitionsData>(
                "SELECT * FROM NpcDefinitions WHERE Id = @Id",
                new { Id = id }));

        public void Save(NpcDefinitionsData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO NpcDefinitions
                    (Id, Name, NpcType, SpriteId, TrainerId)
                VALUES
                    (@Id, @Name, @NpcType, @SpriteId, @TrainerId)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // NpcItemGiving
    // ─────────────────────────────────────────────────────────────
    internal class NpcItemGivingRepository : DbRepository<int, NpcItemGivingData>
    {
        internal NpcItemGivingRepository(IDbConnectionService db) : base(db) { }

        public List<NpcItemGivingData> LoadAll(int npcId) =>
            GetAllCached(() =>
                _db.Query<NpcItemGivingData>(
                    "SELECT * FROM NpcItemGiving WHERE NpcId = @NpcId",
                    new { NpcId = npcId }),
                x => x.Id);

        public void Save(NpcItemGivingData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO NpcItemGiving
                    (Id, NpcId, ItemId)
                VALUES
                    (@Id, @NpcId, @ItemId)",
                data);

        public void SaveAll(IEnumerable<NpcItemGivingData> items)
        {
            foreach (var i in items)
                Save(i);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // NpcShopInventory (composite key encoded)
    // ─────────────────────────────────────────────────────────────
    internal class NpcShopInventoryRepository : DbRepository<int, NpcShopInventoryData>
    {
        internal NpcShopInventoryRepository(IDbConnectionService db) : base(db) { }

        private static int Key(int npcId, int itemId) => (npcId << 16) ^ itemId;

        public List<NpcShopInventoryData> LoadAll(int npcId) =>
            GetAllCached(() =>
                _db.Query<NpcShopInventoryData>(
                    "SELECT * FROM NpcShopInventory WHERE NpcId = @NpcId",
                    new { NpcId = npcId }),
                x => Key(x.NpcId, x.ItemId));

        public void Save(NpcShopInventoryData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO NpcShopInventory
                    (NpcId, ItemId, SlotOrder)
                VALUES
                    (@NpcId, @ItemId, @SlotOrder)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // NpcPokemonTrade (1:1 per NPC)
    // ─────────────────────────────────────────────────────────────
    internal class NpcPokemonTradeRepository : DbRepository<int, NpcPokemonTradeData>
    {
        internal NpcPokemonTradeRepository(IDbConnectionService db) : base(db) { }

        public NpcPokemonTradeData Load(int npcId) =>
            GetCached(npcId, () => _db.QuerySingle<NpcPokemonTradeData>(
                "SELECT * FROM NpcPokemonTrade WHERE NpcId = @NpcId",
                new { NpcId = npcId }));

        public void Save(NpcPokemonTradeData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO NpcPokemonTrade
                    (NpcId, OfferedPokemonId, RequestedPokedexId)
                VALUES
                    (@NpcId, @OfferedPokemonId, @RequestedPokedexId)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // NpcGymLeader
    // ─────────────────────────────────────────────────────────────
    internal class NpcGymLeaderRepository : DbRepository<int, NpcGymLeaderData>
    {
        internal NpcGymLeaderRepository(IDbConnectionService db) : base(db) { }

        public NpcGymLeaderData Load(int npcId) =>
            GetCached(npcId, () => _db.QuerySingle<NpcGymLeaderData>(
                "SELECT * FROM NpcGymLeader WHERE NpcId = @NpcId",
                new { NpcId = npcId }));

        public void Save(NpcGymLeaderData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO NpcGymLeader
                    (NpcId, BadgeId, TmItemId)
                VALUES
                    (@NpcId, @BadgeId, @TmItemId)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // NpcGauntlet
    // ─────────────────────────────────────────────────────────────
    internal class NpcGauntletRepository : DbRepository<int, NpcGauntletData>
    {
        internal NpcGauntletRepository(IDbConnectionService db) : base(db) { }

        public NpcGauntletData Load(int npcId) =>
            GetCached(npcId, () => _db.QuerySingle<NpcGauntletData>(
                "SELECT * FROM NpcGauntlet WHERE NpcId = @NpcId",
                new { NpcId = npcId }));

        public void Save(NpcGauntletData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO NpcGauntlet
                    (NpcId, GauntletType, ProgressionFlagId)
                VALUES
                    (@NpcId, @GauntletType, @ProgressionFlagId)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // NpcGiovanni
    // ─────────────────────────────────────────────────────────────
    internal class NpcGiovanniRepository : DbRepository<int, NpcGiovanniData>
    {
        internal NpcGiovanniRepository(IDbConnectionService db) : base(db) { }

        public NpcGiovanniData Load(int npcId) =>
            GetCached(npcId, () => _db.QuerySingle<NpcGiovanniData>(
                "SELECT * FROM NpcGiovanni WHERE NpcId = @NpcId",
                new { NpcId = npcId }));

        public void Save(NpcGiovanniData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO NpcGiovanni
                    (NpcId, StoryFlagId)
                VALUES
                    (@NpcId, @StoryFlagId)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // NpcItemRewardTrainer
    // ─────────────────────────────────────────────────────────────
    internal class NpcItemRewardTrainerRepository : DbRepository<int, NpcItemRewardTrainerData>
    {
        internal NpcItemRewardTrainerRepository(IDbConnectionService db) : base(db) { }

        public NpcItemRewardTrainerData Load(int npcId) =>
            GetCached(npcId, () => _db.QuerySingle<NpcItemRewardTrainerData>(
                "SELECT * FROM NpcItemRewardTrainer WHERE NpcId = @NpcId",
                new { NpcId = npcId }));

        public void Save(NpcItemRewardTrainerData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO NpcItemRewardTrainer
                    (NpcId, RewardItemGivingId)
                VALUES
                    (@NpcId, @RewardItemGivingId)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // DialogueSets
    // ─────────────────────────────────────────────────────────────
    internal class DialogueSetsRepository : DbRepository<int, DialogueSetsData>
    {
        internal DialogueSetsRepository(IDbConnectionService db) : base(db) { }

        public List<DialogueSetsData> LoadByNpc(int npcId) =>
            GetAllCached(() =>
                _db.Query<DialogueSetsData>(
                    "SELECT * FROM DialogueSets WHERE NpcId = @NpcId",
                    new { NpcId = npcId }),
                x => x.Id);

        public void Save(DialogueSetsData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO DialogueSets
                    (Id, NpcId, SetType, Trigger)
                VALUES
                    (@Id, @NpcId, @SetType, @Trigger)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // DialogueNodes
    // ─────────────────────────────────────────────────────────────
    internal class DialogueNodesRepository : DbRepository<int, DialogueNodesData>
    {
        internal DialogueNodesRepository(IDbConnectionService db) : base(db) { }

        public List<DialogueNodesData> LoadBySet(int setId) =>
            GetAllCached(() =>
                _db.Query<DialogueNodesData>(
                    "SELECT * FROM DialogueNodes WHERE SetId = @SetId ORDER BY SequenceIndex",
                    new { SetId = setId }),
                x => x.Id);

        public void Save(DialogueNodesData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO DialogueNodes
                    (Id, SetId, NodeType, LineText, SequenceIndex)
                VALUES
                    (@Id, @SetId, @NodeType, @LineText, @SequenceIndex)",
                data);
    }

    // ─────────────────────────────────────────────────────────────
    // DialogueEdges
    // ─────────────────────────────────────────────────────────────
    internal class DialogueEdgesRepository : DbRepository<int, DialogueEdgesData>
    {
        internal DialogueEdgesRepository(IDbConnectionService db) : base(db) { }

        public List<DialogueEdgesData> LoadByFromNode(int nodeId) =>
            GetAllCached(() =>
                _db.Query<DialogueEdgesData>(
                    "SELECT * FROM DialogueEdges WHERE FromNodeId = @FromNodeId",
                    new { FromNodeId = nodeId }),
                x => x.Id);

        public void Save(DialogueEdgesData data) =>
            _db.Execute(@"
                INSERT OR REPLACE INTO DialogueEdges
                    (Id, FromNodeId, ToNodeId, ChoiceText, ConditionType, ConditionIntValue)
                VALUES
                    (@Id, @FromNodeId, @ToNodeId, @ChoiceText, @ConditionType, @ConditionIntValue)",
                data);
    }
}