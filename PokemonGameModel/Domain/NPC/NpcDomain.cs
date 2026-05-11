using PokemonGame.Model.Domain.Dialogue;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Npc
{
    public class NpcSpriteDomain
    {
        public Dictionary<FacingDirection, (int TL, int TR, int BL, int BR)> Tiles { get; set; } = new();

        public (int TL, int TR, int BL, int BR)? GetSprite(FacingDirection direction)
            => Tiles.TryGetValue(direction, out var sprite) ? sprite : null;
    }
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
            if(!PlayerDomain.Instance.trainerItemDomain.BagInventory.TryGetValue(_item, out int currentCount))
            {
                PlayerDomain.Instance.trainerItemDomain.BagInventory[_item] = 0;
            }
            PlayerDomain.Instance.trainerItemDomain.BagInventory[_item]++;
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