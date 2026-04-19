using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Dialogue
{
    // ── Enums ────────────────────────────────────────────────────────────────

    public enum TriggerType
    {
        OnApproach,
        OnTalk,
        OnDefeat,
        OnVictory
    }

    public enum DialogueNodeType
    {
        Text,
        Choice,
        Event
    }

    public enum DialogueSetType          // was: dialogueSetType  (PascalCase)
    {
        MainStory,
        SideQuest,
        NpcInteraction                   // was: NPCInteraction
    }

    // ── Value objects ────────────────────────────────────────────────────────

    public class DialogueLine
    {
        public string Text { get; }

        public DialogueLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Dialogue text cannot be empty.", nameof(text));

            Text = text;
        }
    }

    // ── Graph edges ──────────────────────────────────────────────────────────

    public class DialogueEdge
    {
        public string ChoiceText { get; }
        public DialogueNode ToNode { get; }
        public ICondition<BattleState>? Condition { get; }

        public DialogueEdge(string choiceText, DialogueNode toNode,
                            ICondition<BattleState>? condition = null)
        {
            ChoiceText = choiceText ?? throw new ArgumentNullException(nameof(choiceText));
            ToNode = toNode ?? throw new ArgumentNullException(nameof(toNode));
            Condition = condition;
        }

        public bool IsAvailable(BattleState state) =>
            Condition is null || Condition.Check(state);
    }

    // ── Graph nodes ──────────────────────────────────────────────────────────

    public class DialogueNode
    {
        private readonly List<DialogueEdge> _outgoingEdges = new();

        public DialogueNodeType Type { get; }
        public DialogueLine Line { get; }
        public int SequenceIndex { get; }

        // Set by DialogueSet.AddNode — read-only from the outside.
        public DialogueSet? ParentSet { get; private set; }

        public IReadOnlyList<DialogueEdge> OutgoingEdges => _outgoingEdges;

        public DialogueNode(DialogueNodeType type, DialogueLine line, int sequenceIndex)
        {
            Type = type;
            Line = line ?? throw new ArgumentNullException(nameof(line));
            SequenceIndex = sequenceIndex;
        }

        /// <summary>Called exclusively by <see cref="DialogueSet.AddNode"/>.</summary>
        internal void AttachToSet(DialogueSet set) => ParentSet = set;

        public void AddEdge(DialogueEdge edge) =>
            _outgoingEdges.Add(edge ?? throw new ArgumentNullException(nameof(edge)));

        /// <summary>Edges whose condition passes for the given state.</summary>
        public IEnumerable<DialogueEdge> AvailableEdges(BattleState state) =>
            _outgoingEdges.Where(e => e.IsAvailable(state));
    }

    // ── Dialogue graph ───────────────────────────────────────────────────────

    public class DialogueSet
    {
        private readonly List<DialogueNode> _nodes = new();

        public DialogueSetType Type { get; }

        /// <summary>Convenience accessor — the first node added is the entry point.</summary>
        public DialogueNode? StartNode => _nodes.Count > 0 ? _nodes[0] : null;

        public IReadOnlyList<DialogueNode> Nodes => _nodes;

        public DialogueSet(DialogueSetType type) => Type = type;

        public void AddNode(DialogueNode node)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            node.AttachToSet(this);
            _nodes.Add(node);
        }
    }

    // ── NPC state binding ────────────────────────────────────────────────────

    public class NpcDialogueState
    {
        public TriggerType Trigger { get; }            // was: Type (ambiguous)
        public ICondition<BattleState>? Condition { get; }
        public DialogueSet DialogueSet { get; }

        public NpcDialogueState(TriggerType trigger, DialogueSet dialogueSet,
                                ICondition<BattleState>? condition = null)
        {
            Trigger = trigger;
            DialogueSet = dialogueSet ?? throw new ArgumentNullException(nameof(dialogueSet));
            Condition = condition;
        }

        public bool IsMatch(TriggerType trigger, BattleState state) =>
            Trigger == trigger && (Condition is null || Condition.Check(state));
    }

    // ── NPC ──────────────────────────────────────────────────────────────────
    
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
    public class PokemontradingNpcState : NpcDomain
    {
        public PokemonState offered { get; set; }
        public PokemonState requested { get; set; }
    }
    public class TrainerDomain
    {
        public string name;
        public BotLevel AiType;
        public int BaseMoney;
    }
    public class TrainerNpcState : NpcDomain
    {
        private readonly TrainerDomain _trainerInfo;
        public PokemonTeam Team { get; set; }
    }

}