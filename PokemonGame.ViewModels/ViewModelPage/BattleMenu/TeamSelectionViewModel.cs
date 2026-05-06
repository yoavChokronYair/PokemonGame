using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Helper;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class PokemonSlotViewModel : ViewModelBase
    {
        private string _pokemonName = "";
        public string PokemonName
        {
            get => _pokemonName;
            set => SetProperty(ref _pokemonName, value);
        }

        private int _level;
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        public string LevelText => $"Lv{Level}";

        private int _currentHp;
        public int CurrentHp
        {
            get => _currentHp;
            set
            {
                if (SetProperty(ref _currentHp, value))
                {
                    OnPropertyChanged(nameof(HpText));
                    OnPropertyChanged(nameof(HpPercentage));
                    OnPropertyChanged(nameof(HPColor));
                }
            }
        }

        private int _maxHp;
        public int MaxHp
        {
            get => _maxHp;
            set
            {
                if (SetProperty(ref _maxHp, value))
                {
                    OnPropertyChanged(nameof(HpText));
                    OnPropertyChanged(nameof(HpPercentage));
                    OnPropertyChanged(nameof(HPColor));
                }
            }
        }

        public string HpText => $"{CurrentHp}/ {MaxHp}";

        public double HpPercentage => MaxHp > 0
            ? MathHelper.Clamp((double)CurrentHp / MaxHp, 0, 1)
            : 0;

        public Brush HPColor
        {
            get
            {
                if (HpPercentage > 0.5) return new SolidColorBrush(Color.FromRgb(104, 208, 64));
                if (HpPercentage > 0.2) return new SolidColorBrush(Color.FromRgb(248, 208, 48));
                return new SolidColorBrush(Color.FromRgb(240, 64, 48));
            }
        }

        private int _pokedexId;
        public int PokedexId
        {
            get => _pokedexId;
            set => SetProperty(ref _pokedexId, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private bool _isEmpty = true;
        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        private string _gender = "";
        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        // slot index within the team — set by TeamSelectionViewModel on construction
        public int SlotIndex { get; set; }

        public ICommand SelectCommand { get; }

        public PokemonSlotViewModel(Action<PokemonSlotViewModel> onSelected)
        {
            SelectCommand = new RelayCommand(() =>
            {
                if (!IsEmpty) onSelected(this);
            });
        }
    }
    public class TeamSelectionViewModel : ViewModelBase
    {
        private readonly UserStore _userStore;
        private readonly NavigationStore _navigationStore;
        private readonly Func<ViewModelBase> _createCancelViewModel;

        public ObservableCollection<PokemonSlotViewModel> Slots { get; } = new();

        public ICommand CancelCommand { get; }
        public ICommand ConfirmSelectionCommand { get; }

        public TeamSelectionViewModel(
            UserStore userStore,
            NavigationStore navigationStore,
            Func<ViewModelBase> createCancelViewModel)
        {
            _userStore = userStore;
            _navigationStore = navigationStore;
            _createCancelViewModel = createCancelViewModel;

            CancelCommand = new NavigateCommand(navigationStore, createCancelViewModel);
            ConfirmSelectionCommand = new RelayCommand(ConfirmSelection, CanConfirmSelection);

            LoadTeam();
        }

        private void LoadTeam()
        {
            Slots.Clear();

            PokemonTeam? team = _userStore.BattleSesion.ResolvedPlayerTeam;
            int capacity = team?.getAllPokemonCount() ?? 0;

            for (int i = 0; i < 6; i++)
            {
                var slot = new PokemonSlotViewModel(OnSlotSelected) { SlotIndex = i };

                if (team != null && i < capacity)
                    MapPokemonToSlot(team.GetPokemonAt(i), slot);

                Slots.Add(slot);
            }

            if (Slots.Count > 0 && !Slots[0].IsEmpty)
                Slots[0].IsSelected = true;
        }

        private static void MapPokemonToSlot(PokemonState pokemon, PokemonSlotViewModel slot)
        {
            slot.PokedexId = pokemon.PokedexId;
            slot.PokemonName = pokemon.Name;
            slot.Level = pokemon.Level;
            slot.CurrentHp = pokemon.CurrentHP;
            slot.MaxHp = pokemon.MaxHP;
            slot.Gender = pokemon.gender.ToString() ?? "";
            slot.IsEmpty = false;
        }

        private void OnSlotSelected(PokemonSlotViewModel selected)
        {
            foreach (var slot in Slots)
                slot.IsSelected = false;

            selected.IsSelected = true;

            // Switch immediately on selection
            PokemonTeam? team = _userStore.BattleSesion.ResolvedPlayerTeam;
            if (team == null) return;

            team.SwitchTo(selected.SlotIndex);

            // Navigate back
            _navigationStore.CurrentViewModel = _createCancelViewModel();
        }

        private bool CanConfirmSelection()
        {
            return Slots.Any(s => s.IsSelected && !s.IsEmpty);
        }

        private void ConfirmSelection()
        {
            var selected = Slots.FirstOrDefault(s => s.IsSelected && !s.IsEmpty);
            if (selected == null) return;

            PokemonTeam? team = _userStore.BattleSesion.ResolvedPlayerTeam;
            if (team == null) return;

            // Switch the active pokemon to the selected slot
            team.SwitchTo(selected.SlotIndex);

            // Navigate back the same way cancel does
            _navigationStore.CurrentViewModel = _createCancelViewModel();
        }
    }

}