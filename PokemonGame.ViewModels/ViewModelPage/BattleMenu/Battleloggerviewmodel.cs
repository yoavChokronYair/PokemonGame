using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Battle;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class BattleLoggerViewModel : ViewModelBase
    {
        private readonly Queue<BattleLogEntry> _queue = new();

        private string _currentMessage = string.Empty;
        public string CurrentMessage
        {
            get => _currentMessage;
            private set => SetProperty(ref _currentMessage, value);
        }

        private string _phaseLabel = string.Empty;
        public string PhaseLabel
        {
            get => _phaseLabel;
            private set => SetProperty(ref _phaseLabel, value);
        }
        private bool _isTypingAnimation;

        public bool IsTexting => _isTypingAnimation || HasMore;


        private bool _hasMore;
        public bool HasMore
        {
            get => _hasMore;
            set
            {
                if (SetProperty(ref _hasMore, value))
                {
                    OnPropertyChanged(nameof(AreActionsUnlocked));
                    OnPropertyChanged(nameof(IsTexting));
                }
            }
        }
        private bool _isTyping;

        public bool IsTyping
        {
            get => _isTyping;
            set
            {
                if (SetProperty(ref _isTyping, value))
                {
                    OnPropertyChanged(nameof(AreActionsUnlocked));
                }
            }
        }

        public bool AreActionsUnlocked => !HasMore && !IsTyping;
        public ICommand NextCommand { get; }

        public BattleLoggerViewModel()
        {
            NextCommand = new RelayCommand(ShowNext, () => HasMore);
        }

        public void EnqueueEntries(IEnumerable<BattleLogEntry> entries)
        {
            foreach (var entry in entries)
                _queue.Enqueue(entry);

            if (_queue.Count > 0 && !HasMore)
                ShowNext();
        }

        public void EnqueueStringEntries(IEnumerable<string> messages, BattleLogPhase phase = BattleLogPhase.Action, int turn = 0)
        {
            var entries = messages.Select(msg => new BattleLogEntry(phase, turn, msg));
            EnqueueEntries(entries);
        }

        public void FlushSetupMessages()
        {
            while (_queue.Count > 0)
            {
                var entry = _queue.Dequeue();
                if (entry.Phase != BattleLogPhase.Setup)
                {
                    var remaining = new List<BattleLogEntry>(_queue);
                    _queue.Clear();
                    _queue.Enqueue(entry);
                    foreach (var r in remaining)
                        _queue.Enqueue(r);
                    break;
                }
                ApplyEntry(entry);
            }
            HasMore = _queue.Count > 0;
        }

        private TaskCompletionSource<bool>? _waitTcs;

        public Task WaitUntilQueueEmpty()
        {
            if (!HasMore && !IsTyping)
                return Task.CompletedTask;

            _waitTcs = new TaskCompletionSource<bool>();
            return _waitTcs.Task;
        }

        public void ShowNext()
        {
            if (IsTyping)
                return;
            if (_queue.Count == 0)
            {
                HasMore = false;
                _waitTcs?.TrySetResult(true);
                return;
            }

            var entry = _queue.Dequeue();
            ApplyEntry(entry);
            HasMore = _queue.Count > 0;
            ((RelayCommand)NextCommand).NotifyCanExecuteChanged();
            if (!HasMore)
                _waitTcs?.TrySetResult(true);
        }

        private async void ApplyEntry(BattleLogEntry entry)
        {
            PhaseLabel = FormatPhaseLabel(entry);
            IsTyping = true;
            CurrentMessage = "";
            _isTypingAnimation = true;
            OnPropertyChanged(nameof(IsTexting));

            foreach (char c in entry.Message)
            {
                CurrentMessage += c;
                await Task.Delay(18);
            }

            _isTypingAnimation = false;
            OnPropertyChanged(nameof(IsTexting));
            IsTyping = false;

            ((RelayCommand)NextCommand).NotifyCanExecuteChanged();

            // Only complete here, after typing is done AND queue is empty
            if (!HasMore)
                _waitTcs?.TrySetResult(true);
        }

        private static string FormatPhaseLabel(BattleLogEntry entry) =>
            entry.Phase switch
            {
                BattleLogPhase.Setup => "Start",
                BattleLogPhase.TurnStart => $"Turn {entry.Turn}",
                BattleLogPhase.Action => $"Turn {entry.Turn} · Action",
                BattleLogPhase.StatusEffect => $"Turn {entry.Turn} · Status",
                BattleLogPhase.Faint => $"Turn {entry.Turn} · Faint",
                BattleLogPhase.Switch => $"Turn {entry.Turn} · Switch",
                BattleLogPhase.Weather => $"Turn {entry.Turn} · Weather",
                BattleLogPhase.BattleEnd => "End",
                _ => string.Empty
            };
    }
}