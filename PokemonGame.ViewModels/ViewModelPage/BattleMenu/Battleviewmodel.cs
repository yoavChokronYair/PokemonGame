using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Managers;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    // ── Root page VM — set as DataContext on PokemonBattlePage ───────────────
    public class BattleViewModel : ViewModelBase
    {
        private readonly BattleManager _manager;

        public PokemonBattleStatusViewModel PlayerStatus { get; }
        public EnemyBattleStatusViewModel EnemyStatus { get; }
        public BattleMenuViewModel BattleMenu { get; }
        public BattleLoggerViewModel Logger { get; }

        private int _logCursor = 0;

        public string? WinnerName => _manager.Winner?.Active.Name;

        public BattleViewModel(UserStore playerUserStore)
        {
            var session = playerUserStore.BattleSesion;

            var playerTeam = session.ResolvedPlayerTeam
                ?? throw new InvalidOperationException("ResolvedPlayerTeam was not set before navigating to battle.");

            var botTeam = session.ResolvedBotTeam
                ?? throw new InvalidOperationException("ResolvedBotTeam was not set before navigating to battle.");

            var botLevel = ResolveBotLevel(session.BotDifficulty);

            _manager = new BattleManager(playerTeam, botTeam, botLevel);

            Logger = new BattleLoggerViewModel();
            PlayerStatus = new PokemonBattleStatusViewModel();
            EnemyStatus = new EnemyBattleStatusViewModel();
            BattleMenu = new BattleMenuViewModel(OnMoveChosen, OnSwitchChosen, _manager, Logger);

            SyncAll(flushSetup: true);
        }

        private void OnMoveChosen(int moveIndex)
        {
            _manager.RunTurn(moveIndex);
            SyncAll();
        }

        private void OnSwitchChosen(int slotIndex)
        {
            _manager.RunTurn(slotIndex, BattleAction.Switch);
            SyncAll();
        }

        private void SyncAll(bool flushSetup = false)
        {
            var p = _manager.PlayerActive;
            PlayerStatus.PokedexId = p.PokedexId;
            PlayerStatus.PokemonName = p.Name;
            PlayerStatus.Level = p.Level;
            PlayerStatus.CurrentHP = p.CurrentHP;
            PlayerStatus.MaxHP = p.MaxHP;
            PlayerStatus.StatusCondition = p.Status.ToString();

            var e = _manager.BotActive;
            EnemyStatus.PokedexId = e.PokedexId;
            EnemyStatus.PokemonName = e.Name;
            EnemyStatus.Level = e.Level;
            EnemyStatus.CurrentHP = e.CurrentHP;
            EnemyStatus.MaxHP = e.MaxHP;
            EnemyStatus.StatusCondition = e.Status.ToString();

            BattleMenu.RefreshMoves(_manager.PlayerActive.Moves);

            var allEntries = _manager.logger.Entries;
            if (allEntries.Count > _logCursor)
            {
                var newEntries = allEntries.Skip(_logCursor).ToList();
                _logCursor = allEntries.Count;
                Logger.EnqueueEntries(newEntries);

                if (flushSetup)
                    Logger.FlushSetupMessages();
            }

            OnPropertyChanged(nameof(WinnerName));
        }

        private static BotLevel ResolveBotLevel(BotDifficulty difficulty) =>
            difficulty switch
            {
                BotDifficulty.Easy => BotLevel.Easy,
                BotDifficulty.Medium => BotLevel.Medium,
                BotDifficulty.Hard => BotLevel.Hard,
                _ => BotLevel.Easy
            };
    }


    // ── BattlePokemonMovesetChooserViewModel ──────────────────────────────────
    public class BattlePokemonMovesetChooserViewModel : ViewModelBase
    {
        private readonly Action<int> _onMoveClicked;
        private readonly Action<IMove?> _onMoveHovered;

        public MoveSlotViewModel Move0 { get; }
        public MoveSlotViewModel Move1 { get; }
        public MoveSlotViewModel Move2 { get; }
        public MoveSlotViewModel Move3 { get; }

        public BattlePokemonMovesetChooserViewModel(
            Action<int> onMoveClicked,
            Action<IMove?> onMoveHovered,
            BattleLoggerViewModel logger)
        {
            _onMoveClicked = onMoveClicked;
            _onMoveHovered = onMoveHovered;

            Move0 = new MoveSlotViewModel(0, onMoveClicked, onMoveHovered, logger);
            Move1 = new MoveSlotViewModel(1, onMoveClicked, onMoveHovered, logger);
            Move2 = new MoveSlotViewModel(2, onMoveClicked, onMoveHovered, logger);
            Move3 = new MoveSlotViewModel(3, onMoveClicked, onMoveHovered, logger);
        }

        public void LoadMoves(IReadOnlyList<IMove> moves)
        {
            var slots = new[] { Move0, Move1, Move2, Move3 };
            for (int i = 0; i < 4; i++)
            {
                if (i < moves.Count)
                {
                    slots[i].SetMove(moves[i]);
                }
                else
                {
                    slots[i].Clear();
                }
            }
        }
    }

    // ── MoveSlotViewModel ─────────────────────────────────────────────────────
    public class MoveSlotViewModel : ViewModelBase
    {
        private readonly int _index;
        private readonly Action<int> _onClick;
        private readonly Action<IMove?> _onHover;
        private readonly BattleLoggerViewModel _logger;

        private string _moveName = "-";
        private bool _hasMove = false;   // true when a real move is loaded
        private IMove? _move;

        public string MoveName
        {
            get => _moveName;
            private set => SetProperty(ref _moveName, value);
        }

        /// <summary>
        /// IsEnabled = move slot has a move AND the log queue is empty.
        /// Bound to IsEnabled on the button in XAML.
        /// </summary>
        public bool IsEnabled => _hasMove && _logger.AreActionsUnlocked;

        public ICommand ClickCommand { get; }
        public ICommand HoverCommand { get; }
        public ICommand LeaveCommand { get; }

        public MoveSlotViewModel(
            int index,
            Action<int> onClick,
            Action<IMove?> onHover,
            BattleLoggerViewModel logger)
        {
            _index = index;
            _onClick = onClick;
            _onHover = onHover;
            _logger = logger;

            ClickCommand = new RelayCommand(
                () => _onClick(_index),
                () => IsEnabled);

            HoverCommand = new RelayCommand(() => _onHover(_move));
            LeaveCommand = new RelayCommand(() => _onHover(null));

            // Re-evaluate CanExecute whenever the queue drains or refills
            _logger.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BattleLoggerViewModel.AreActionsUnlocked))
                {
                    OnPropertyChanged(nameof(IsEnabled));
                    ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
                }
            };
        }

        public void SetMove(IMove move)
        {
            _move = move;
            MoveName = (((_move as MoveState)))?.Name ?? "-";
            _hasMove = true;
            OnPropertyChanged(nameof(IsEnabled));
            ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
        }

        public void Clear()
        {
            _move = null;
            MoveName = "-";
            _hasMove = false;
            OnPropertyChanged(nameof(IsEnabled));
            ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
        }
    }

}