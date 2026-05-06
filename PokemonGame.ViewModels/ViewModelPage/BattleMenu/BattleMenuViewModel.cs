using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Managers;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;

public class BattleMenuViewModel : ViewModelBase
{
    private readonly Action<int> _onMoveChosen;
    private readonly Action<int> _onSwitchChosen;
    private readonly Action _onForfeit;
    private readonly BattleManager _manager;
    private readonly BattleLoggerViewModel _logger;

    private bool _isWaitingForLog = false;

    public bool IsMainMenuVisible => !IsMovesetVisible && !_isWaitingForLog;

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

    public ICommand OpenMovesetCommand { get; }
    public ICommand CloseMovesetCommand { get; }
    public ICommand ForfeitCommand { get; }
    public ICommand OpenSwitchCommand { get; }

    public BattleMenuViewModel(
    Action<int> onMoveChosen,
    Action<int> onSwitchChosen,
    Action onForfeit,
    Action onSwitch,          // ← add this
    BattleManager manager,
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
            () => _logger.AreActionsUnlocked);

        CloseMovesetCommand = new RelayCommand(() =>
        {
            IsMovesetVisible = false;
            SelectedMove = null;
        });

        ForfeitCommand = new RelayCommand(
            () => _onForfeit(),
            () => _logger.AreActionsUnlocked);

        OpenSwitchCommand = new RelayCommand(
        () => onSwitch(),
        () => _logger.AreActionsUnlocked);

        _logger.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BattleLoggerViewModel.AreActionsUnlocked))
            {
                if (_logger.AreActionsUnlocked)
                    _isWaitingForLog = false;

                OnPropertyChanged(nameof(IsMainMenuVisible));
                ((RelayCommand)OpenMovesetCommand).NotifyCanExecuteChanged();
                ((RelayCommand)ForfeitCommand).NotifyCanExecuteChanged();
                ((RelayCommand)OpenSwitchCommand).NotifyCanExecuteChanged();  // ← add this
            }
        };
    }

    

    public void RefreshMoves(IReadOnlyList<IMove> moves) => MovesetChooser.LoadMoves(moves);

    private void OnMoveButtonClicked(int index)
    {
        IsMovesetVisible = false;
        SelectedMove = null;
        _isWaitingForLog = true;
        OnPropertyChanged(nameof(IsMainMenuVisible));
        _onMoveChosen(index);
    }

    private void OnMoveHovered(IMove? move) => SelectedMove = move;
}