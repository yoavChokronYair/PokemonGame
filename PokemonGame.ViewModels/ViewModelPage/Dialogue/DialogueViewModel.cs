using PokemonGame.Model.Domain.Dialogue;
using PokemonGame.Model.Enums;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.Map.Command;

namespace PokemonGame.ViewModels.ViewModelPage.Dialogue
{
    public class DialogueViewModel : ViewModelBase
    {
        // ---------------------------------------------------------------
        // State
        // ---------------------------------------------------------------
        private DialogueNode? _currentNode;
        private string _npcName = string.Empty;

        private bool _isOpen;
        private string _speakerName = string.Empty;
        private string _text = string.Empty;
        private bool _hasChoices;
        private List<DialogueChoiceViewModel> _choices = new();
        public event Action? FocusRequested;
        public event Action? DialogueOpened;
        public event Action? DialogueClosed;


        // ---------------------------------------------------------------
        // Properties
        // ---------------------------------------------------------------
        public bool IsOpen
        {
            get => _isOpen;
            private set => SetProperty(ref _isOpen, value);
        }

        public string SpeakerName
        {
            get => _speakerName;
            private set => SetProperty(ref _speakerName, value);
        }

        public string Text
        {
            get => _text;
            private set => SetProperty(ref _text, value);
        }

        public bool HasChoices
        {
            get => _hasChoices;
            private set => SetProperty(ref _hasChoices, value);
        }

        public List<DialogueChoiceViewModel> Choices
        {
            get => _choices;
            private set => SetProperty(ref _choices, value);
        }

        // UI binds this to swap "Next" → "Close" on the last line
        public bool IsLastLine =>
            _currentNode != null &&
            !HasChoices &&
            !_currentNode.AvailableEdges().Any();

        // ---------------------------------------------------------------
        // Commands
        // ---------------------------------------------------------------
        public AdvanceDialogueCommand AdvanceCommand { get; }

        // ---------------------------------------------------------------
        // Construction
        // ---------------------------------------------------------------
        public DialogueViewModel()
        {
            AdvanceCommand = new AdvanceDialogueCommand(this);
        }

        // ---------------------------------------------------------------
        // Open
        // ---------------------------------------------------------------
        public void Open(DialogueSet set, string npcName)
        {
            if (set.StartNode == null) return;

            _npcName = npcName;
            IsOpen = true;
            ShowNode(set.StartNode);
            DialogueOpened?.Invoke();
        }

        // ---------------------------------------------------------------
        // Advance — confirm/interact key with no choices
        // ---------------------------------------------------------------
        public void Advance()
        {
            if (!IsOpen || _currentNode == null) return;
            if (HasChoices) return;

            var edges = _currentNode.AvailableEdges().ToList();

            if (edges.Count == 0)
            {
                Close();
                return;
            }

            // Single edge with empty ChoiceText = auto-advance
            ShowNode(edges[0].ToNode);
            FocusRequested?.Invoke();

        }

        // ---------------------------------------------------------------
        // PickChoice — player selects a branch option
        // ---------------------------------------------------------------
        public void PickChoice(int choiceIndex)
        {
            if (!IsOpen || _currentNode == null) return;

            var edges = _currentNode.AvailableEdges().ToList();
            if (choiceIndex < 0 || choiceIndex >= edges.Count) return;

            ShowNode(edges[choiceIndex].ToNode);
            FocusRequested?.Invoke();

        }

        // ---------------------------------------------------------------
        // Close
        // ---------------------------------------------------------------
        public void Close()
        {
            IsOpen = false;
            _currentNode = null;
            _npcName = string.Empty;
            SpeakerName = string.Empty;
            Text = string.Empty;
            HasChoices = false;
            Choices = new();
            OnPropertyChanged(nameof(IsLastLine));
            FocusRequested?.Invoke();
            DialogueClosed?.Invoke();
        }

        // ---------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------
        private void ShowNode(DialogueNode node)
        {
            _currentNode = node;
            SpeakerName = _npcName;
            Text = node.Line.Text;

            var edges = node.AvailableEdges().ToList();

            // Multiple edges OR single edge with a label = show choice buttons
            // Single edge with empty label = auto-advance, no buttons needed
            HasChoices = edges.Count > 1
                || (edges.Count == 1 && !string.IsNullOrEmpty(edges[0].ChoiceText));

            Choices = HasChoices
                ? edges.Select((e, i) => new DialogueChoiceViewModel(e.ChoiceText, i, this)).ToList()
                : new();

            OnPropertyChanged(nameof(IsLastLine));
        }
    }

    // ---------------------------------------------------------------
    // Per-choice VM
    // ---------------------------------------------------------------
    public class DialogueChoiceViewModel
    {
        private readonly int _index;
        private readonly DialogueViewModel _owner;

        public string Label { get; }

        public DialogueChoiceViewModel(string label, int index, DialogueViewModel owner)
        {
            Label = label;
            _index = index;
            _owner = owner;
        }

        public void Pick() => _owner.PickChoice(_index);
    }
}