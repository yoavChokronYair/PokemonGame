using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace PokemonGame.ViewModels.OnlineBattle
{
    public partial class TeamSelectViewModel : ObservableObject
    {
        // --- Teams ---
        [ObservableProperty] private TeamViewModel _selectedTeam;
        [ObservableProperty] private PokemonSlotViewModel _selectedSlot;

        // --- Pokémon ---
        public ObservableCollection<TeamViewModel> Teams { get; }
        public ICommand ClearSlotCommand { get; }

        public ObservableCollection<PokemonViewModel> AvailablePokemon { get; }
        public ICollectionView PokemonView { get; }

        // --- UI Control ---
        [ObservableProperty] private bool _showFilters = false; // toggled by filter button
        [ObservableProperty] private bool _showPokemonPanel = false; // show Pokémon only after slot selected

        // --- Search / Filter ---
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedType = "All";
        public ObservableCollection<string> PokemonTypes { get; }

        // --- Commands ---
        public ICommand SelectSlotCommand { get; }
        public ICommand PlacePokemonCommand { get; }
        public ICommand NextTeamCommand { get; }
        public ICommand PreviousTeamCommand { get; }
        public ICommand FilterPokemonCommand { get; }
        public ICommand ToggleFiltersCommand { get; }

        public TeamSelectViewModel()
        {
            // Initialize Teams
             Teams = new ObservableCollection<TeamViewModel>
            {
                new TeamViewModel("Team 1"),
                new TeamViewModel("Team 2"),
                new TeamViewModel("Team 3")
            };
            SelectedTeam = Teams[0];
            ClearSlotCommand = new RelayCommand(() =>
            {
                if (SelectedSlot != null)
                {
                    SelectedSlot.Pokemon = null;
                    SelectedSlot = null;
                    ShowPokemonPanel = false; // hide Pokémon panel after clearing
                }
            }); 

            // Initialize Pokémon
            var allPokemon = new ObservableCollection<PokemonViewModel>
            {
                new("Pikachu", "pack://application:,,,/Assets/Images/GenOnePokemon/25.png", "Electric"),
                new("Charizard", "pack://application:,,,/Assets/Images/GenOnePokemon/6.png", "Fire"),
                new("Blastoise", "pack://application:,,,/Assets/Images/GenOnePokemon/9.png", "Water"),
                new("Venusaur", "pack://application:,,,/Assets/Images/GenOnePokemon/3.png", "Grass")
            };
            AvailablePokemon = allPokemon;


            // Setup CollectionView
            PokemonView = CollectionViewSource.GetDefaultView(allPokemon);
            PokemonView.Filter = FilterPokemon;

            // Commands
            SelectSlotCommand = new RelayCommand<PokemonSlotViewModel>(slot =>
            {
                SelectedSlot = slot;
                ShowPokemonPanel = true; // show Pokémon when a slot is selected
            });

            PlacePokemonCommand = new RelayCommand<PokemonViewModel>(pokemon =>
            {
                if (SelectedSlot != null && pokemon != null)
                {
                    SelectedSlot.Pokemon = pokemon;
                    SelectedSlot = null;
                    ShowPokemonPanel = false; // hide Pokémon panel after placing
                }
            });

            NextTeamCommand = new RelayCommand(() =>
            {
                int index = Teams.IndexOf(SelectedTeam);
                SelectedTeam = Teams[(index + 1) % Teams.Count];
            });

            PreviousTeamCommand = new RelayCommand(() =>
            {
                int index = Teams.IndexOf(SelectedTeam);
                SelectedTeam = Teams[(index - 1 + Teams.Count) % Teams.Count];
            });

            FilterPokemonCommand = new RelayCommand<string>(type =>
            {
                SelectedType = type;
                ShowFilters = false; // hide filter panel after selection
                PokemonView.Refresh();
            });

            ToggleFiltersCommand = new RelayCommand(() =>
            {
                ShowFilters = !ShowFilters; // toggle filters visibility
            });

            PokemonTypes = new ObservableCollection<string> { "All", "Electric", "Fire", "Water", "Grass" };
        }

        private bool FilterPokemon(object obj)
        {
            if (obj is PokemonViewModel p)
            {
                bool typeMatches = SelectedType == "All" || p.Type == SelectedType;
                bool searchMatches = string.IsNullOrWhiteSpace(SearchText) || p.Name.ToLower().Contains(SearchText.ToLower());
                return typeMatches && searchMatches;
            }
            return false;
        }

        partial void OnSearchTextChanged(string oldValue, string newValue)
        {
            PokemonView.Refresh();
        }
    }

    // --- Models ---
    public class TeamViewModel : ObservableObject
    {
        public string Name { get; }
        public ObservableCollection<PokemonSlotViewModel> Slots { get; }

        public TeamViewModel(string name)
        {
            Name = name;
            Slots = new ObservableCollection<PokemonSlotViewModel>();
            for (int i = 0; i < 6; i++)
                Slots.Add(new PokemonSlotViewModel());
        }
    }

    public class PokemonSlotViewModel : ObservableObject
    {
        private PokemonViewModel _pokemon;
        public PokemonViewModel Pokemon
        {
            get => _pokemon;
            set
            {
                _pokemon = value;
                OnPropertyChanged(nameof(Pokemon));
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public bool IsEmpty => Pokemon == null;
    }

    public class PokemonViewModel : ObservableObject
    {
        public string Name { get; }
        public string Sprite { get; }
        public string Type { get; }

        public PokemonViewModel(string name, string sprite, string type)
        {
            Name = name;
            Sprite = sprite;
            Type = type;
        }
    }
}
