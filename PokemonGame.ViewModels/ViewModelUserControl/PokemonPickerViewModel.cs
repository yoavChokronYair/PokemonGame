using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    public class PokemonPickerViewModel : ViewModelBase
    {
        private readonly TeamBuilderState _state;
        public ObservableCollection<PokemonDisplayEntry> AllPokemon { get; }

        private string _pokemonSearchText = string.Empty;
        public string PokemonSearchText
        {
            get => _pokemonSearchText;
            set
            {
                if (SetProperty(ref _pokemonSearchText, value))
                {
                    OnPropertyChanged(nameof(FilteredPokemons));
                }
            }
        }

        public IEnumerable<PokemonDisplayEntry> FilteredPokemons =>
            string.IsNullOrWhiteSpace(PokemonSearchText)
                ? AllPokemon
                : AllPokemon.Where(i => i.Name.IndexOf(PokemonSearchText, StringComparison.OrdinalIgnoreCase) >= 0);

        private PokemonDisplayEntry _pickerPokemon;
        public PokemonDisplayEntry PickerPokemon
        {
            get => _pickerPokemon;
            set => SetProperty(ref _pickerPokemon, value);
        }

        public RelayCommand<PokemonDisplayEntry> ConfirmPokemonCommand { get; }

        public PokemonPickerViewModel(TeamBuilderState state,
            ObservableCollection<PokemonDisplayEntry> allPokemon)
        {
            _state = state;
            AllPokemon = allPokemon;
            _state.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TeamBuilderState.IsPokemonPickerOpen) && _state.IsPokemonPickerOpen)
                {
                    PokemonSearchText = string.Empty;
                    OnPropertyChanged(nameof(FilteredPokemons));
                }
            };

            ConfirmPokemonCommand = new RelayCommand<PokemonDisplayEntry>(pokemon =>
            {
                if (pokemon == null)
                {
                    return;
                }

                var newSlot = new TeamSlotEntry(pokemon);

                if (_state.SelectedSlotIndex >= 0 && _state.SelectedSlotIndex < 6
                    && _state.TeamSlots[_state.SelectedSlotIndex] == null)
                {
                    _state.TeamSlots[_state.SelectedSlotIndex] = newSlot;
                }

                _state.SelectedPokemon = newSlot;
                _state.CloseAllPanels();
            });
        }
    }
}
