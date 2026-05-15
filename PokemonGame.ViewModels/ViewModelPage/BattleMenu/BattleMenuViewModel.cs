using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Managers;
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
    public bool IsLoggerActive => UiState == BattleUiState.Logger;
    public bool IsMenuActive => UiState == BattleUiState.Menu;
    public bool IsMovesetActive => UiState == BattleUiState.Moveset;

    private readonly Func<int, Task> _onMoveChosen;
    private readonly Action<int> _onSwitchChosen;
    private readonly Action _onForfeit;
    private readonly BattleLoggerViewModel _logger;
    public ICommand MoveKeyCommand { get; }

    public ICommand LoggerNextCommand { get; }
    private bool _isWaitingForLog = false;
    private bool _waitingForOpponent = false;
    public BattleLoggerViewModel Logger => _logger;
    private BattleUiState _uiState = BattleUiState.Logger;
    public BattleUiState UiState
    {
        get => _uiState;
        set
        {
            if (SetProperty(ref _uiState, value))
            {
                OnPropertyChanged(nameof(IsLoggerActive));
                OnPropertyChanged(nameof(IsMenuActive));
                OnPropertyChanged(nameof(IsMovesetActive));
            }
        }
    }
    public ICommand OpenMenuCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand FightKeyCommand { get; }
    public bool IsMainMenuVisible =>
     !IsMovesetVisible &&
     !_isWaitingForLog &&
     !_waitingForOpponent;

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

    public BattlePokemonMovesetChooserViewModel MovesetChooser { get; }

    private MoveSnapshot? _selectedSnapshot;
    public MoveSnapshot? SelectedSnapshot
    {
        get => _selectedSnapshot;
        set
        {
            if (SetProperty(ref _selectedSnapshot, value))
            {
                OnPropertyChanged(nameof(SelectedMovePP));
                OnPropertyChanged(nameof(SelectedMoveType));
            }
        }
    }

    public string SelectedMovePP => _selectedSnapshot is MoveSnapshot s ? $"PP {s.PP}/{s.MaxPP}" : "PP --/--";
    public string SelectedMoveType => _selectedSnapshot is MoveSnapshot s2 ? $"TYPE/ {s2.Type}" : "TYPE/ --";

    public bool ActionsLocked => _waitingForOpponent || !_logger.AreActionsUnlocked;
    public bool AreInputsEnabled => !_isInputLocked && _logger.AreActionsUnlocked && !_waitingForOpponent;

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
    private int _menuSelectedIndex = 0;
    public int MenuSelectedIndex
    {
        get => _menuSelectedIndex;
        set
        {
            _menuSelectedIndex = (value + 4) % 4;
            OnPropertyChanged(nameof(MenuSelectedIndex));
            OnPropertyChanged(nameof(IsFightSelected));
            OnPropertyChanged(nameof(IsPokemonSelected));
            OnPropertyChanged(nameof(IsRunSelected));
        }
    }

    public bool IsFightSelected => MenuSelectedIndex == 0;
    public bool IsPokemonSelected => MenuSelectedIndex == 2;
    public bool IsRunSelected => MenuSelectedIndex == 3;
    private bool _isBackSelected;
    public bool IsBackSelected
    { 
        get => _isBackSelected;
        set => SetProperty(ref _isBackSelected, value);
    }

    public ICommand MenuNavigateCommand { get; }
    public ICommand MenuConfirmCommand { get; }
    public ICommand OpenMovesetCommand { get; }
    public ICommand CloseMovesetCommand { get; }
    public ICommand ForfeitCommand { get; }
    public ICommand OpenSwitchCommand { get; }
    public ICommand MoveSelectCommand { get; }
    public ICommand ConfirmMoveCommand { get; }

    public BattleMenuViewModel(
        Func<int, Task> onMoveChosen,
        Action<int> onSwitchChosen,
        Action onForfeit,
        Action onSwitch,
        BattleLoggerViewModel logger)
    {
        _onMoveChosen = onMoveChosen;
        _onSwitchChosen = onSwitchChosen;
        _onForfeit = onForfeit;
        _logger = logger;

        MovesetChooser = new BattlePokemonMovesetChooserViewModel(
            OnMoveButtonClicked,
            OnMoveHovered,
            () => IsBackSelected = true,
            () => IsBackSelected = false,
            logger);

        OpenMovesetCommand = new RelayCommand(
            () => IsMovesetVisible = true,
            () => AreInputsEnabled);

        CloseMovesetCommand = new RelayCommand(() =>
        {
            IsMovesetVisible = false;
            _selectedSnapshot = null;
            OnPropertyChanged(nameof(SelectedMovePP));
            OnPropertyChanged(nameof(SelectedMoveType));
        });
        LoggerNextCommand = new RelayCommand(_logger.ShowNext,() => logger.HasMore);
        ForfeitCommand = new RelayCommand(
             () => _onForfeit(),
             () => AreInputsEnabled);

        OpenSwitchCommand = new RelayCommand(
            () => onSwitch(),
            () => AreInputsEnabled);
        // RUN - no CanExecute guard
        OpenMenuCommand = new RelayCommand(() =>
        {
            IsMovesetVisible = true;
            MovesetChooser.ResetSelection();
            SetState(BattleUiState.Moveset);
        }, () => AreInputsEnabled);
        MoveSelectCommand = new RelayCommand<string>(param =>
        {
            if (IsMovesetVisible)
            {
                switch (param)
                {
                    case "Left": MovesetChooser.MoveSelection(-1, 0); break;
                    case "Right": MovesetChooser.MoveSelection(1, 0); break;
                    case "Up": MovesetChooser.MoveSelection(0, -1); break;
                    case "Down": MovesetChooser.MoveSelection(0, 1); break;
                }
            }
            else if (AreInputsEnabled)
            {
                MenuNavigateCommand.Execute(param);
            }
        });
        MenuNavigateCommand = new RelayCommand<string>(param =>
        {
            if (IsMovesetVisible || !AreInputsEnabled) return;
            // 2x2: FIGHT=0, BAG=1, POKEMON=2, RUN=3
            int dx = param == "Right" ? 1 : param == "Left" ? -1 : 0;
            int dy = param == "Down" ? 1 : param == "Up" ? -1 : 0;
            int col = (_menuSelectedIndex % 2 + dx + 2) % 2;
            int row = (_menuSelectedIndex / 2 + dy + 2) % 2;
            MenuSelectedIndex = row * 2 + col;
        });

        MenuConfirmCommand = new RelayCommand(() =>
        {
            if (IsMovesetVisible || !AreInputsEnabled) return;
            switch (MenuSelectedIndex)
            {
                case 0: OpenMenuCommand.Execute(null); break;
                case 2: OpenSwitchCommand.Execute(null); break;
                case 3: ForfeitCommand.Execute(null); break;
            }
        });
        ConfirmMoveCommand = new RelayCommand(() =>
        {
            if (IsMovesetVisible)
            {
                if (IsBackSelected)
                    CloseMovesetCommand.Execute(null);
                else
                    MovesetChooser.ConfirmSelection();
            }
            else if (AreInputsEnabled)
                MenuConfirmCommand.Execute(null);
            else if (_logger.HasMore)
                _logger.ShowNext();
        });

        FightKeyCommand = new RelayCommand(() =>
        {
            if (!AreInputsEnabled) return;
            SetState(BattleUiState.Moveset);
        });
        BackCommand = new RelayCommand(() =>
        {
            switch (UiState)
            {
                case BattleUiState.Moveset:
                    SetState(BattleUiState.Menu);
                    break;

                case BattleUiState.Menu:
                    SetState(BattleUiState.Logger);
                    break;
            }
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
                OnPropertyChanged(nameof(AreInputsEnabled));
                OnPropertyChanged(nameof(ActionsLocked));
                OnPropertyChanged(nameof(IsMainMenuVisible));

                RefreshCommands();
                CommandManager.InvalidateRequerySuggested(); // ← add this
            }
        };
        SetState(BattleUiState.Logger);
        RefreshCommands();
    }
    public void ExecuteMoveKey(int index)
    {
        if (!IsMovesetVisible) return;
        var slots = new[] { MovesetChooser.Move0, MovesetChooser.Move1, MovesetChooser.Move2, MovesetChooser.Move3 };
        var slot = slots[index];
        if (slot.IsEnabled)
            slot.ClickCommand.Execute(null);
    }

    public void SetWaitingForOpponent(bool waiting)
    {
        _waitingForOpponent = waiting;
        OnPropertyChanged(nameof(ActionsLocked));
        OnPropertyChanged(nameof(IsMainMenuVisible));
        NotifyInputChanged();
    }

    public void RefreshMoves(IReadOnlyList<MoveSnapshot> moves)
        => MovesetChooser.LoadMoves(moves);
    private void RefreshCommands()
    {
        ((RelayCommand)OpenMenuCommand).NotifyCanExecuteChanged();
        ((RelayCommand)ForfeitCommand).NotifyCanExecuteChanged();
        ((RelayCommand)OpenSwitchCommand).NotifyCanExecuteChanged();
        ((RelayCommand)OpenMovesetCommand).NotifyCanExecuteChanged();
    }

    private async void OnMoveButtonClicked(int index)
    {
        IsMovesetVisible = false;
        SelectedSnapshot = null;
        _isWaitingForLog = true;
        OnPropertyChanged(nameof(IsMainMenuVisible));

        await _onMoveChosen(index);  // ← awaited properly now

        _isWaitingForLog = false;
        OnPropertyChanged(nameof(IsMainMenuVisible));
    }

    private void SetState(BattleUiState state)
    {
        UiState = state;

        OnPropertyChanged(nameof(IsLoggerActive));
        OnPropertyChanged(nameof(IsMenuActive));
        OnPropertyChanged(nameof(IsMovesetActive));
    }
    private void OnMoveHovered(MoveSnapshot? snap)
    {
        _selectedSnapshot = snap;
        OnPropertyChanged(nameof(SelectedMovePP));
        OnPropertyChanged(nameof(SelectedMoveType));
    }
    private void NotifyInputChanged()
    {
        OnPropertyChanged(nameof(AreInputsEnabled));
        OnPropertyChanged(nameof(ActionsLocked));
        ((RelayCommand)OpenMovesetCommand).NotifyCanExecuteChanged();
        ((RelayCommand)ForfeitCommand).NotifyCanExecuteChanged();
        ((RelayCommand)OpenSwitchCommand).NotifyCanExecuteChanged();
    }
}