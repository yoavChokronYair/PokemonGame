using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Battle;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class BattleMenuViewModel : ViewModelBase
    {
        private readonly Action<int> _onMoveChosen;
        private readonly Action<int> _onSwitchChosen;
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
                {
                    OnPropertyChanged(nameof(IsMainMenuVisible));
                }
            }
        }


        // ── Moveset chooser child VM ──────────────────────────────────────────
        public BattlePokemonMovesetChooserViewModel MovesetChooser { get; }

        // ── Selected move info panel ──────────────────────────────────────────
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

        // ── Commands ──────────────────────────────────────────────────────────
        public ICommand OpenMovesetCommand { get; }
        public ICommand CloseMovesetCommand { get; }

        public BattleMenuViewModel(
            Action<int> onMoveChosen,
            Action<int> onSwitchChosen,
            BattleManager manager,
            BattleLoggerViewModel logger)
        {
            _onMoveChosen = onMoveChosen;
            _onSwitchChosen = onSwitchChosen;
            _manager = manager;
            _logger = logger;

            MovesetChooser = new BattlePokemonMovesetChooserViewModel(
                OnMoveButtonClicked,
                OnMoveHovered,
                logger);

            OpenMovesetCommand = new RelayCommand(
                () => IsMovesetVisible = true,
                // Can't open moveset while log messages are pending
                () => _logger.AreActionsUnlocked);

            CloseMovesetCommand = new RelayCommand(() =>
            {
                IsMovesetVisible = false;
                SelectedMove = null;
            });

            // When the logger drains its queue, re-evaluate the open command
            _logger.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BattleLoggerViewModel.AreActionsUnlocked))
                {
                    // Queue just drained — re-show the main menu
                    if (_logger.AreActionsUnlocked)
                    {
                        _isWaitingForLog = false;
                    }

                    OnPropertyChanged(nameof(IsMainMenuVisible));
                    ((RelayCommand)OpenMovesetCommand).NotifyCanExecuteChanged();
                }
            };
        }

        public void RefreshMoves(IReadOnlyList<IMove> moves)
        {
            MovesetChooser.LoadMoves(moves);
        }

        private void OnMoveButtonClicked(int index)
        {
            IsMovesetVisible = false;
            SelectedMove = null;

            // Also hide the main menu until the log queue drains
            _isWaitingForLog = true;
            OnPropertyChanged(nameof(IsMainMenuVisible));

            _onMoveChosen(index);
        }

        private void OnMoveHovered(IMove? move)
        {
            SelectedMove = move;
        }
    }
}