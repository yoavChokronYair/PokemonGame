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

        public PokemonBattleStatusViewModel PlayerStatus { get; }
        public EnemyBattleStatusViewModel EnemyStatus { get; }
        public BattleMenuViewModel BattleMenu { get; }
        public BattleLoggerViewModel Logger { get; }

        // ── Online log cursor: tracks how many snapshot log entries we've consumed ──
        private int _logCursor = 0;
        private bool _isBattleOverHandled = false;

        // ── Online log: we accumulate string entries here so we never touch _manager ──
        private readonly List<string> _onlineLogAccumulator = new();

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
                // ── Online path ───────────────────────────────────────────────
                _service = playerUserStore.BattleService!;
                _service.OnStateUpdated += () =>
                    System.Windows.Application.Current.Dispatcher.Invoke(() => SyncAll());
            }
            else
            {
                // ── Offline path ──────────────────────────────────────────────
                var session = playerUserStore.BattleSesion;
                var playerTeam = session.ResolvedPlayerTeam
                    ?? throw new InvalidOperationException("Team not set.");
                var botTeam = session.ResolvedBotTeam
                    ?? throw new InvalidOperationException("Bot team not set.");
                var botLevel = ResolveBotLevel(session.BotDifficulty);

                _manager = new BattleManager(playerTeam, botTeam, botLevel);
            }

            Logger = new BattleLoggerViewModel();
            PlayerStatus = new PokemonBattleStatusViewModel();
            EnemyStatus = new EnemyBattleStatusViewModel();
            BattleMenu = new BattleMenuViewModel(
                OnMoveChosen,
                OnSwitchChosen,
                OnForfeit,
                OnOpenSwitch,
                _manager,
                Logger);

            NewGameCommand = new RelayCommand(() => CloseRequested?.Invoke(this, BattleResultAction.NewGame));
            BackCommand = new RelayCommand(() => CloseRequested?.Invoke(this, BattleResultAction.Back));
            RematchCommand = new RelayCommand(() => CloseRequested?.Invoke(this, BattleResultAction.Rematch));

            SyncAll(flushSetup: true);
        }

        // ── Move chosen ───────────────────────────────────────────────────────
        private async void OnMoveChosen(int moveIndex)
        {
            if (_isOnline)
            {
                // Fire-and-forget; state update arrives via OnStateUpdated event
                _service!.RunTurn(moveIndex);
            }
            else
            {
                _manager!.RunTurn(moveIndex);

                Logger.EnqueueEntries(
                    _manager.logger.Entries
                        .Skip(_logCursor)
                        .ToList());

                _logCursor = _manager.logger.Entries.Count;

                await Logger.WaitUntilQueueEmpty();

                EnemyStatus.CurrentHP = _manager.BotActive.CurrentHP;
                await EnemyStatus.WaitForHpAnimation();
                if (_manager.BotActive.IsFainted) SyncEnemyPokemon();

                PlayerStatus.CurrentHP = _manager.PlayerActive.CurrentHP;
                await PlayerStatus.WaitForHpAnimation();
                if (_manager.PlayerActive.IsFainted) SyncPlayerPokemon();

                SyncAll();
            }
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

                var newEntries = _manager.logger.Entries
                    .Skip(_logCursor)
                    .ToList();

                Logger.EnqueueEntries(newEntries);
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
            {
                _service!.Forfeit();
            }
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

                BattleMenu.RefreshMoves(snap.PlayerMoves);

                // ── Use snapshot log entries directly — never touch _manager ──
                var allEntries = snap.LogEntries;
                if (allEntries.Count > _logCursor)
                {
                    var newStringEntries = allEntries
                        .Skip(_logCursor)
                        .ToList();

                    Logger.EnqueueStringEntries(newStringEntries);
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
                        PP = (m as MoveState)?.PP ?? 0
                    })
                    .ToList());

                var allEntries = _manager.logger.BattleLog;
                if (allEntries.Count > _logCursor)
                {
                    var newEntries = _manager.logger.Entries
                        .Skip(_logCursor)
                        .ToList();

                    Logger.EnqueueEntries(newEntries);
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

        // ── SyncPlayerPokemon / SyncEnemyPokemon — offline only ──────────────
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

            var snapshots = p.Moves.Select((m, i) =>
            {
                var moveBase = m as MoveState;
                return new MoveSnapshot
                {
                    Index = i,
                    Name = moveBase?.Name ?? "???",
                    Type = moveBase?.Element.ToString() ?? "Normal",
                    PP = moveBase?.PP ?? 0,
                    Power = moveBase?.Category == MoveCategory.Status ? null : 0,
                    Accuracy = 100
                };
            }).ToList();

            BattleMenu.RefreshMoves(snapshots);
        }

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
                })
                .ToList());

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
                // FIX #8: Compare winner player ID, not Pokémon name vs username
                var snap = _service!.GetState();
                playerWon = snap.WinnerName == _playerUserStore.Username;
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

        public void LoadMoves(IReadOnlyList<MoveSnapshot> moves)
        {
            var slots = new[] { Move0, Move1, Move2, Move3 };
            for (int i = 0; i < 4; i++)
            {
                if (i < moves.Count)
                    slots[i].SetMoveFromSnapshot(moves[i]);
                else
                    slots[i].Clear();
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
        private bool _hasMove = false;
        private IMove? _move;

        public string MoveName
        {
            get => _moveName;
            private set => SetProperty(ref _moveName, value);
        }

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
            MoveName = snap.Name;
            _hasMove = true;
            OnPropertyChanged(nameof(IsEnabled));
            ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
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