using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Item
{
    // ─────────────────────────────────────────
    //  Enum
    // ─────────────────────────────────────────

    public enum ItemType
    {
        HeldItem,
        Consumable,
        Hm,
        Pokeball,
        KeyItem
    }

    // ─────────────────────────────────────────
    //  Base item
    // ─────────────────────────────────────────

    public class itemsDomain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public IEffect Effect { get; set; }
        public string Description { get; set; }
        public bool UsableInBattle { get; set; }
        public bool UsableInField { get; set; }
        public int Price { get; set; } = 0;

        public override bool Equals(object? obj) =>
            obj is itemsDomain other && Id == other.Id;

        public override int GetHashCode() => Id.GetHashCode();
    }

    // ─────────────────────────────────────────
    //  Key item
    // ─────────────────────────────────────────

    public class KeyItemState : itemsDomain
    {
        /// <summary>
        /// Maps to keyitems.usageid — the effect triggered when
        /// the player uses this item from the bag / registered slot.
        /// </summary>
        public IEffect UsageEffect { get; set; }

        /// <summary>
        /// Maps to keyitems.conditionid — optional gate on when
        /// the item can be used (e.g. only near a specific tile).
        /// </summary>
        private readonly ICondition<BattleState> _condition;

        /// <summary>
        /// Maps to keyitems.registerable — can the player
        /// hotkey this item to the Y / shortcut button.
        /// </summary>
        public bool Registerable { get; set; }

        public KeyItemState(IEffect usageEffect,
                            ICondition<BattleState> condition,
                            bool registerable = false)
        {
            UsageEffect = usageEffect;
            _condition = condition;
            Registerable = registerable;
            Type = ItemType.KeyItem;
            UsableInBattle = false;
            UsableInField = true;
        }

        public bool CanUse(BattleState context) =>
            _condition?.Check(context) ?? true;
    }

    // ─────────────────────────────────────────
    //  Held item
    // ─────────────────────────────────────────

    public class HeldItemState : itemsDomain, IHeldItem
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

    public class PokeballState : itemsDomain
    {
        /// <summary>
        /// Maps to pokeballs.caughteffectid — the effect applied
        /// to a Pokémon that was caught in this ball
        /// (e.g. Heal Ball restores HP, Friend Ball sets friendship).
        /// </summary>
        public IEffect CaughtEffect { get; set; }

        /// <summary>
        /// Maps to pokeballs.conditionid — extra catch condition
        /// (e.g. Net Ball only boosts on Water/Bug types).
        /// </summary>
        private readonly ICondition<BattleState> _condition;

        /// <summary>
        /// Maps to pokeballs.multiplier — catch-rate multiplier
        /// (1.0 = Poké Ball, 1.5 = Great Ball, 2.0 = Ultra Ball…).
        /// </summary>
        public float Multiplier { get; set; }

        public PokeballState(string name,
                             IEffect caughtEffect,
                             ICondition<BattleState> condition,
                             float multiplier = 1f,
                             string description = "")
        {
            Name = name;
            CaughtEffect = caughtEffect;
            _condition = condition;
            Multiplier = multiplier;
            Description = description;
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
        public void ApplyCaughtEffect(BattleState battle) =>
            CaughtEffect?.Apply(battle);
    }

    // ─────────────────────────────────────────
    //  TM / HM
    // ─────────────────────────────────────────

    public class TmHmState : itemsDomain
    {
        /// <summary>
        /// Maps to tms_hms.moveid — the move this disc teaches.
        /// </summary>
        public MoveState Move { get; set; }

        /// <summary>
        /// Maps to tms_hms.isHm — HMs cannot be discarded and
        /// their moves cannot be forgotten without the Move Deleter.
        /// </summary>
        public bool IsHm { get; set; }

        public TmHmState(string name,
                         MoveState move,
                         bool isHm = false,
                         string description = "")
        {
            Name = name;
            Move = move;
            IsHm = isHm;
            Description = description;
            Type = isHm ? ItemType.Hm : ItemType.Consumable;
            UsableInBattle = false;
            UsableInField = true;
        }

        /// <summary>
        /// Returns false if a TM has already been used and single-use
        /// semantics are in effect (Gen 1-4 behaviour).
        /// Always returns true for HMs and reusable TMs (Gen 5+).
        /// </summary>
        public bool CanTeach(bool alreadyUsed) =>
            IsHm || !alreadyUsed;
    }
}