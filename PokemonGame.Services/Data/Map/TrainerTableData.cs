using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.Map
{
    // ── TrainerData ───────────────────────────────────────────────────────────
    public class TrainerTableData
    {
        public int Id { get; set; }
        public int BaseMoney { get; set; }
        public int AiType { get; set; }
        public int TrainerClass { get; set; }
    }

    // ── NpcDefinitions ────────────────────────────────────────────────────────
    public class NpcDefinitionsData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NpcType { get; set; }
        public int? SpriteId { get; set; }
        public int? TrainerId { get; set; }
    }

    // ── NpcItemGiving ─────────────────────────────────────────────────────────
    public class NpcItemGivingData
    {
        public int Id { get; set; }
        public int NpcId { get; set; }
        public int ItemId { get; set; }
    }

    // ── NpcShopInventory ──────────────────────────────────────────────────────
    public class NpcShopInventoryData
    {
        public int NpcId { get; set; }
        public int ItemId { get; set; }
        public int SlotOrder { get; set; }
    }

    // ── NpcPokemonTrade ───────────────────────────────────────────────────────
    public class NpcPokemonTradeData
    {
        public int NpcId { get; set; }
        public int OfferedPokemonId { get; set; }
        public int RequestedPokedexId { get; set; }
    }

    // ── NpcGymLeader ──────────────────────────────────────────────────────────
    public class NpcGymLeaderData
    {
        public int NpcId { get; set; }
        public int BadgeId { get; set; }
        public int TmItemId { get; set; }
    }

    // ── NpcGauntlet ───────────────────────────────────────────────────────────
    public class NpcGauntletData
    {
        public int NpcId { get; set; }
        public int GauntletType { get; set; }
        public int ProgressionFlagId { get; set; }
    }

    // ── NpcGiovanni ───────────────────────────────────────────────────────────
    public class NpcGiovanniData
    {
        public int NpcId { get; set; }
        public int StoryFlagId { get; set; }
    }

    // ── NpcItemRewardTrainer ──────────────────────────────────────────────────
    public class NpcItemRewardTrainerData
    {
        public int NpcId { get; set; }
        public int RewardItemGivingId { get; set; }
    }

    // ── DialogueSets ──────────────────────────────────────────────────────────
    public class DialogueSetsData
    {
        public int Id { get; set; }
        public int NpcId { get; set; }
        public int SetType { get; set; }
        public int Trigger { get; set; }
    }

    // ── DialogueNodes ─────────────────────────────────────────────────────────
    public class DialogueNodesData
    {
        public int Id { get; set; }
        public int SetId { get; set; }
        public int NodeType { get; set; }
        public string LineText { get; set; }
        public int SequenceIndex { get; set; }
    }

    // ── DialogueEdges ─────────────────────────────────────────────────────────
    public class DialogueEdgesData
    {
        public int Id { get; set; }
        public int FromNodeId { get; set; }
        public int ToNodeId { get; set; }
        public string ChoiceText { get; set; }
        public string? ConditionType { get; set; }
        public int? ConditionIntValue { get; set; }
    }
}
