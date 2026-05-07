using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Battle;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    /// <summary>
    /// Owns the battle log display.
    /// Messages are queued and revealed one at a time when the player
    /// presses Next — exactly like the in-game message box.
    /// While the queue is non-empty, move buttons stay disabled.
    /// </summary>
    public class BattleLoggerViewModel : ViewModelBase
    {
        // ── Internal queue ────────────────────────────────────────────────────
        private readonly Queue<BattleLogEntry> _queue = new();

        // ── Displayed text ────────────────────────────────────────────────────
        private string _currentMessage = string.Empty;
        public string CurrentMessage
        {
            get => _currentMessage;
            private set => SetProperty(ref _currentMessage, value);
        }

        // ── Phase label (e.g. "Turn 3 · Action") shown as subtitle ───────────
        private string _phaseLabel = string.Empty;
        public string PhaseLabel
        {
            get => _phaseLabel;
            private set => SetProperty(ref _phaseLabel, value);
        }

        // ── True while there are still messages waiting ───────────────────────
        private bool _hasMore;
        public bool HasMore
        {
            get => _hasMore;
            private set
            {
                if (SetProperty(ref _hasMore, value))
                {
                    // Move buttons listen to !HasMore, so notify them too
                    OnPropertyChanged(nameof(AreActionsUnlocked));
                    ((RelayCommand)NextCommand).NotifyCanExecuteChanged();
                }
            }
        }

        /// <summary>True when the queue is empty — move buttons should bind to this.</summary>
        public bool AreActionsUnlocked => !HasMore;

        // ── Commands ──────────────────────────────────────────────────────────
        public ICommand NextCommand { get; }

        public BattleLoggerViewModel()
        {
            NextCommand = new RelayCommand(ShowNext, () => HasMore);
        }

        // ── Public API called by BattleViewModel ──────────────────────────────

        /// <summary>
        /// Enqueue every new entry that arrived since the last turn.
        /// BattleViewModel calls this after RunTurn / PlayerSwitch.
        /// </summary>
        public void EnqueueEntries(IEnumerable<BattleLogEntry> entries)
        {
            foreach (var entry in entries)
            {
                _queue.Enqueue(entry);
            }

            // Immediately show the first message so the box is never blank
            if (_queue.Count > 0 && !HasMore)
            {
                ShowNext();
            }
        }

        /// <summary>
        /// Skip straight to the end of the queue (used for setup messages
        /// that don't need player interaction).
        /// </summary>
        public void FlushSetupMessages()
        {
            while (_queue.Count > 0)
            {
                var entry = _queue.Dequeue();
                if (entry.Phase != BattleLogPhase.Setup)
                {
                    // Put non-setup messages back and stop
                    // (re-enqueue is easiest by rebuilding — setup is always first so this path is rare)
                    var remaining = new List<BattleLogEntry>(_queue) { };
                    _queue.Clear();
                    _queue.Enqueue(entry);
                    foreach (var r in remaining)
                    {
                        _queue.Enqueue(r);
                    }

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
            {
                return Task.CompletedTask;
            }

            _waitTcs = new TaskCompletionSource<bool>();

            return _waitTcs.Task;
        }

        // ── Private ───────────────────────────────────────────────────────────

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
            {
                _waitTcs?.TrySetResult(true);
            }
        }

        private void ApplyEntry(BattleLogEntry entry)
        {
            CurrentMessage = entry.Message;
            PhaseLabel = FormatPhaseLabel(entry);
        }

        private static string FormatPhaseLabel(BattleLogEntry entry)
        {
            string phase = entry.Phase switch
            {
                BattleLogPhase.Setup => "Start",
                BattleLogPhase.TurnStart => $"Turn {entry.Turn}",
                BattleLogPhase.Action => $"Turn {entry.Turn} · Action",
                BattleLogPhase.StatusEffect => $"Turn {entry.Turn} · Status",
                BattleLogPhase.Faint => $"Turn {entry.Turn} · Faint",
                BattleLogPhase.Switch => $"Turn {entry.Turn} · Switch",
                BattleLogPhase.Weather => $"Turn {entry.Turn} · Weather",
                BattleLogPhase.BattleEnd => "End",
                _ => string.Empty,
            };
            return phase;
        }
    }
}