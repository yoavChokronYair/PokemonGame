using System.Runtime.CompilerServices;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;

namespace PokemonGame.Model.Domain.Item
{
    // ─────────────────────────────────────────
    //  Enum
    // ─────────────────────────────────────────

    public enum ItemType
    {
        HeldItem,
        Consumable,
        Tm,
        Hm,
        Pokeball,
        KeyItem
    }

    // ─────────────────────────────────────────
    //  Base item
    // ─────────────────────────────────────────

    public class ItemsDomain
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ItemType Type { get; set; }

        public IEffect Effect { get; set; } = new NoEffect();

        public string Description { get; set; } = string.Empty;

        public bool UsableInBattle { get; set; }

        public bool UsableInField { get; set; }

        public int Price { get; set; } = 0;

        public bool IsFullyLoaded { get; set; } = true;

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        public bool SameDatabaseItemAs(ItemsDomain other)
        {
            return other != null && Id > 0 && Id == other.Id;
        }
    }

    // ─────────────────────────────────────────
    //  Key item
    // ─────────────────────────────────────────

    public class KeyItemState : ItemsDomain
    {
        public IEffect UsageEffect { get; set; } = new NoEffect();

        public ICondition<PlayerDomain>? FieldCondition { get; set; }

        public bool Registerable { get; set; }

        public KeyItemState(
            IEffect? usageEffect = null,
            ICondition<PlayerDomain>? fieldCondition = null,
            bool registerable = false)
        {
            UsageEffect = usageEffect ?? new NoEffect();
            Effect = UsageEffect;

            FieldCondition = fieldCondition;
            Registerable = registerable;

            Type = ItemType.KeyItem;
            UsableInBattle = false;
            UsableInField = true;
        }

        public bool CanUseInField(PlayerDomain player)
        {
            return FieldCondition?.Check(player) ?? true;
        }

        public void UseInField(PlayerDomain player)
        {
            if (!CanUseInField(player))
                return;

           // UsageEffect.Apply(player);
        }
    }

    // ─────────────────────────────────────────
    //  Held item
    // ─────────────────────────────────────────

    public class HeldItemState : ItemsDomain, IHeldItem
    {
        /// <summary>
        /// Maps to held_items.conditionid — the battle-state
        /// predicate that must be true before the effect fires.
        /// </summary>
        private readonly ICondition<BattleState> _condition;

        /// <summary>
        /// Maps to held_items.isOneTimeUse — consumed on use
        /// (e.g. Sitrus Berry) vs persistent (e.g. Choice Band).
        /// </summary>
        public bool IsOneTimeUse { get; set; }

        /// <summary>
        /// Maps to held_items.trigger — which battle event
        /// causes this item to activate.
        /// </summary>
        public BattleEventTrigger Trigger { get; set; }

        public HeldItemState(string name,
                             ICondition<BattleState> condition,
                             IEffect effect,
                             BattleEventTrigger trigger,
                             bool isConsumable = false,
                             string description = "")
        {
            Name = name;
            _condition = condition;
            Effect = effect;
            Trigger = trigger;
            IsOneTimeUse = isConsumable;
            Description = description;
            Type = ItemType.HeldItem;
            UsableInBattle = false; // held items activate automatically
            UsableInField = false;
        }

        public void Apply(BattleState battle)
        {
            if (_condition.Check(battle))
                Effect.Apply(battle);
        }
    }

    // ─────────────────────────────────────────
    //  Poké Ball
    // ─────────────────────────────────────────

    public class PokeballState : ItemsDomain
    {
        /// <summary>
        /// Maps to pokeballs.caughteffectid — the effect applied
        /// to a Pokémon that was caught in this ball
        /// (e.g. Heal Ball restores HP, Friend Ball sets friendship).
        /// </summary>
        public IEffect CaughtEffect { get; set; } = new NoEffect();

        /// <summary>
        /// Maps to pokeballs.conditionid — extra catch condition
        /// (e.g. Net Ball only boosts on Water/Bug types).
        /// </summary>
        private readonly ICondition<BattleState> _condition;
        public PokeBallType BallType { get; set; }

        /// <summary>
        /// Maps to pokeballs.multiplier — catch-rate multiplier
        /// (1.0 = Poké Ball, 1.5 = Great Ball, 2.0 = Ultra Ball…).
        /// </summary>
        public float Multiplier { get; set; }

        public PokeballState(
            string name,
            IEffect caughtEffect,
            ICondition<BattleState> condition,
            float multiplier = 1f,
            string description = "",
            PokeBallType ballType = PokeBallType.PokeBall)
        {
            Name = name;
            CaughtEffect = caughtEffect;
            _condition = condition;
            Multiplier = multiplier;
            Description = description;
            BallType = ballType;

            Type = ItemType.Pokeball;
            UsableInBattle = true;
            UsableInField = false;
        }

        /// <summary>
        /// Returns the effective catch multiplier, taking the
        /// optional condition into account.
        /// </summary>
        public float GetEffectiveMultiplier(BattleState battle) =>
            (_condition?.Check(battle) ?? true) ? Multiplier : 1f;

        /// <summary>
        /// Applies the on-caught effect to the newly caught Pokémon.
        /// </summary>
        public void ApplyCaughtEffect(BattleState battle)
        {
            CaughtEffect.Apply(battle);
        }
    }

    // ─────────────────────────────────────────
    //  TM / HM
    // ─────────────────────────────────────────

    public class TmHmState : ItemsDomain
    {
        public MoveState Move { get; set; }

        public bool IsHm { get; set; }

        public bool IsReusable { get; set; }

        public bool WasUsed { get; private set; }

        public TmHmState(
            string name,
            MoveState move,
            bool isHm = false,
            bool isReusable = false,
            string description = "")
        {
            Name = name;
            Move = move;
            IsHm = isHm;
            IsReusable = isReusable || isHm;
            Description = description;

            Type = isHm ? ItemType.Hm : ItemType.Tm;

            UsableInBattle = false;
            UsableInField = true;
        }

        public bool CanTeach()
        {
            return IsReusable || !WasUsed;
        }

        public void MarkUsed()
        {
            if (!IsReusable)
                WasUsed = true;
        }
    }
}