using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    public class EvIvEditorViewModel : ViewModelBase
    {
        private readonly TeamBuilderState _state;

        public TeamSlotEntry SelectedPokemon => _state.SelectedPokemon;

        public List<string> IvSpreadOptions { get; } = new List<string>
        {
            "max all", "min Atk", "min Atk, min Spe", "min Spe", "min all"
        };
        public List<string> NatureOptions { get; } = new List<string>
        {
            "Hardy",  "Lonely", "Brave",   "Adamant", "Naughty",
            "Bold",   "Docile", "Relaxed", "Impish",  "Lax",
            "Timid",  "Hasty",  "Serious", "Jolly",   "Naive",
            "Modest", "Mild",   "Quiet",   "Bashful", "Rash",
            "Calm",   "Gentle", "Sassy",   "Careful", "Quirky"
        };
        private string _selectedIvSpread;
        public string SelectedIvSpread
        {
            get => _selectedIvSpread;
            set
            {
                if (SetProperty(ref _selectedIvSpread, value) && value != null)
                {
                    _state.SelectedPokemon?.ApplyIvSpread(value);
                }
            }
        }

        public RelayCommand<string> ApplyIvSpreadCommand { get; }

        public EvIvEditorViewModel(TeamBuilderState state)
        {
            _state = state;
            _state.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TeamBuilderState.SelectedPokemon))
                {
                    OnPropertyChanged(nameof(SelectedPokemon));
                }
            };

            ApplyIvSpreadCommand = new RelayCommand<string>(spread =>
            {
                if (spread == null)
                {
                    return;
                }

                _state.SelectedPokemon?.ApplyIvSpread(spread);
            });
        }
    }
}
