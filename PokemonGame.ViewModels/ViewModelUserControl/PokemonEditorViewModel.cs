using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    public class PokemonEditorViewModel : ViewModelBase
    {
        private readonly TeamBuilderState _state;

        public TeamSlotEntry SelectedPokemon => _state.SelectedPokemon;

        public List<string> NatureOptions { get; } = new List<string>
    {
        "Hardy", "Lonely", "Brave", "Adamant", "Naughty",
        "Bold", "Docile", "Relaxed", "Impish", "Lax",
        "Timid", "Hasty", "Serious", "Jolly", "Naive",
        "Modest", "Mild", "Quiet", "Bashful", "Rash",
        "Calm", "Gentle", "Sassy", "Careful", "Quirky"
    };
        public List<string> GenderOptions { get; } = new List<string> { "—", "♂", "♀" };

        public RelayCommand ToggleEvIvCommand { get; }
        public RelayCommand OpenMoveSlot1Command { get; }
        public RelayCommand OpenMoveSlot2Command { get; }
        public RelayCommand OpenMoveSlot3Command { get; }
        public RelayCommand OpenMoveSlot4Command { get; }
        public RelayCommand OpenItemPickerCommand { get; }
        public RelayCommand OpenPokemonPickerCommand { get; }

        public PokemonEditorViewModel(TeamBuilderState state, TeamBuilderService service)
        {
            _state = state;
            _state.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TeamBuilderState.SelectedPokemon))
                {
                    OnPropertyChanged(nameof(SelectedPokemon));
                }
            };

            ToggleEvIvCommand = new RelayCommand(() =>
            {
                _state.IsEvIvPanelOpen = !_state.IsEvIvPanelOpen;
                if (_state.IsEvIvPanelOpen)
                {
                    _state.IsMovePickerOpen = false;
                    _state.IsItemPickerOpen = false;
                    _state.IsPokemonPickerOpen = false;
                }
            });

            OpenMoveSlot1Command = new RelayCommand(() => OpenMoveSlot(1));
            OpenMoveSlot2Command = new RelayCommand(() => OpenMoveSlot(2));
            OpenMoveSlot3Command = new RelayCommand(() => OpenMoveSlot(3));
            OpenMoveSlot4Command = new RelayCommand(() => OpenMoveSlot(4));

            OpenItemPickerCommand = new RelayCommand(() =>
            {
                _state.IsItemPickerOpen = !_state.IsItemPickerOpen;
                if (_state.IsItemPickerOpen)
                {
                    _state.IsEvIvPanelOpen = false;
                    _state.IsMovePickerOpen = false;
                    _state.IsPokemonPickerOpen = false;
                }
            });

            OpenPokemonPickerCommand = new RelayCommand(() =>
            {
                _state.IsPokemonPickerOpen = !_state.IsPokemonPickerOpen;
                if (_state.IsPokemonPickerOpen)
                {
                    _state.IsEvIvPanelOpen = false;
                    _state.IsMovePickerOpen = false;
                    _state.IsItemPickerOpen = false;
                }
            });
        }

        private void OpenMoveSlot(int slot)
        {
            _state.ActiveMoveSlot = slot;
            _state.IsMovePickerOpen = true;
            _state.IsEvIvPanelOpen = false;
            _state.IsItemPickerOpen = false;
            _state.IsPokemonPickerOpen = false;
        }
    }
}
