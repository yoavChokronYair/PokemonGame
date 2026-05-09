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

        private bool _hasMore;
        public bool HasMore
        {
            get => _hasMore;
            private set
            {
                if (SetProperty(ref _hasMore, value))
                {
                    OnPropertyChanged(nameof(AreActionsUnlocked));
                    ((RelayCommand)NextCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public bool AreActionsUnlocked => !HasMore;

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
            if (!HasMore)
                return Task.CompletedTask;

            _waitTcs = new TaskCompletionSource<bool>();
            return _waitTcs.Task;
        }

        private void ShowNext()
        {
            if (_queue.Count == 0)
            {
                HasMore = false;
                _waitTcs?.TrySetResult(true);
                return;
            }

            var entry = _queue.Dequeue();
            ApplyEntry(entry);
            HasMore = _queue.Count > 0;

            if (!HasMore)
                _waitTcs?.TrySetResult(true);
        }

        private void ApplyEntry(BattleLogEntry entry)
        {
            CurrentMessage = entry.Message;
            PhaseLabel = FormatPhaseLabel(entry);
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