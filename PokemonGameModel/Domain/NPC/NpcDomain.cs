using PokemonGame.Model.Domain.Dialogue;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Player;
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
        public virtual void OnDialogueFinishedTrue()
        {

        }
        public virtual void OnDialogueFinishedFalse()
        {

        }

        public NpcType? Type;
        public string? Name;    
        public int Id;
    }
    public class ItemGivingDomain
    {
        private readonly itemsDomain _item;
        public int id;
        private bool _hasBeenGiven;

        public bool IsAvailable() => !_hasBeenGiven;

        public void Give()
        {
            if (_hasBeenGiven)
                throw new InvalidOperationException(
                    "Item has already been given.");

            _hasBeenGiven = true;
            if(!PlayerDomain.Instance.BagInventory.TryGetValue(_item, out int currentCount))
            {
                PlayerDomain.Instance.BagInventory[_item] = 0;
            }
            PlayerDomain.Instance.BagInventory[_item]++;
        }
    }

    public class TrainerDomain
    {
        public int id { get; set; }
        public BotLevel AiType;
        public int BaseMoney;
        public TrainerClass TrainerClass;
    }
}