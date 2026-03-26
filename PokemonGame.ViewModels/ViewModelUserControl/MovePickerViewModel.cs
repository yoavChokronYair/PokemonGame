using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    public class MovePickerViewModel : ViewModelBase
    {
        private readonly TeamBuilderState _state;
        public TeamSlotEntry SelectedPokemon => _state.SelectedPokemon;

        public int ActiveMoveSlot => _state.ActiveMoveSlot;

        private string _moveSearchText = string.Empty;
        public string MoveSearchText
        {
            get => _moveSearchText;
            set
            {
                if (SetProperty(ref _moveSearchText, value))
                {
                    OnPropertyChanged(nameof(FilteredMoves));
                }
            }
        }

        public IEnumerable<MoveDisplayEntry> FilteredMoves =>
            string.IsNullOrWhiteSpace(MoveSearchText)
                ? _state.SelectedPokemon?.AvailableMoves ?? Enumerable.Empty<MoveDisplayEntry>()
                : _state.SelectedPokemon?.AvailableMoves
                    .Where(m => m.Name.IndexOf(MoveSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                  ?? Enumerable.Empty<MoveDisplayEntry>();

        private MoveDisplayEntry _selectedMove;
        public MoveDisplayEntry SelectedMove
        {
            get => _selectedMove;
            set
            {
                if (SetProperty(ref _selectedMove, value) && value != null)
                {
                    ConfirmMoveCommand.Execute(null);
                }
            }
        }

        public RelayCommand ConfirmMoveCommand { get; }

        public MovePickerViewModel(TeamBuilderState state, TeamBuilderService service)
        {
            _state = state;
            _state.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TeamBuilderState.ActiveMoveSlot))
                {
                    OnPropertyChanged(nameof(ActiveMoveSlot));
                }

                if (e.PropertyName == nameof(TeamBuilderState.SelectedPokemon))
                {
                    OnPropertyChanged(nameof(SelectedPokemon));
                    OnPropertyChanged(nameof(FilteredMoves));
                }
                if (e.PropertyName == nameof(TeamBuilderState.IsMovePickerOpen) && _state.IsMovePickerOpen)
                {
                    _selectedMove = null;
                    MoveSearchText = string.Empty;
                    OnPropertyChanged(nameof(SelectedMove));
                    OnPropertyChanged(nameof(FilteredMoves));
                }
            };

            ConfirmMoveCommand = new RelayCommand(() =>
            {
                if (_state.SelectedPokemon == null || SelectedMove == null)
                {
                    return;
                }

                switch (_state.ActiveMoveSlot)
                {
                    case 1: _state.SelectedPokemon.Move1 = SelectedMove; break;
                    case 2: _state.SelectedPokemon.Move2 = SelectedMove; break;
                    case 3: _state.SelectedPokemon.Move3 = SelectedMove; break;
                    case 4: _state.SelectedPokemon.Move4 = SelectedMove; break;
                }
                _selectedMove = null;
                OnPropertyChanged(nameof(SelectedMove));
                _state.IsMovePickerOpen = false;
            });
        }
    }
}
