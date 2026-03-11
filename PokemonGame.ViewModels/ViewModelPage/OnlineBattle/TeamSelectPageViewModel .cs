using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class TeamSelectPageViewModel : ViewModelBase
    {
        private readonly TeamSelectService _handler;

        // --- Teams ---
        public ObservableCollection<TeamViewModel> Teams { get; }

        private TeamViewModel _selectedTeam;
        public TeamViewModel SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                if (_selectedTeam != value)
                {
                    _selectedTeam = value;
                    OnPropertyChanged(nameof(SelectedTeam));
                }
            }
        }

        // --- Slot selection ---
        private PokemonSlotViewModel _selectedSlot;
        public PokemonSlotViewModel SelectedSlot
        {
            get => _selectedSlot;
            set
            {
                if (_selectedSlot != value)
                {
                    _selectedSlot = value;
                    OnPropertyChanged(nameof(SelectedSlot));
                    ShowPokemonPanel = _selectedSlot != null;
                }
            }
        }

        // --- Pokemon list with filtering ---
        private readonly ObservableCollection<PokemonViewModel> _allPokemon;
        public ICollectionView PokemonView { get; }

        // --- Search ---
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    PokemonView.Refresh();
                }
            }
        }

        // --- Type filter ---
        public ObservableCollection<string> PokemonTypes { get; }
        private string _selectedType = string.Empty;

        private bool _showFilters;
        public bool ShowFilters
        {
            get => _showFilters;
            set
            {
                if (_showFilters != value)
                {
                    _showFilters = value;
                    OnPropertyChanged(nameof(ShowFilters));
                }
            }
        }

        // --- Panel visibility ---
        private bool _showPokemonPanel;
        public bool ShowPokemonPanel
        {
            get => _showPokemonPanel;
            set
            {
                if (_showPokemonPanel != value)
                {
                    _showPokemonPanel = value;
                    OnPropertyChanged(nameof(ShowPokemonPanel));
                }
            }
        }

        // --- Commands ---
        public ICommand SelectSlotCommand { get; }
        public ICommand PlacePokemonCommand { get; }
        public ICommand ClearSlotCommand { get; }
        public ICommand PreviousTeamCommand { get; }
        public ICommand NextTeamCommand { get; }
        public ICommand ToggleFiltersCommand { get; }
        public ICommand FilterPokemonCommand { get; }

        public TeamSelectPageViewModel(UserStore userStore)
        {
            _handler = new TeamSelectService();

            // Initialize teams using the logged-in player's name
            Teams = new ObservableCollection<TeamViewModel>
            {
                new TeamViewModel($"{userStore.Username} - Team 1"),
                new TeamViewModel($"{userStore.Username} - Team 2"),
                new TeamViewModel($"{userStore.Username} - Team 3")
            };
            _selectedTeam = Teams[0];

            // Load all pokemon from DB
            _allPokemon = new ObservableCollection<PokemonViewModel>();
            LoadPokemon();

            // Setup filtered collection view
            PokemonView = CollectionViewSource.GetDefaultView(_allPokemon);
            PokemonView.Filter = FilterPokemon;

            // Type names matching the int Type index in BaseStatsData
            PokemonTypes = new ObservableCollection<string>
            {
                "Normal", "Fire", "Water", "Electric", "Grass",
                "Ice", "Fighting", "Poison", "Ground", "Flying",
                "Psychic", "Bug", "Rock", "Ghost", "Dragon",
                "Dark", "Steel", "Fairy"
            };

            // Wire commands
            SelectSlotCommand = new RelayCommand<PokemonSlotViewModel>(OnSelectSlot);
            PlacePokemonCommand = new RelayCommand<PokemonViewModel>(OnPlacePokemon);
            ClearSlotCommand = new RelayCommand(OnClearSlot);
            PreviousTeamCommand = new RelayCommand(OnPreviousTeam);
            NextTeamCommand = new RelayCommand(OnNextTeam);
            ToggleFiltersCommand = new RelayCommand(OnToggleFilters);
            FilterPokemonCommand = new RelayCommand<string>(OnFilterByType);
        }

        // --- Load all pokemon from DB via service ---
        private void LoadPokemon()
        {
            var pokemonList = _handler.GetAllPokemon();
            _allPokemon.Clear();

            foreach (var p in pokemonList)
            {
                var stats = _handler.GetBaseStats(p.PokemonID);
                string typeName = stats != null ? stats.Type1 : "Normal";

                _allPokemon.Add(new PokemonViewModel(
                    p.SpeciesName,
                    $"pack://application:,,,/Resources/Sprites/{p.SpeciesName.ToLower()}.png",
                    typeName
                ));
            }
        }

        // --- Filter predicate ---
        private bool FilterPokemon(object obj)
        {
            if (obj is not PokemonViewModel pokemon) return false;

            bool matchesSearch = string.IsNullOrWhiteSpace(SearchText)
                || pokemon.Name.ToLower().Contains(SearchText.ToLower());

            bool matchesType = string.IsNullOrWhiteSpace(_selectedType)
                || pokemon.Type == _selectedType;

            return matchesSearch && matchesType;
        }

        // --- Command handlers ---
        private void OnSelectSlot(PokemonSlotViewModel slot)
        {
            SelectedSlot = slot;
        }

        private void OnPlacePokemon(PokemonViewModel pokemon)
        {
            if (SelectedSlot == null || pokemon == null)
                return;

            SelectedSlot.Pokemon = pokemon;
            SelectedSlot = null;
            ShowPokemonPanel = false;
        }

        private void OnClearSlot()
        {
            if (SelectedSlot == null) return;
            SelectedSlot.Pokemon = null;
            SelectedSlot = null;
            ShowPokemonPanel = false;
        }

        private void OnPreviousTeam()
        {
            int index = Teams.IndexOf(SelectedTeam);
            if (index > 0)
                SelectedTeam = Teams[index - 1];
        }

        private void OnNextTeam()
        {
            int index = Teams.IndexOf(SelectedTeam);
            if (index < Teams.Count - 1)
                SelectedTeam = Teams[index + 1];
        }

        private void OnToggleFilters()
        {
            ShowFilters = !ShowFilters;
            if (!ShowFilters)
            {
                _selectedType = string.Empty;
                PokemonView.Refresh();
            }
        }

        private void OnFilterByType(string type)
        {
            _selectedType = _selectedType == type ? string.Empty : type;
            PokemonView.Refresh();
        }
    }
}