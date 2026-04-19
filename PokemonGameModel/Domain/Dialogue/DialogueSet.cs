using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Dialogue
{
    public enum DialogueNodeType
    {
        Text,
        Choice,
        Event
    }
    public enum dialogueSetType
    {
        MainStory,
        SideQuest,
        NPCInteraction
    }
    public class DialogueSet
    {
        public dialogueSetType Type { get; set; }

        private readonly List<DialogueNode> _nodes = new();
        public IReadOnlyList<DialogueNode> Nodes => _nodes;

        public void AddNode(DialogueNode node)
        {
            node.SetParent(this);
            _nodes.Add(node);
        }
    }
    public class DialogueNode
    {
        private readonly List<DialogueEdge> outgoingEdges = new();
        public DialogueSet ParentSet { get; set; }
        public DialogueLine Line { get; set; }
        public DialogueNodeType Type { get; set; } // Text / Choice / Event
        public int SequenceIndex { get; set; }
        public IReadOnlyList<DialogueEdge> OutgoingEdges => outgoingEdges;
        internal void SetParent(DialogueSet set)
        {
            ParentSet = set;
        }

        public void AddEdge(DialogueEdge edge)
        {
            outgoingEdges.Add(edge);
        }

    }
    public class DialogueLine
    {
        public string Text { get; set; }
    }
    public class DialogueEdge
    {
        public ICondition<BattleState> Condition { get; set; }
        public string ChoiceText { get; set; }
        public DialogueNode ToNode { get; set; }
        public bool IsAvailable(BattleState state)
        {
            return Condition == null || Condition.Check(state);
        }
    }
}
