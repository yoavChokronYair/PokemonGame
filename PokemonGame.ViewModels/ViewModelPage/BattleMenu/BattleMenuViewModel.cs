using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Battle;
using PokemonGame.Model.Model.Helper.MoveHelper;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class BattleMenuViewModel : ViewModelBase
    {
        private readonly Action<int> _onMoveChosen;
        private readonly Action<int> _onSwitchChosen;
        private readonly BattleManager _manager;

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

        public bool IsMainMenuVisible => !IsMovesetVisible;

        // ── Moveset chooser child VM ─────────────────────────────────────────
        public BattlePokemonMovesetChooserViewModel MovesetChooser { get; }

        // ── Selected move info (shown in the right panel) ────────────────────
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

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand OpenMovesetCommand { get; }
        public ICommand CloseMovesetCommand { get; }

        public BattleMenuViewModel(Action<int> onMoveChosen, Action<int> onSwitchChosen, BattleManager manager)
        {
            _onMoveChosen = onMoveChosen;
            _onSwitchChosen = onSwitchChosen;
            _manager = manager;

            MovesetChooser = new BattlePokemonMovesetChooserViewModel(OnMoveButtonClicked, OnMoveHovered);

            OpenMovesetCommand = new RelayCommand(() => IsMovesetVisible = true);
            CloseMovesetCommand = new RelayCommand(() => { IsMovesetVisible = false; SelectedMove = null; });
        }

        public void RefreshMoves(IReadOnlyList<IMove> moves)
        {
            MovesetChooser.LoadMoves(moves);
        }

        private void OnMoveButtonClicked(int index)
        {
            IsMovesetVisible = false;
            SelectedMove = null;
            _onMoveChosen(index);
        }

        private void OnMoveHovered(IMove? move)
        {
            SelectedMove = move;
        }
    }
}
