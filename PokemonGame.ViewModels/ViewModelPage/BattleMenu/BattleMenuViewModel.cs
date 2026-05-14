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

    private readonly Action<int> _onMoveChosen;
    private readonly Action<int> _onSwitchChosen;
    private readonly Action _onForfeit;
    private readonly BattleLoggerViewModel _logger;
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

    private IMove? _selectedMove;
    public IMove? SelectedMove
    {
        get => _selectedMove;
        set
        {
            if (SetProperty(ref _selectedMove, value))
            {
                OnPropertyChanged(nameof(SelectedMovePP));
                OnPropertyChanged(nameof(SelectedMoveType));
            }
        }
    }

    public string SelectedMovePP => SelectedMove is MoveState ms ? $"PP {ms.PP}/{ms.MaxPP}" : "PP --/--";
    public string SelectedMoveType => SelectedMove is MoveState ms2 ? $"TYPE/ {ms2.Element}" : "TYPE/ --";

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

    public ICommand OpenMovesetCommand { get; }
    public ICommand CloseMovesetCommand { get; }
    public ICommand ForfeitCommand { get; }
    public ICommand OpenSwitchCommand { get; }

    public BattleMenuViewModel(
        Action<int> onMoveChosen,
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
            logger);

        OpenMovesetCommand = new RelayCommand(
            () => IsMovesetVisible = true,
            () => AreInputsEnabled);

        CloseMovesetCommand = new RelayCommand(() =>
        {
            IsMovesetVisible = false;
            SelectedMove = null;
        });
        LoggerNextCommand = new RelayCommand(_logger.ShowNext,() => logger.HasMore);
        ForfeitCommand = new RelayCommand(
             () => _onForfeit(),
             () => AreInputsEnabled);

        OpenSwitchCommand = new RelayCommand(
            () => onSwitch(),
            () => AreInputsEnabled);
        OpenMenuCommand = new RelayCommand(() =>
        {
            SetState(BattleUiState.Moveset);
        }, () => AreInputsEnabled);
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
        _logger.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BattleLoggerViewModel.AreActionsUnlocked))
            {
                OnPropertyChanged(nameof(AreInputsEnabled));
                OnPropertyChanged(nameof(ActionsLocked));
                OnPropertyChanged(nameof(IsMainMenuVisible));

                RefreshCommands();
            }
        };
        SetState(BattleUiState.Logger);
        RefreshCommands();
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
        SelectedMove = null;

        _isWaitingForLog = true;

        SetState(BattleUiState.Logger);

        _onMoveChosen(index);

        // wait until logger finishes typing EVERYTHING
        await _logger.WaitUntilQueueEmpty();

        _isWaitingForLog = false;

        SetState(BattleUiState.Menu);
    }

    private void SetState(BattleUiState state)
    {
        UiState = state;

        OnPropertyChanged(nameof(IsLoggerActive));
        OnPropertyChanged(nameof(IsMenuActive));
        OnPropertyChanged(nameof(IsMovesetActive));
    }
    private void OnMoveHovered(IMove? move) => SelectedMove = move;

    private void NotifyInputChanged()
    {
        OnPropertyChanged(nameof(AreInputsEnabled));
        OnPropertyChanged(nameof(ActionsLocked));
        ((RelayCommand)OpenMovesetCommand).NotifyCanExecuteChanged();
        ((RelayCommand)ForfeitCommand).NotifyCanExecuteChanged();
        ((RelayCommand)OpenSwitchCommand).NotifyCanExecuteChanged();
    }
}