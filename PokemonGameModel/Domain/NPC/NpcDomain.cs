using PokemonGame.Model.Domain.Dialogue;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Npc
{
    public class NpcDomain
    {
        private readonly List<NpcDialogueState> _dialogueStates = new();

        public IReadOnlyList<NpcDialogueState> DialogueStates => _dialogueStates;

        public void AddDialogueState(NpcDialogueState state) =>
            _dialogueStates.Add(state ?? throw new ArgumentNullException(nameof(state)));

        public DialogueSet? GetDialogue(TriggerType trigger) =>
            _dialogueStates.FirstOrDefault(d => d.IsMatch(trigger))?.DialogueSet;

        public NpcType? Type;
        public string? Name;
        public int Id;
    }

    public class NpcRewardDomain
    {
        public TriggerType TriggerType { get; }
        public RewardType RewardType { get; }
        public int RewardValue { get; }
        public bool IsRepeatable { get; }

        private bool _hasBeenClaimed;

        public NpcRewardDomain(
            TriggerType triggerType,
            RewardType rewardType,
            int rewardValue,
            bool isRepeatable)
        {
            TriggerType = triggerType;
            RewardType = rewardType;
            RewardValue = rewardValue;
            IsRepeatable = isRepeatable;
        }

        public bool IsAvailable() => IsRepeatable || !_hasBeenClaimed;

        public void Claim()
        {
            if (!IsRepeatable && _hasBeenClaimed)
                throw new InvalidOperationException(
                    "Reward is not repeatable and has already been claimed.");

            _hasBeenClaimed = true;
        }
    }

    public class ItemGivingDomain
    {
        private readonly itemsDomain _item;
        private bool _hasBeenGiven;

        public bool IsAvailable() => !_hasBeenGiven;

        public void Give()
        {
            if (_hasBeenGiven)
                throw new InvalidOperationException(
                    "Item has already been given.");

            _hasBeenGiven = true;
        }
    }

    public class TrainerDomain
    {
        public BotLevel AiType;
        public int BaseMoney;
    }
}