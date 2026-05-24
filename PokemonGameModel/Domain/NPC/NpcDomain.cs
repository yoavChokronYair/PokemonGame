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

        public int Id { get; set; }

        public string? Name { get; set; }

        public NpcType? Type { get; set; }

        public int? SpriteId { get; set; }

        public void AddDialogueState(NpcDialogueState state)
        {
            _dialogueStates.Add(
                state ?? throw new ArgumentNullException(nameof(state)));
        }

        public DialogueSet? GetDialogue(TriggerType trigger)
        {
            return _dialogueStates
                .FirstOrDefault(d => d.IsMatch(trigger))
                ?.DialogueSet;
        }

        public virtual void OnDialogueFinishedTrue()
        {
        }

        public virtual void OnDialogueFinishedFalse()
        {
        }
    }

    public class ItemGivingDomain
    {
        private readonly ItemsDomain _item;

        private bool _hasBeenGiven;

        public int Id { get; set; }

        public ItemGivingDomain(ItemsDomain item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public bool IsAvailable()
        {
            return !_hasBeenGiven;
        }

        public void Give()
        {
            if (_hasBeenGiven)
            {
                throw new InvalidOperationException(
                    "Item has already been given.");
            }

            _hasBeenGiven = true;

            if (!PlayerDomain.Instance.trainerItemDomain.BagInventory
                    .TryGetValue(_item, out int currentCount))
            {
                PlayerDomain.Instance.trainerItemDomain.BagInventory[_item] = 0;
            }

            PlayerDomain.Instance.trainerItemDomain.BagInventory[_item]++;
        }
    }

    public class TrainerDomain
    {
        public int Id { get; set; }

        public BotLevel AiType { get; set; }

        public int BaseMoney { get; set; }

        public TrainerClass TrainerClass { get; set; }
    }
}