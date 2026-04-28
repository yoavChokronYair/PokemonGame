using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Dialogue
{
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

        // null = always passable (choice edge), non-null = evaluated by runner
        public ICondition<PlayerDomain>? Condition { get; }

        // Choice edge (no condition)
        public DialogueEdge(string choiceText, DialogueNode toNode)
        {
            ChoiceText = choiceText ?? throw new ArgumentNullException(nameof(choiceText));
            ToNode = toNode ?? throw new ArgumentNullException(nameof(toNode));
        }

        // Condition edge
        public DialogueEdge(string choiceText, DialogueNode toNode, ICondition<PlayerDomain> condition)
            : this(choiceText, toNode)
        {
            Condition = condition;
        }

        public bool CanTraverse(PlayerDomain player) =>
            Condition is null || Condition.Check(player);
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
        public IEnumerable<DialogueEdge> AvailableEdges() => _outgoingEdges;

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
        public IEnumerable<DialogueEdge> GetValidEdges(PlayerDomain player) =>
            _outgoingEdges.Where(e => e.CanTraverse(player));
        /// <summary>Edges whose condition passes for the given state.</summary>

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
        public TriggerType Trigger { get; }
        public DialogueSet DialogueSet { get; }

        public NpcDialogueState(TriggerType trigger, DialogueSet dialogueSet)
        {
            Trigger = trigger;
            DialogueSet = dialogueSet ?? throw new ArgumentNullException(nameof(dialogueSet));
        }

        public bool IsMatch(TriggerType trigger) => Trigger == trigger;
    }
}