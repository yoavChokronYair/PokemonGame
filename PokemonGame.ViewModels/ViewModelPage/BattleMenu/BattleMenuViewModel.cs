using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;

public class BattleMenuViewModel : ViewModelBase
{
    public enum BattleUiState
    {
        Logger,
        Menu,
        Moveset
    }

    // ─────────────────────────────────────────────────────────────
    // State helpers
    // ─────────────────────────────────────────────────────────────

    public bool IsLoggerActive => UiState == BattleUiState.Logger;
    public bool IsMenuActive => UiState == BattleUiState.Menu;
    public bool IsMovesetActive => UiState == BattleUiState.Moveset;

    private BattleUiState _uiState = BattleUiState.Logger;
    public BattleUiState UiState
    {
        get => _uiState;
        private set
        {
            if (SetProperty(ref _uiState, value))
            {
                OnPropertyChanged(nameof(IsLoggerActive));
                OnPropertyChanged(nameof(IsMenuActive));
                OnPropertyChanged(nameof(IsMovesetActive));
            }
        }
    }

    private bool _isWaitingForLog;
    private bool _waitingForOpponent;
    private bool _isInputLocked;

    public bool IsInputLocked
    {
        get => _isInputLocked;
        set
        {
            if (SetProperty(ref _isInputLocked, value))
                NotifyInputChanged();
        }
    }

    public bool ActionsLocked =>
        _waitingForOpponent || !_logger.AreActionsUnlocked;

    public bool AreInputsEnabled =>
        !_isInputLocked &&
        _logger.AreActionsUnlocked &&
        !_waitingForOpponent;

    public bool IsMainMenuVisible =>
        !IsMovesetVisible &&
        !_isWaitingForLog &&
        !_waitingForOpponent &&
        !IsReconnectEscapeVisible;

    // ─────────────────────────────────────────────────────────────
    // Callbacks
    // ─────────────────────────────────────────────────────────────

    private readonly Func<int, Task> _onMoveChosen;
    private readonly Action<int> _onSwitchChosen;
    private readonly Action _onForfeit;
    private readonly Action _onBag;
    private readonly Action _onSwitch;
    private readonly BattleLoggerViewModel _logger;
    private readonly Action _onDisconnectFromMatch;

    public BattleLoggerViewModel Logger => _logger;

    // ─────────────────────────────────────────────────────────────
    // Main menu selection
    //
    // 0 = Fight
    // 1 = Bag
    // 2 = Pokemon
    // 3 = Run
    // ─────────────────────────────────────────────────────────────

    private int _menuSelectedIndex;
    public int MenuSelectedIndex
    {
        get => _menuSelectedIndex;
        set
        {
            _menuSelectedIndex = (value + 4) % 4;

            OnPropertyChanged(nameof(MenuSelectedIndex));
            OnPropertyChanged(nameof(IsFightSelected));
            OnPropertyChanged(nameof(IsBagSelected));
            OnPropertyChanged(nameof(IsPokemonSelected));
            OnPropertyChanged(nameof(IsRunSelected));
        }
    }

    public bool IsFightSelected => MenuSelectedIndex == 0;
    public bool IsBagSelected => MenuSelectedIndex == 1;
    public bool IsPokemonSelected => MenuSelectedIndex == 2;
    public bool IsRunSelected => MenuSelectedIndex == 3;

    // ─────────────────────────────────────────────────────────────
    // Moveset state
    // ─────────────────────────────────────────────────────────────

    private bool _isMovesetVisible;
    public bool IsMovesetVisible
    {
        get => _isMovesetVisible;
        set
        {
            if (SetProperty(ref _isMovesetVisible, value))
                OnPropertyChanged(nameof(IsMainMenuVisible));
        }
    }

    private bool _isBackSelected;
    public bool IsBackSelected
    {
        get => _isBackSelected;
        set => SetProperty(ref _isBackSelected, value);
    }

    private MoveSnapshot? _selectedSnapshot;
    public MoveSnapshot? SelectedSnapshot
    {
        get => _selectedSnapshot;
        private set
        {
            if (SetProperty(ref _selectedSnapshot, value))
            {
                OnPropertyChanged(nameof(SelectedMovePP));
                OnPropertyChanged(nameof(SelectedMoveType));
            }
        }
    }

    public string SelectedMovePP =>
        SelectedSnapshot is MoveSnapshot s
            ? $"PP {s.PP}/{s.MaxPP}"
            : "PP --/--";

    public string SelectedMoveType =>
        SelectedSnapshot is MoveSnapshot s
            ? $"TYPE/ {s.Type}"
            : "TYPE/ --";

    public BattlePokemonMovesetChooserViewModel MovesetChooser { get; }

    // ─────────────────────────────────────────────────────────────
    // Commands
    // ─────────────────────────────────────────────────────────────

    public ICommand LoggerNextCommand { get; }
    public ICommand OpenMenuCommand { get; }
    public ICommand BackCommand { get; }

    public ICommand FightKeyCommand { get; }
    public ICommand MoveKeyCommand { get; }

    public ICommand MenuNavigateCommand { get; }
    public ICommand MenuConfirmCommand { get; }

    public ICommand MoveSelectCommand { get; }
    public ICommand ConfirmMoveCommand { get; }

    public ICommand OpenMovesetCommand { get; }
    public ICommand CloseMovesetCommand { get; }

    public ICommand OpenBagCommand { get; }
    public ICommand OpenSwitchCommand { get; }
    public ICommand ForfeitCommand { get; }

    // ─────────────────────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────────────────────

    public BattleMenuViewModel(
        Func<int, Task> onMoveChosen,
        Action<int> onSwitchChosen,
        Action onForfeit,
        Action onBag,
        Action onSwitch,
        Action onDisconnectFromMatch,
        BattleLoggerViewModel logger)
    { 
        _onMoveChosen = onMoveChosen;
        _onSwitchChosen = onSwitchChosen;
        _onForfeit = onForfeit;
        _onBag = onBag;
        _onSwitch = onSwitch;
        _logger = logger;

        MovesetChooser = new BattlePokemonMovesetChooserViewModel(
            OnMoveButtonClicked,
            OnMoveHovered,
            () => IsBackSelected = true,
            () => IsBackSelected = false,
            logger);

        LoggerNextCommand = new RelayCommand(
            _logger.ShowNext,
            () => _logger.HasMore);

        OpenMovesetCommand = new RelayCommand(
            OpenMoveset,
            () => AreInputsEnabled);

        CloseMovesetCommand = new RelayCommand(CloseMoveset);

        OpenBagCommand = new RelayCommand(
            () => _onBag(),
            () => AreInputsEnabled);

        OpenSwitchCommand = new RelayCommand(
            () => _onSwitch(),
            () => AreInputsEnabled);

        ForfeitCommand = new RelayCommand(
            () => _onForfeit(),
            () => AreInputsEnabled);

        OpenMenuCommand = new RelayCommand(
            OpenMoveset,
            () => AreInputsEnabled);

        MenuNavigateCommand = new RelayCommand<string>(NavigateMainMenu);

        MenuConfirmCommand = new RelayCommand(ConfirmMainMenu);

        MoveSelectCommand = new RelayCommand<string>(MoveSelection);

        ConfirmMoveCommand = new RelayCommand(ConfirmCurrentSelection);

        FightKeyCommand = new RelayCommand(() =>
        {
            if (!AreInputsEnabled)
                return;

            OpenMovesetCommand.Execute(null);
        });

        BackCommand = new RelayCommand(() =>
        {
            if (IsMovesetVisible)
            {
                CloseMovesetCommand.Execute(null);
                return;
            }

            if (UiState == BattleUiState.Menu)
                SetState(BattleUiState.Logger);
        });

        MoveKeyCommand = new RelayCommand<string>(param =>
        {
            if (int.TryParse(param, out int index))
                ExecuteMoveKey(index);
        });

        _logger.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BattleLoggerViewModel.AreActionsUnlocked))
            {
                NotifyInputChanged();
                CommandManager.InvalidateRequerySuggested();
            }

            if (e.PropertyName == nameof(BattleLoggerViewModel.HasMore))
            {
                ((RelayCommand)LoggerNextCommand).NotifyCanExecuteChanged();
            }
        };
        _onDisconnectFromMatch = onDisconnectFromMatch;
        DisconnectFromMatchCommand = new RelayCommand(
            () => _onDisconnectFromMatch(),
            () => IsReconnectEscapeVisible);
        MenuSelectedIndex = 0;
        SetState(BattleUiState.Logger);
        RefreshCommands();
    }
    public void SetReconnectEscapeVisible(bool visible)
    {
        IsReconnectEscapeVisible = visible;

        if (visible)
        {
            IsMovesetVisible = false;
            SelectedSnapshot = null;
            IsBackSelected = false;
            SetState(BattleUiState.Logger);
        }

        OnPropertyChanged(nameof(IsMainMenuVisible));
    }
    // ─────────────────────────────────────────────────────────────
    // Main menu
    // ─────────────────────────────────────────────────────────────
    private bool _isReconnectEscapeVisible;

    public bool IsReconnectEscapeVisible
    {
        get => _isReconnectEscapeVisible;
        set
        {
            if (SetProperty(ref _isReconnectEscapeVisible, value))
            {
                OnPropertyChanged(nameof(IsMainMenuVisible));
                ((RelayCommand)DisconnectFromMatchCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public ICommand DisconnectFromMatchCommand { get; }
    private void NavigateMainMenu(string? param)
    {
        if (IsMovesetVisible || !AreInputsEnabled)
            return;

        int dx = param == "Right" ? 1 :
                 param == "Left" ? -1 : 0;

        int dy = param == "Down" ? 1 :
                 param == "Up" ? -1 : 0;

        int col = (_menuSelectedIndex % 2 + dx + 2) % 2;
        int row = (_menuSelectedIndex / 2 + dy + 2) % 2;

        MenuSelectedIndex = row * 2 + col;
    }

    private void ConfirmMainMenu()
    {
        if (IsMovesetVisible || !AreInputsEnabled)
            return;

        switch (MenuSelectedIndex)
        {
            case 0:
                OpenMovesetCommand.Execute(null);
                break;

            case 1:
                OpenBagCommand.Execute(null);
                break;

            case 2:
                OpenSwitchCommand.Execute(null);
                break;

            case 3:
                ForfeitCommand.Execute(null);
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Moveset
    // ─────────────────────────────────────────────────────────────

    private void OpenMoveset()
    {
        if (!AreInputsEnabled)
            return;

        IsMovesetVisible = true;
        SelectedSnapshot = null;
        IsBackSelected = false;

        MovesetChooser.ResetSelection();

        SetState(BattleUiState.Moveset);
    }

    private void CloseMoveset()
    {
        IsMovesetVisible = false;
        SelectedSnapshot = null;
        IsBackSelected = false;

        SetState(BattleUiState.Menu);
    }

    private void MoveSelection(string? param)
    {
        if (IsMovesetVisible)
        {
            switch (param)
            {
                case "Left":
                    MovesetChooser.MoveSelection(-1, 0);
                    break;

                case "Right":
                    MovesetChooser.MoveSelection(1, 0);
                    break;

                case "Up":
                    MovesetChooser.MoveSelection(0, -1);
                    break;

                case "Down":
                    MovesetChooser.MoveSelection(0, 1);
                    break;
            }

            return;
        }

        if (AreInputsEnabled)
            MenuNavigateCommand.Execute(param);
    }

    private void ConfirmCurrentSelection()
    {
        if (IsMovesetVisible)
        {
            if (IsBackSelected)
                CloseMovesetCommand.Execute(null);
            else
                MovesetChooser.ConfirmSelection();

            return;
        }

        if (AreInputsEnabled)
        {
            MenuConfirmCommand.Execute(null);
            return;
        }

        if (_logger.HasMore)
            _logger.ShowNext();
    }

    private async void OnMoveButtonClicked(int index)
    {
        IsMovesetVisible = false;
        SelectedSnapshot = null;
        IsBackSelected = false;

        _isWaitingForLog = true;
        OnPropertyChanged(nameof(IsMainMenuVisible));

        await _onMoveChosen(index);

        _isWaitingForLog = false;
        OnPropertyChanged(nameof(IsMainMenuVisible));
    }

    private void OnMoveHovered(MoveSnapshot? snap)
    {
        SelectedSnapshot = snap;
    }

    public void ExecuteMoveKey(int index)
    {
        if (!IsMovesetVisible)
            return;

        var slots = new[]
        {
            MovesetChooser.Move0,
            MovesetChooser.Move1,
            MovesetChooser.Move2,
            MovesetChooser.Move3
        };

        if (index < 0 || index >= slots.Length)
            return;

        var slot = slots[index];

        if (slot.IsEnabled)
            slot.ClickCommand.Execute(null);
    }

    public void RefreshMoves(IReadOnlyList<MoveSnapshot> moves)
    {
        MovesetChooser.LoadMoves(moves);
    }

    // ─────────────────────────────────────────────────────────────
    // Lock / waiting
    // ─────────────────────────────────────────────────────────────

    public void SetWaitingForOpponent(bool waiting)
    {
        _waitingForOpponent = waiting;

        OnPropertyChanged(nameof(ActionsLocked));
        OnPropertyChanged(nameof(IsMainMenuVisible));

        NotifyInputChanged();
    }

    private void SetState(BattleUiState state)
    {
        UiState = state;

        OnPropertyChanged(nameof(IsLoggerActive));
        OnPropertyChanged(nameof(IsMenuActive));
        OnPropertyChanged(nameof(IsMovesetActive));
    }

    private void NotifyInputChanged()
    {
        OnPropertyChanged(nameof(AreInputsEnabled));
        OnPropertyChanged(nameof(ActionsLocked));
        OnPropertyChanged(nameof(IsMainMenuVisible));

        RefreshCommands();
    }

    private void RefreshCommands()
    {
        ((RelayCommand)OpenMovesetCommand).NotifyCanExecuteChanged();
        ((RelayCommand)OpenBagCommand).NotifyCanExecuteChanged();
        ((RelayCommand)ForfeitCommand).NotifyCanExecuteChanged();
        ((RelayCommand)OpenSwitchCommand).NotifyCanExecuteChanged();
        ((RelayCommand)DisconnectFromMatchCommand).NotifyCanExecuteChanged();
    }
}