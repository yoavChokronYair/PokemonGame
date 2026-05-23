using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class BattleViewModel : ViewModelBase
    {
        // ── Online path ───────────────────────────────────────────────────────
        private readonly IBattleService? _service;

        // ── Offline path ──────────────────────────────────────────────────────
        private readonly BattleManager? _manager;

        // ── Mode flag ─────────────────────────────────────────────────────────
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

        public double ProgressBarWidth =>
           RatingMax > 0 ? MathHelper.Clamp((double)RatingCurrent / RatingMax * 200.0, 0, 200) : 0;

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

        public string? WinnerName => _isOnline
            ? (_service!.GetState().IsOver ? _service.GetState().WinnerName : null)
            : _manager!.Winner?.Active.Name;

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
                    System.Windows.Application.Current.Dispatcher.Invoke(() => SyncAll());
            }
            else
            {
                var session = playerUserStore.BattleSesion;
                var playerTeam = session.ResolvedPlayerTeam
                    ?? throw new InvalidOperationException("Player team not set.");
                var botTeam = session.ResolvedBotTeam
                    ?? throw new InvalidOperationException("Bot team not set.");

                _manager = new BattleManager(playerTeam, botTeam, ResolveBotLevel(session.BotDifficulty));
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

            NewGameCommand = new RelayCommand(() => CloseRequested?.Invoke(this, BattleResultAction.NewGame));
            BackCommand = new RelayCommand(() => CloseRequested?.Invoke(this, BattleResultAction.Back));
            RematchCommand = new RelayCommand(() => CloseRequested?.Invoke(this, BattleResultAction.Rematch));

            SyncAll(flushSetup: true);
        }
        private void OnOpenBag()
        {
            Logger.EnqueueStringEntries(new[]
            {
                "You cannot use the Bag in this battle."
            });
        }

        // ── Move chosen ───────────────────────────────────────────────────────
        private async Task OnMoveChosen(int moveIndex)
        {
            if (_isOnline)
            {
                // Lock buttons immediately so player can't change their mind
                BattleMenu.SetWaitingForOpponent(true);
                _service!.RunTurn(moveIndex);
                // UI will update when OnStateUpdated fires from server
            }
            else
            {
                _manager!.RunTurn(moveIndex);

                // Replace your two lines with this:
                var newMessages = _manager.logger.Entries
                    .Skip(_logCursor)
                    .Select(e => e.Message); // Extract just the string

                Logger.EnqueueStringEntries(newMessages);
                _logCursor = _manager.logger.Entries.Count;

                await Logger.WaitUntilQueueEmpty();
                if (_manager.HasBotFainted)
                {
                    EnemyStatus.CurrentHP = 0;

                }
                await EnemyStatus.WaitForHpAnimation();
                if (_manager.BotActive.IsFainted) SyncEnemyPokemon();
                if (_manager.HasTrainerFainted)
                {
                    PlayerStatus.CurrentHP = 0;
                }
                await PlayerStatus.WaitForHpAnimation();
                if (_manager.PlayerActive.IsFainted) SyncPlayerPokemon();

                SyncAll();
                if (_manager.HasBotFainted && _manager.Winner == null)
                {
                    _manager.LogSwitchPromptAfterBotFaint();

                    var questionMessages = _manager.logger.Entries
                        .Skip(_logCursor)
                        .Select(e => e.Message);

                    Logger.EnqueueStringEntries(questionMessages);
                    _logCursor = _manager.logger.Entries.Count;

                    await Logger.WaitUntilQueueEmpty();

                    Logger.AskYesNoQuestion(
                        onYes: OpenFreeSwitchAfterBotFaint,
                        onNo: () =>
                        {
                            SyncAll();
                        });
                }
            }
        }
        private void OpenFreeSwitchAfterBotFaint()
        {
            _navigationStore.CurrentViewModel = new TeamSelectionViewModel(
                _playerUserStore,
                _navigationStore,
                () =>
                {
                    SyncPlayerPokemon();
                    SyncEnemyPokemon();
                    SyncBattleStateOnly();
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
            if (_isOnline)
                return;

            bool switched = _manager!.FreeSwitchPlayer(slotIndex);

            if (!switched)
            {
                SyncPlayerPokemon();
                SyncEnemyPokemon();
                SyncBattleStateOnly();
                return;
            }

            var newMessages = _manager.logger.Entries
                .Skip(_logCursor)
                .Select(e => e.Message);

            Logger.EnqueueStringEntries(newMessages);
            _logCursor = _manager.logger.Entries.Count;

            await Logger.WaitUntilQueueEmpty();

            SyncPlayerPokemon();
            SyncEnemyPokemon();
            SyncBattleStateOnly();
        }

        // ── Switch chosen ─────────────────────────────────────────────────────
        private async void OnSwitchChosen(int slotIndex)
        {
            if (_isOnline)
            {
                _service!.RunTurn(slotIndex, "Switch");
            }
            else
            {
                _manager!.RunTurn(slotIndex, BattleAction.Switch);

                var newMessages = _manager.logger.Entries
                    .Skip(_logCursor)
                    .Select(e => e.Message); // Extract just the string

                Logger.EnqueueStringEntries(newMessages);
                _logCursor = _manager.logger.Entries.Count;

                await Logger.WaitUntilQueueEmpty();

                SyncPlayerPokemon();
                SyncEnemyPokemon();
                SyncBattleStateOnly();
            }
        }

        // ── Forfeit ───────────────────────────────────────────────────────────
        private void OnForfeit()
        {
            if (_isOnline)
                _service!.Forfeit();
            else
            {
                _manager!.ForceWinner(_manager.BotTeam);
                SyncAll();
            }
        }

        // ── Open switch screen ────────────────────────────────────────────────
        private void OnOpenSwitch()
        {
            _navigationStore.CurrentViewModel = new TeamSelectionViewModel(
                _playerUserStore,
                _navigationStore,
                () => this,
                new TeamSelectionOptions(),
                OnSwitchChosen,
                true);
        }

        // ── SyncAll ───────────────────────────────────────────────────────────
        private void SyncAll(bool flushSetup = false)
        {
            if (_isOnline)
            {
                var snap = _service!.GetState();

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

                // FIX #6: use EnqueueStringEntries — online log arrives as List<string>
                // from the snapshot, never from _manager (which is null in online mode).
                var allEntries = snap.LogEntries;
                if (allEntries.Count > _logCursor)
                {
                    Logger.EnqueueStringEntries(allEntries.Skip(_logCursor).ToList());
                    _logCursor = allEntries.Count;
                    if (flushSetup) Logger.FlushSetupMessages();
                }

                OnPropertyChanged(nameof(WinnerName));

                if (snap.IsOver && !_isBattleOverHandled)
                {
                    _isBattleOverHandled = true;
                    _ = OnBattleEndedAsync();
                }
            }
            else
            {
                var p = _manager!.PlayerActive;
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
                        MaxPP = (m as MoveState)?.MaxPP ?? 0  // ← add this
                    }).ToList());

                var allEntries = _manager.logger.BattleLog;
                if (allEntries.Count > _logCursor)
                {
                    // Replace your two lines with this:
                    var newMessages = _manager.logger.Entries
                        .Skip(_logCursor)
                        .Select(e => e.Message); // Extract just the string

                    Logger.EnqueueStringEntries(newMessages);
                    _logCursor = _manager.logger.Entries.Count;
                    if (flushSetup) Logger.FlushSetupMessages();
                }

                OnPropertyChanged(nameof(WinnerName));

                if (_manager.Winner != null && !_isBattleOverHandled)
                {
                    _isBattleOverHandled = true;
                    _ = OnBattleEndedAsync();
                }
            }
        }

        // ── SyncPlayerPokemon — offline only ──────────────────────────────────
        private void SyncPlayerPokemon()
        {
            if (_isOnline) return;
            var p = _manager!.PlayerActive;
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

        // ── SyncEnemyPokemon — offline only ───────────────────────────────────
        private void SyncEnemyPokemon()
        {
            if (_isOnline) return;
            var e = _manager!.BotActive;
            EnemyStatus.PokedexId = e.PokedexId;
            EnemyStatus.PokemonName = e.Name;
            EnemyStatus.Level = e.Level;
            EnemyStatus.MaxHP = e.MaxHP;
            EnemyStatus.CurrentHP = e.CurrentHP;
            EnemyStatus.StatusCondition = e.Status.ToString();
        }

        // ── SyncBattleStateOnly — offline only ────────────────────────────────
        private void SyncBattleStateOnly()
        {
            if (_isOnline) return;

            BattleMenu.RefreshMoves(_manager!.PlayerActive.Moves
                .Select((m, i) => new MoveSnapshot
                {
                    Index = i,
                    Name = (m as MoveState)?.Name ?? "-",
                    Type = (m as MoveState)?.Element.ToString() ?? string.Empty,
                    PP = (m as MoveState)?.PP ?? 0
                }).ToList());

            OnPropertyChanged(nameof(WinnerName));

            if (_manager.Winner != null && !_isBattleOverHandled)
            {
                _isBattleOverHandled = true;
                _ = OnBattleEndedAsync();
            }
        }

        // ── OnBattleEndedAsync ────────────────────────────────────────────────
        private async Task OnBattleEndedAsync()
        {
            bool playerWon;

            if (_isOnline)
            {
                var snap = _service!.GetState();

                // FIX #2: compare WinnerPlayerId (int) to BattlePlayerID (int).
                // The old code compared WinnerName (a display string, e.g. "Alice")
                // to Username, which is fragile and was set to a Pokémon's name
                // in ServerBattleSession before fix #4 was applied.
                playerWon = snap.WinnerPlayerId.HasValue
                            && snap.WinnerPlayerId.Value == _playerUserStore.BattlePlayerID;
            }
            else
            {
                playerWon = _manager!.Winner == _manager.PlayerTeam;
            }

            RankDelta = playerWon ? 22 : -18;
            int newTotalElo = Math.Max(0, STARTING_ELO + RankDelta);
            var rankInfo = RankManager.GetRank(newTotalElo);

            WinnerText = playerWon ? "YOU WON!" : "YOU LOST!";
            ResultMethod = playerWon ? "All opposing Pokémon fainted" : "Your party fainted";
            RankName = rankInfo.RankName;
            RatingCurrent = rankInfo.CurrentProgress;
            RatingMax = rankInfo.MaxProgress;

            BattleResultAction action = await _dialogService.ShowBattleResultAsync(this);

            switch (action)
            {
                case BattleResultAction.NewGame:
                case BattleResultAction.Back:
                case BattleResultAction.Rematch:
                    _navigationStore.CurrentViewModel = _createGameModeChooserViewModel();
                    break;
            }
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
                // fire hover for the newly selected slot
                _onMoveHovered(slots[_selectedIndex].Snapshot);
                OnPropertyChanged(nameof(SelectedIndex));
            }
        }

        private MoveSlotViewModel[] Slots => new[] { Move0, Move1, Move2, Move3 };

        public void MoveSelection(int dx, int dy)
        {

            int col = _selectedIndex % 2;
            int row = _selectedIndex / 2;

            if (dx == 1 && col == 1)
            {
                _onBack();
                return;
            }
            _onBackCleared();  // clear back selection whenever navigating moves

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

        public void ResetSelection() => SelectedIndex = 0;

        private readonly Action<MoveSnapshot?> _onMoveHovered;
        private readonly Action _onBack;
        private readonly Action _onBackCleared;


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

        public void LoadMoves(IReadOnlyList<MoveSnapshot> moves)
        {
            var slots = new[] { Move0, Move1, Move2, Move3 };
            for (int i = 0; i < 4; i++)
            {
                if (i < moves.Count) slots[i].SetMoveFromSnapshot(moves[i]);
                else slots[i].Clear();
            }
        }
    }

    // ── MoveSlotViewModel ─────────────────────────────────────────────────────
    public class MoveSlotViewModel : ViewModelBase
    {
        private readonly int _index;
        private readonly Action<int> _onClick;
        private readonly Action<MoveSnapshot?> _onHover;
        private readonly BattleLoggerViewModel _logger;

        private string _moveName = "-";
        private bool _hasMove = false;
        private IMove? _move;

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

            ClickCommand = new RelayCommand(() => _onClick(_index), () => IsEnabled);
            HoverCommand = new RelayCommand(() => _onHover(_snapshot));
            LeaveCommand = new RelayCommand(() => _onHover(null));

            _logger.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BattleLoggerViewModel.AreActionsUnlocked))
                {
                    OnPropertyChanged(nameof(IsEnabled));
                    ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
                }
            };
        }

        private MoveSnapshot? _snapshot;

        public void SetMoveFromSnapshot(MoveSnapshot snap)
        {
            _move = null;
            _snapshot = snap;
            MoveName = snap.Name;
            _hasMove = true;
            OnPropertyChanged(nameof(IsEnabled));
            ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
        }

        public MoveSnapshot? Snapshot => _snapshot;

        public void SetMove(IMove move)
        {
            _move = move;
            MoveName = (_move as MoveState)?.Name ?? "-";
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