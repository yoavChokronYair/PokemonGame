using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Dialogue;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Npc
{
    public class NpcDomain
    {

        private readonly List<NpcDialogueState> _dialogueStates = new();

        public IReadOnlyList<NpcDialogueState> DialogueStates => _dialogueStates;

        public void AddDialogueState(NpcDialogueState state) =>
            _dialogueStates.Add(state ?? throw new ArgumentNullException(nameof(state)));

        /// <summary>
        /// Returns the first <see cref="DialogueSet"/> whose trigger and condition
        /// match the current game state, or <c>null</c> if none apply.
        /// </summary>
        public DialogueSet? GetDialogue(TriggerType trigger, BattleState state) =>
            _dialogueStates.FirstOrDefault(d => d.IsMatch(trigger, state))?.DialogueSet;
        public NpcType? Type;
        public string? Name;
        public int Id;

    }
    public class NpcRewardDomain
    {
        public TriggerType TriggerType { get; }         // reuses your existing enum
        public RewardType RewardType { get; }
        public int RewardValue { get; }                 // what item
        public bool IsRepeatable { get; }
        public ICondition<BattleState>? Condition { get; }

        private bool _hasBeenClaimed;

        public NpcRewardDomain(
            TriggerType triggerType,
            RewardType rewardType,
            int rewardValue,
            bool isRepeatable,
            ICondition<BattleState>? condition = null)
        {
            TriggerType = triggerType;
            RewardType = rewardType;
            RewardValue = rewardValue;
            IsRepeatable = isRepeatable;
            Condition = condition;
        }

        /// <summary>
        /// Whether this reward can be granted right now.
        /// Accounts for repeatability and the optional condition.
        /// </summary>
        public bool IsAvailable(BattleState state) =>
            (IsRepeatable || !_hasBeenClaimed) &&
            (Condition is null || Condition.Check(state));

        /// <summary>
        /// Marks the reward as claimed. No-op if already claimed and non-repeatable.
        /// </summary>
        public void Claim()
        {
            if (!IsRepeatable && _hasBeenClaimed)
                throw new InvalidOperationException(
                    $"Reward is not repeatable and has already been claimed.");

            _hasBeenClaimed = true;
        }
    }
    public class ItemGivingDomain
    {
        private readonly itemsDomain _item;
        public ICondition<BattleState>? Condition { get; }

        private bool _hasBeenGiven;


        /// <summary>Items can only be given once (no repeatable flag in the schema).</summary>
        public bool IsAvailable(BattleState state) =>
            !_hasBeenGiven &&
            (Condition is null || Condition.Check(state));


    }
    public class TrainerDomain
    {
                public BotLevel AiType;
        public int BaseMoney;
    }
}
