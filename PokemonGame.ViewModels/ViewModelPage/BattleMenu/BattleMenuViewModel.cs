using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;

public class BattleMenuViewModel : ViewModelBase
{
    private readonly Action<int> _onMoveChosen;
    private readonly Action<int> _onSwitchChosen;
    private readonly Action _onForfeit;
    private readonly BattleManager? _manager;
    private readonly BattleLoggerViewModel _logger;

    private bool _isWaitingForLog = false;
    private bool _waitingForOpponent = false;

    public bool IsMainMenuVisible => !IsMovesetVisible && !_isWaitingForLog && !_waitingForOpponent;

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
        BattleManager? manager,
        BattleLoggerViewModel logger)
    {
        _onMoveChosen = onMoveChosen;
        _onSwitchChosen = onSwitchChosen;
        _onForfeit = onForfeit;
        _manager = manager;
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

        ForfeitCommand = new RelayCommand(
            () => _onForfeit(),
            () => AreInputsEnabled);

        OpenSwitchCommand = new RelayCommand(
            () => onSwitch(),
            () => AreInputsEnabled);

        _logger.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BattleLoggerViewModel.AreActionsUnlocked))
            {
                if (_logger.AreActionsUnlocked)
                    _isWaitingForLog = false;

                OnPropertyChanged(nameof(IsMainMenuVisible));
                NotifyInputChanged();
            }
        };
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

    private void OnMoveButtonClicked(int index)
    {
        IsMovesetVisible = false;
        SelectedMove = null;
        _isWaitingForLog = true;
        OnPropertyChanged(nameof(IsMainMenuVisible));
        _onMoveChosen(index);
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