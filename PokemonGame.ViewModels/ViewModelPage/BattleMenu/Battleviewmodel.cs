using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Interfaces;
using PokemonGame.Services.Services;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class BattleViewModel : ViewModelBase
    {
        // ─────────────────────────────────────────────────────────────
        // Online / Offline state
        // ─────────────────────────────────────────────────────────────

        private readonly IBattleService? _service;
        private readonly BattleManager? _manager;
        private readonly bool _isOnline;

        private readonly NavigationStore _navigationStore;
        private readonly IDialogService _dialogService;
        private readonly Func<ViewModelBase> _createGameModeChooserViewModel;
        private readonly UserStore _playerUserStore;

        private const int STARTING_ELO = 1525;

        public PokemonBattleStatusViewModel PlayerStatus { get; set; }
        public EnemyBattleStatusViewModel EnemyStatus { get; set; }
        public BattleMenuViewModel BattleMenu { get; set; }
        public BattleLoggerViewModel Logger { get; set; }

        private int _logCursor = 0;
        private bool _isBattleOverHandled = false;

        // ─────────────────────────────────────────────────────────────
        // Result / ranking UI
        // ─────────────────────────────────────────────────────────────

        public double ProgressBarWidth =>
            RatingMax > 0
                ? MathHelper.Clamp((double)RatingCurrent / RatingMax * 200.0, 0, 200)
                : 0;

        private string _winnerText = "";
        public string WinnerText
        {
            get => _winnerText;
            set => SetProperty(ref _winnerText, value);
        }

        private string _resultMethod = "";
        public string ResultMethod
        {
            get => _resultMethod;
            set => SetProperty(ref _resultMethod, value);
        }

        private string _rankName = "";
        public string RankName
        {
            get => _rankName;
            set => SetProperty(ref _rankName, value);
        }

        private int _rankDelta;
        public int RankDelta
        {
            get => _rankDelta;
            set
            {
                if (SetProperty(ref _rankDelta, value))
                {
                    OnPropertyChanged(nameof(RankDeltaText));
                    OnPropertyChanged(nameof(IsPositiveDelta));
                }
            }
        }

        public string RankDeltaText => RankDelta >= 0 ? $"+{RankDelta}" : $"{RankDelta}";
        public bool IsPositiveDelta => RankDelta >= 0;

        private int _ratingCurrent;
        public int RatingCurrent
        {
            get => _ratingCurrent;
            set
            {
                if (SetProperty(ref _ratingCurrent, value))
                {
                    OnPropertyChanged(nameof(RatingText));
                    OnPropertyChanged(nameof(ProgressBarWidth));
                }
            }
        }

        private int _ratingMax = 100;
        public int RatingMax
        {
            get => _ratingMax;
            set
            {
                if (SetProperty(ref _ratingMax, value))
                {
                    OnPropertyChanged(nameof(RatingText));
                    OnPropertyChanged(nameof(ProgressBarWidth));
                }
            }
        }

        public string RatingText => $"{RatingCurrent}/{RatingMax}";

        public ICommand NewGameCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand RematchCommand { get; }

        public event EventHandler<BattleResultAction> CloseRequested;

        public string? WinnerName
        {
            get
            {
                if (_isOnline)
                {
                    if (_service == null || !_service.HasInitialState)
                        return null;

                    var snap = _service.GetState();

                    return snap.IsOver ? snap.WinnerName : null;
                }

                return _manager?.Winner?.Active.Name;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        public BattleViewModel(
            UserStore playerUserStore,
            NavigationStore navigationStore,
            IDialogService dialogService,
            Func<ViewModelBase> createGameModeChooserViewModel)
        {
            _navigationStore = navigationStore;
            _dialogService = dialogService;
            _createGameModeChooserViewModel = createGameModeChooserViewModel;
            _playerUserStore = playerUserStore;

            _isOnline = playerUserStore.Resolver.IsOnline
                        && playerUserStore.BattleService is not null;

            if (_isOnline)
            {
                _service = playerUserStore.BattleService!;

                _service.OnStateUpdated += () =>
                    System.Windows.Application.Current.Dispatcher.Invoke(SyncAll);

                _service.OnError += ex =>
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        Logger.EnqueueStringEntries(new[]
                        {
                            $"Online error: {ex.Message}"
                        });
                    });
            }
            else
            {
                var session = playerUserStore.BattleSesion;

                var playerTeam = session.ResolvedPlayerTeam
                    ?? throw new InvalidOperationException("Player team not set.");

                var botTeam = session.ResolvedBotTeam
                    ?? throw new InvalidOperationException("Bot team not set.");

                _manager = new BattleManager(
                    playerTeam,
                    botTeam,
                    ResolveBotLevel(session.BotDifficulty));
            }

            Logger = new BattleLoggerViewModel();
            PlayerStatus = new PokemonBattleStatusViewModel();
            EnemyStatus = new EnemyBattleStatusViewModel();

            BattleMenu = new BattleMenuViewModel(
                OnMoveChosen,
                OnSwitchChosen,
                OnForfeit,
                OnOpenBag,
                OnOpenSwitch,
                Logger);

            NewGameCommand = new RelayCommand(() =>
                CloseRequested?.Invoke(this, BattleResultAction.NewGame));

            BackCommand = new RelayCommand(() =>
                CloseRequested?.Invoke(this, BattleResultAction.Back));

            RematchCommand = new RelayCommand(() =>
                CloseRequested?.Invoke(this, BattleResultAction.Rematch));

            OnPropertyChanged(nameof(Logger));
            OnPropertyChanged(nameof(PlayerStatus));
            OnPropertyChanged(nameof(EnemyStatus));
            OnPropertyChanged(nameof(BattleMenu));

            SyncAll(flushSetup: true);
        }

        // ─────────────────────────────────────────────────────────────
        // Battle menu actions
        // ─────────────────────────────────────────────────────────────

        private void OnOpenBag()
        {
            Logger.EnqueueStringEntries(new[]
            {
                "You cannot use the Bag in this battle."
            });
        }

        private async Task OnMoveChosen(int moveIndex)
        {
            if (_isOnline)
            {
                await RunOnlineMoveAsync(moveIndex);
                return;
            }

            await RunOfflineMoveAsync(moveIndex);
        }

        private async void OnSwitchChosen(int slotIndex)
        {
            if (_isOnline)
            {
                await RunOnlineSwitchAsync(slotIndex);
                return;
            }

            await RunOfflineSwitchAsync(slotIndex);
        }

        private async void OnForfeit()
        {
            if (_isOnline)
            {
                await RunOnlineForfeitAsync();
                return;
            }

            RunOfflineForfeit();
        }

        private void OnOpenSwitch()
        {
            _navigationStore.CurrentViewModel = new TeamSelectionViewModel(
                _playerUserStore,
                _navigationStore,
                () => this,
                new TeamSelectionOptions
                {
                    CanSwitch = true,
                    CanMove = false,
                    CanSummary = true,
                    AutoConfirmSelection = false,
                    IsUsingUserStore = true
                },
                OnSwitchChosen,
                true);
        }

        // ─────────────────────────────────────────────────────────────
        // Online flow
        // ─────────────────────────────────────────────────────────────

        private async Task RunOnlineMoveAsync(int moveIndex)
        {
            if (_service == null)
                return;

            try
            {
                BattleMenu.SetWaitingForOpponent(true);

                await _service.RunTurnAsync(
                    moveIndex,
                    OnlineBattleActionTypes.Move);
            }
            catch (Exception ex)
            {
                BattleMenu.SetWaitingForOpponent(false);

                Logger.EnqueueStringEntries(new[]
                {
                    $"Online action failed: {ex.Message}"
                });
            }
        }

        private async Task RunOnlineSwitchAsync(int slotIndex)
        {
            if (_service == null)
                return;

            try
            {
                BattleMenu.SetWaitingForOpponent(true);

                await _service.RunTurnAsync(
                    slotIndex,
                    OnlineBattleActionTypes.Switch);
            }
            catch (Exception ex)
            {
                BattleMenu.SetWaitingForOpponent(false);

                Logger.EnqueueStringEntries(new[]
                {
                    $"Online switch failed: {ex.Message}"
                });
            }
        }

        private async Task RunOnlineForfeitAsync()
        {
            if (_service == null)
                return;

            try
            {
                await _service.ForfeitAsync();
            }
            catch (Exception ex)
            {
                Logger.EnqueueStringEntries(new[]
                {
                    $"Forfeit failed: {ex.Message}"
                });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Offline flow
        // ─────────────────────────────────────────────────────────────

        private async Task RunOfflineMoveAsync(int moveIndex)
        {
            if (_manager == null)
                return;

            _manager.RunTurn(moveIndex);

            FlushNewDomainLogs();

            await Logger.WaitUntilQueueEmpty();

            await AnimateFaintResultsAsync();

            SyncAll();

            await AskForFreeSwitchIfEnemyFaintedAsync();
        }

        private async Task RunOfflineSwitchAsync(int slotIndex)
        {
            if (_manager == null)
                return;

            _manager.RunTurn(slotIndex, BattleActionType.Switch);

            FlushNewDomainLogs();

            await Logger.WaitUntilQueueEmpty();

            SyncPlayerPokemon();
            SyncEnemyPokemon();
            SyncBattleStateOnly();
        }

        private void RunOfflineForfeit()
        {
            if (_manager == null)
                return;

            _manager.ForceWinner(_manager.BotTeam);
            SyncAll();
        }

        private async Task AnimateFaintResultsAsync()
        {
            if (_manager == null)
                return;

            if (_manager.HasBotFainted)
            {
                EnemyStatus.CurrentHP = 0;
            }

            await EnemyStatus.WaitForHpAnimation();

            if (_manager.BotActive.IsFainted)
            {
                SyncEnemyPokemon();
            }

            if (_manager.HasTrainerFainted)
            {
                PlayerStatus.CurrentHP = 0;
            }

            await PlayerStatus.WaitForHpAnimation();

            if (_manager.PlayerActive.IsFainted)
            {
                SyncPlayerPokemon();
            }
        }

        private async Task AskForFreeSwitchIfEnemyFaintedAsync()
        {
            if (_manager == null)
                return;

            if (!_manager.HasBotFainted)
                return;

            if (_manager.Winner != null)
                return;

            _manager.LogSwitchPromptAfterBotFaint();

            FlushNewDomainLogs();

            await Logger.WaitUntilQueueEmpty();

            Logger.AskYesNoQuestion(
                onYes: OpenFreeSwitchAfterBotFaint,
                onNo: () =>
                {
                    SyncAll();
                });
        }

        // ─────────────────────────────────────────────────────────────
        // Free switch after enemy faint
        // ─────────────────────────────────────────────────────────────

        private void OpenFreeSwitchAfterBotFaint()
        {
            _navigationStore.CurrentViewModel = new TeamSelectionViewModel(
                _playerUserStore,
                _navigationStore,
                () =>
                {
                    SyncAll();
                    return this;
                },
                new TeamSelectionOptions
                {
                    CanSwitch = true,
                    CanMove = false,
                    CanSummary = true,
                    AutoConfirmSelection = false,
                    IsUsingUserStore = true
                },
                OnFreeSwitchAfterBotFaintChosen,
                true);
        }

        private async void OnFreeSwitchAfterBotFaintChosen(int slotIndex)
        {
            if (_isOnline || _manager == null)
                return;

            bool switched = _manager.FreeSwitchPlayer(slotIndex);

            if (!switched)
            {
                SyncAll();
                return;
            }

            FlushNewDomainLogs();

            await Logger.WaitUntilQueueEmpty();

            SyncAll();
        }

        // ─────────────────────────────────────────────────────────────
        // Domain logger -> UI logger bridge
        // ─────────────────────────────────────────────────────────────

        private void FlushNewDomainLogs()
        {
            if (_manager == null)
                return;

            var newMessages = _manager.logger.Entries
                .Skip(_logCursor)
                .Select(e => e.Message);

            Logger.EnqueueStringEntries(newMessages);

            _logCursor = _manager.logger.Entries.Count;
        }

        // ─────────────────────────────────────────────────────────────
        // Sync
        // ─────────────────────────────────────────────────────────────

        private void SyncAll(bool flushSetup = false)
        {
            if (_isOnline)
            {
                if (_service == null || !_service.HasInitialState)
                    return;

                var snap = _service.GetState();
                if (snap.Player == null || snap.Enemy == null)
                    return;

                if (snap.Player.PokedexId <= 0 || snap.Enemy.PokedexId <= 0)
                {
                    Console.WriteLine(
                        $"[CLIENT SyncAll] Ignored invalid snapshot. PlayerDex={snap.Player.PokedexId}, EnemyDex={snap.Enemy.PokedexId}");

                    return;
                }
                PlayerStatus.PokedexId = snap.Player.PokedexId;
                PlayerStatus.PokemonName = snap.Player.Name;
                PlayerStatus.Level = snap.Player.Level;
                PlayerStatus.CurrentHP = snap.Player.CurrentHP;
                PlayerStatus.MaxHP = snap.Player.MaxHP;
                PlayerStatus.StatusCondition = snap.Player.StatusCondition;

                EnemyStatus.PokedexId = snap.Enemy.PokedexId;
                EnemyStatus.PokemonName = snap.Enemy.Name;
                EnemyStatus.Level = snap.Enemy.Level;
                EnemyStatus.CurrentHP = snap.Enemy.CurrentHP;
                EnemyStatus.MaxHP = snap.Enemy.MaxHP;
                EnemyStatus.StatusCondition = snap.Enemy.StatusCondition;

                BattleMenu.SetWaitingForOpponent(false);
                BattleMenu.RefreshMoves(snap.PlayerMoves);

                var allEntries = snap.LogEntries;

                if (allEntries.Count > _logCursor)
                {
                    Logger.EnqueueStringEntries(allEntries.Skip(_logCursor).ToList());
                    _logCursor = allEntries.Count;

                    if (flushSetup)
                        Logger.FlushSetupMessages();
                }

                OnPropertyChanged(nameof(WinnerName));

                if (snap.IsOver && !_isBattleOverHandled)
                {
                    _isBattleOverHandled = true;
                    _ = OnBattleEndedAsync();
                }

                return;
            }

            if (_manager == null)
                return;

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

            BattleMenu.RefreshMoves(_manager.PlayerActive.Moves
                .Select((m, i) => new MoveSnapshot
                {
                    Index = i,
                    Name = (m as MoveState)?.Name ?? "-",
                    Type = (m as MoveState)?.Element.ToString() ?? string.Empty,
                    PP = (m as MoveState)?.PP ?? 0,
                    MaxPP = (m as MoveState)?.MaxPP ?? 0
                })
                .ToList());

            var allBattleLogEntries = _manager.logger.BattleLog;

            if (allBattleLogEntries.Count > _logCursor)
            {
                FlushNewDomainLogs();

                if (flushSetup)
                    Logger.FlushSetupMessages();
            }

            OnPropertyChanged(nameof(WinnerName));

            if (_manager.Winner != null && !_isBattleOverHandled)
            {
                _isBattleOverHandled = true;
                _ = OnBattleEndedAsync();
            }
        }

        private void SyncPlayerPokemon()
        {
            if (_isOnline || _manager == null)
                return;

            var p = _manager.PlayerActive;

            PlayerStatus.PokedexId = p.PokedexId;
            PlayerStatus.PokemonName = p.Name;
            PlayerStatus.Level = p.Level;
            PlayerStatus.MaxHP = p.MaxHP;
            PlayerStatus.CurrentHP = p.CurrentHP;
            PlayerStatus.StatusCondition = p.Status.ToString();

            BattleMenu.RefreshMoves(p.Moves.Select((m, i) =>
            {
                var ms = m as MoveState;

                return new MoveSnapshot
                {
                    Index = i,
                    Name = ms?.Name ?? "???",
                    Type = ms?.Element.ToString() ?? "Normal",
                    PP = ms?.PP ?? 0,
                    MaxPP = ms?.MaxPP ?? 0,
                    Power = ms?.Category == MoveCategory.Status ? null : 0,
                    Accuracy = 100
                };
            }).ToList());
        }

        private void SyncEnemyPokemon()
        {
            if (_isOnline || _manager == null)
                return;

            var e = _manager.BotActive;

            EnemyStatus.PokedexId = e.PokedexId;
            EnemyStatus.PokemonName = e.Name;
            EnemyStatus.Level = e.Level;
            EnemyStatus.MaxHP = e.MaxHP;
            EnemyStatus.CurrentHP = e.CurrentHP;
            EnemyStatus.StatusCondition = e.Status.ToString();
        }

        private void SyncBattleStateOnly()
        {
            if (_isOnline || _manager == null)
                return;

            BattleMenu.RefreshMoves(_manager.PlayerActive.Moves
                .Select((m, i) => new MoveSnapshot
                {
                    Index = i,
                    Name = (m as MoveState)?.Name ?? "-",
                    Type = (m as MoveState)?.Element.ToString() ?? string.Empty,
                    PP = (m as MoveState)?.PP ?? 0,
                    MaxPP = (m as MoveState)?.MaxPP ?? 0
                })
                .ToList());

            OnPropertyChanged(nameof(WinnerName));

            if (_manager.Winner != null && !_isBattleOverHandled)
            {
                _isBattleOverHandled = true;
                _ = OnBattleEndedAsync();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Battle result
        // ─────────────────────────────────────────────────────────────

        private async Task OnBattleEndedAsync()
        {
            bool playerWon;

            if (_isOnline)
            {
                if (_service == null || !_service.HasInitialState)
                    return;

                var snap = _service.GetState();

                playerWon = snap.WinnerPlayerId.HasValue
                            && snap.WinnerPlayerId.Value == _playerUserStore.BattlePlayerID;
            }
            else
            {
                if (_manager == null)
                    return;

                playerWon = _manager.Winner == _manager.PlayerTeam;
            }

            RankDelta = playerWon ? 22 : -18;

            int newTotalElo = Math.Max(0, STARTING_ELO + RankDelta);

            var rankInfo = RankManager.GetRank(newTotalElo);

            WinnerText = playerWon ? "YOU WON!" : "YOU LOST!";
            ResultMethod = playerWon
                ? "All opposing Pokémon fainted"
                : "Your party fainted";

            RankName = rankInfo.RankName;
            RatingCurrent = rankInfo.CurrentProgress;
            RatingMax = rankInfo.MaxProgress;

            BattleResultAction action =
                await _dialogService.ShowBattleResultAsync(this);

            switch (action)
            {
                case BattleResultAction.NewGame:
                case BattleResultAction.Back:
                case BattleResultAction.Rematch:
                    _navigationStore.CurrentViewModel = _createGameModeChooserViewModel();
                    break;
            }
        }

        private static BotLevel ResolveBotLevel(BotDifficulty difficulty)
        {
            return difficulty switch
            {
                BotDifficulty.Easy => BotLevel.Easy,
                BotDifficulty.Medium => BotLevel.Medium,
                BotDifficulty.Hard => BotLevel.Hard,
                _ => BotLevel.Easy
            };
        }
    }

    // ─────────────────────────────────────────────────────────────
    // BattlePokemonMovesetChooserViewModel
    // ─────────────────────────────────────────────────────────────

    public class BattlePokemonMovesetChooserViewModel : ViewModelBase
    {
        private readonly Action<int> _onMoveClicked;
        private readonly Action<MoveSnapshot?> _onMoveHovered;
        private readonly Action _onBack;
        private readonly Action _onBackCleared;

        public MoveSlotViewModel Move0 { get; }
        public MoveSlotViewModel Move1 { get; }
        public MoveSlotViewModel Move2 { get; }
        public MoveSlotViewModel Move3 { get; }

        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                var slots = Slots;

                _selectedIndex = MathHelper.Clamp(value, 0, 3);

                for (int i = 0; i < slots.Length; i++)
                    slots[i].IsSelected = i == _selectedIndex;

                _onMoveHovered(slots[_selectedIndex].Snapshot);

                OnPropertyChanged(nameof(SelectedIndex));
            }
        }

        private MoveSlotViewModel[] Slots => new[]
        {
            Move0,
            Move1,
            Move2,
            Move3
        };

        public BattlePokemonMovesetChooserViewModel(
            Action<int> onMoveClicked,
            Action<MoveSnapshot?> onMoveHovered,
            Action onBack,
            Action onBackCleared,
            BattleLoggerViewModel logger)
        {
            _onBack = onBack;
            _onMoveClicked = onMoveClicked;
            _onMoveHovered = onMoveHovered;
            _onBackCleared = onBackCleared;

            Move0 = new MoveSlotViewModel(0, onMoveClicked, onMoveHovered, logger);
            Move1 = new MoveSlotViewModel(1, onMoveClicked, onMoveHovered, logger);
            Move2 = new MoveSlotViewModel(2, onMoveClicked, onMoveHovered, logger);
            Move3 = new MoveSlotViewModel(3, onMoveClicked, onMoveHovered, logger);
        }

        public void MoveSelection(int dx, int dy)
        {
            int col = _selectedIndex % 2;
            int row = _selectedIndex / 2;

            if (dx == 1 && col == 1)
            {
                _onBack();
                return;
            }

            _onBackCleared();

            col = (col + dx + 2) % 2;
            row = (row + dy + 2) % 2;

            SelectedIndex = row * 2 + col;
        }

        public void ConfirmSelection()
        {
            var slot = Slots[_selectedIndex];

            if (slot.IsEnabled)
                slot.ClickCommand.Execute(null);
        }

        public void ResetSelection()
        {
            SelectedIndex = 0;
        }

        public void LoadMoves(IReadOnlyList<MoveSnapshot> moves)
        {
            var slots = Slots;

            for (int i = 0; i < 4; i++)
            {
                if (i < moves.Count)
                    slots[i].SetMoveFromSnapshot(moves[i]);
                else
                    slots[i].Clear();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // MoveSlotViewModel
    // ─────────────────────────────────────────────────────────────

    public class MoveSlotViewModel : ViewModelBase
    {
        private readonly int _index;
        private readonly Action<int> _onClick;
        private readonly Action<MoveSnapshot?> _onHover;
        private readonly BattleLoggerViewModel _logger;

        private string _moveName = "-";
        private bool _hasMove = false;
        private IMove? _move;
        private MoveSnapshot? _snapshot;

        public string MoveName
        {
            get => _moveName;
            private set => SetProperty(ref _moveName, value);
        }

        public bool IsEnabled => _hasMove && _logger.AreActionsUnlocked;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public MoveSnapshot? Snapshot => _snapshot;

        public ICommand ClickCommand { get; }
        public ICommand HoverCommand { get; }
        public ICommand LeaveCommand { get; }

        public MoveSlotViewModel(
            int index,
            Action<int> onClick,
            Action<MoveSnapshot?> onHover,
            BattleLoggerViewModel logger)
        {
            _index = index;
            _onClick = onClick;
            _onHover = onHover;
            _logger = logger;

            ClickCommand = new RelayCommand(
                () => _onClick(_index),
                () => IsEnabled);

            HoverCommand = new RelayCommand(
                () => _onHover(_snapshot));

            LeaveCommand = new RelayCommand(
                () => _onHover(null));

            _logger.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BattleLoggerViewModel.AreActionsUnlocked))
                {
                    OnPropertyChanged(nameof(IsEnabled));
                    ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
                }
            };
        }

        public void SetMoveFromSnapshot(MoveSnapshot snap)
        {
            _move = null;
            _snapshot = snap;

            MoveName = snap.Name;
            _hasMove = true;

            OnPropertyChanged(nameof(IsEnabled));
            ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
        }

        public void SetMove(IMove move)
        {
            _move = move;
            _snapshot = null;

            MoveName = (_move as MoveState)?.Name ?? "-";
            _hasMove = true;

            OnPropertyChanged(nameof(IsEnabled));
            ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
        }

        public void Clear()
        {
            _move = null;
            _snapshot = null;

            MoveName = "-";
            _hasMove = false;

            OnPropertyChanged(nameof(IsEnabled));
            ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
        }
    }
}