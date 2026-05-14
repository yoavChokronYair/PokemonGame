using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Helper;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class TeamSelectionOptions
    {
        public bool CanSwitch { get; set; } = true;
        public bool CanMove { get; set; } = true;
        public bool CanSummary { get; set; } = true;
        public bool IsUsingUserStore { get; set; } = true;
    }
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

        public int SlotIndex { get; set; }

        public ICommand SelectCommand { get; }

        public PokemonSlotViewModel(Action<PokemonSlotViewModel> onSelected)
        {
            SelectCommand = new RelayCommand(() =>
            {
                if (!IsEmpty)
                    onSelected(this);
            });
        }
    }

    public class TeamSelectionViewModel : ViewModelBase
    {
        private readonly UserStore _userStore;
        private readonly NavigationStore _navigationStore;
        private readonly Func<ViewModelBase> _createCancelViewModel;
        private readonly bool _switchImmediately;
        private TeamSelectionOptions _options;
        public TeamSelectionOptions Options { get => _options; set => SetProperty(ref _options, value);}

        public ICommand ConfirmSelectionCommand { get; }
        // NEW
        private readonly Action<int> _onSwitchChosen;

        public ObservableCollection<PokemonSlotViewModel> Slots { get; } = new();
        private bool _isActionMenuOpen;
        public bool IsActionMenuOpen
        {
            get => _isActionMenuOpen;
            set
            {
                if (SetProperty(ref _isActionMenuOpen, value))
                {
                    OnPropertyChanged(nameof(IsSwitchSelected));
                    OnPropertyChanged(nameof(IsMoveSelected));
                    OnPropertyChanged(nameof(IsSummarySelected));
                    OnPropertyChanged(nameof(IsCancelSelected));
                }
            }
        }
        private PokemonSlotViewModel? _pendingSwapSlot;
        private bool _isMoveMode;
        public bool IsMoveMode
        {
            get => _isMoveMode;
            set => SetProperty(ref _isMoveMode, value);
        }
        public bool IsSwitchSelected => IsActionMenuOpen && ActionMenuIndex == SwitchActionIndex;
        public bool IsMoveSelected => IsActionMenuOpen && ActionMenuIndex == MoveActionIndex;
        public bool IsSummarySelected => IsActionMenuOpen && ActionMenuIndex == SummaryActionIndex;
        public bool IsCancelSelected => IsActionMenuOpen && ActionMenuIndex == CancelActionIndex;

        private int _actionMenuIndex;
        public int ActionMenuIndex
        {
            get => _actionMenuIndex;
            set
            {
                if (SetProperty(ref _actionMenuIndex, value))
                {
                    OnPropertyChanged(nameof(IsSwitchSelected));
                    OnPropertyChanged(nameof(IsMoveSelected));
                    OnPropertyChanged(nameof(IsSummarySelected));
                    OnPropertyChanged(nameof(IsCancelSelected));
                }
            }
        }

        public ICommand StartMoveCommand { get; }
        public ICommand CompleteMoveCommand { get; }

        public ICommand SwitchCommand { get; }
        public ICommand MovePokemonCommand { get; }
        public ICommand OpenSummaryCommand { get; }
        public ICommand CloseActionMenuCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SelectNextSlotCommand { get; }
        public ICommand SelectPreviousSlotCommand { get; }
        public ICommand ConfirmCurrentSelectionCommand { get; }
        public ICommand ActionSwitchCommand { get; }
        public ICommand ActionMoveCommand { get; }
        public ICommand ActionSummaryCommand { get; }

        public TeamSelectionViewModel(
            UserStore userStore,
            NavigationStore navigationStore,
            Func<ViewModelBase> createCancelViewModel,
            TeamSelectionOptions options = null,
            Action<int>? onSwitchChosen = null,
            bool switchImmediately = false
            )
        {
            Options = options ?? new TeamSelectionOptions();
            _userStore = userStore;
            _navigationStore = navigationStore;
            _createCancelViewModel = createCancelViewModel;

            _onSwitchChosen = onSwitchChosen;
            _switchImmediately = switchImmediately;

            CancelCommand = new RelayCommand(() =>
            {
                if (IsActionMenuOpen)
                    CloseActionMenu();
                else
                    _navigationStore.CurrentViewModel = _createCancelViewModel();
            });

            ConfirmSelectionCommand = new RelayCommand(
                ConfirmSelection,
                CanConfirmSelection);
            SwitchCommand = new RelayCommand<PokemonSlotViewModel>(OnSwitch);
            MovePokemonCommand = new RelayCommand<PokemonSlotViewModel>(OnMovePokemon);
            OpenSummaryCommand = new RelayCommand<PokemonSlotViewModel>(OnSummary);
            CloseActionMenuCommand = new RelayCommand(CloseActionMenu);
            SelectNextSlotCommand = new RelayCommand(SelectNextSlot);
            SelectPreviousSlotCommand = new RelayCommand(SelectPreviousSlot);
            ConfirmCurrentSelectionCommand = new RelayCommand(ConfirmCurrentSelection);
            StartMoveCommand = new RelayCommand<PokemonSlotViewModel>(StartMove);
            CompleteMoveCommand = new RelayCommand<PokemonSlotViewModel>(CompleteMove);
            ActionSwitchCommand = new RelayCommand(() =>
            {
                var selected = Slots.FirstOrDefault(s => s.IsSelected && !s.IsEmpty);
                if (selected != null) OnSwitch(selected);
            });

            ActionMoveCommand = new RelayCommand(() =>
            {
                var selected = Slots.FirstOrDefault(s => s.IsSelected && !s.IsEmpty);
                if (selected != null) OnMovePokemon(selected);
            });

            ActionSummaryCommand = new RelayCommand(() =>
            {
                var selected = Slots.FirstOrDefault(s => s.IsSelected && !s.IsEmpty);
                if (selected != null) OnSummary(selected);
            });

            if (_options.IsUsingUserStore)
                LoadTeam(_userStore);
            else
                LoadTeam(PlayerDomain.Instance);
            
        }
        private void OnMovePokemon(PokemonSlotViewModel? slot)
        {
            if (slot == null) return;
            IsActionMenuOpen = false;
            StartMove(slot);
        }

        private void StartMove(PokemonSlotViewModel? slot)
        {
            if (slot == null) return;
            _pendingSwapSlot = slot;
            slot.IsSelected = true;
            IsMoveMode = true;
        }

        private void CompleteMove(PokemonSlotViewModel? slot)
        {
            if (slot == null || _pendingSwapSlot == null) return;
            if (slot == _pendingSwapSlot)
            {
                CancelMove();
                return;
            }

            // Swap the data
            int indexA = _pendingSwapSlot.SlotIndex;
            int indexB = slot.SlotIndex;

            // Swap all visual data between the two slots
            SwapSlotData(_pendingSwapSlot, slot);

            // Swap in the actual domain model too
            if (_options.IsUsingUserStore) 
                _userStore.BattleSesion.ResolvedPlayerTeam?.SwapSlots(indexA, indexB);
            else
                PlayerDomain.Instance.Team?.SwapSlots(indexA, indexB);

            CancelMove();
        }

        private void CancelMove()
        {
            _pendingSwapSlot = null;
            IsMoveMode = false;
            foreach (var s in Slots) s.IsSelected = false;
        }

        private static void SwapSlotData(PokemonSlotViewModel a, PokemonSlotViewModel b)
        {
            (a.PokemonName, b.PokemonName) = (b.PokemonName, a.PokemonName);
            (a.Level, b.Level) = (b.Level, a.Level);
            (a.CurrentHp, b.CurrentHp) = (b.CurrentHp, a.CurrentHp);
            (a.MaxHp, b.MaxHp) = (b.MaxHp, a.MaxHp);
            (a.PokedexId, b.PokedexId) = (b.PokedexId, a.PokedexId);
            (a.Gender, b.Gender) = (b.Gender, a.Gender);
            (a.IsEmpty, b.IsEmpty) = (b.IsEmpty, a.IsEmpty);
            (a.SlotIndex, b.SlotIndex) = (b.SlotIndex, a.SlotIndex);
        }
        private void SelectNextSlot()
        {
            if (IsMoveMode)
            {
                // cycle to next slot to swap with
                var filled = Slots.Where(s => !s.IsEmpty).ToList();
                var current = filled.FirstOrDefault(s => s.IsSelected && s != _pendingSwapSlot);
                var next = current == null
                    ? filled.First(s => s != _pendingSwapSlot)
                    : filled[(filled.IndexOf(current) + 1) % filled.Count];
                if (next == _pendingSwapSlot)
                    next = filled[(filled.IndexOf(next) + 1) % filled.Count];
                foreach (var s in Slots) s.IsSelected = s == _pendingSwapSlot;
                next.IsSelected = true;
                return;
            }

            if (IsActionMenuOpen)
            {
                var actions = GetAvailableActions();
                if (actions.Count == 0) return;
                ActionMenuIndex = (ActionMenuIndex + 1) % actions.Count;
                return;
            }

            var filledSlots = Slots.Where(s => !s.IsEmpty).ToList();
            if (!filledSlots.Any()) return;
            var cur = filledSlots.FirstOrDefault(s => s.IsSelected);
            var nx = cur == null
                ? filledSlots[0]
                : filledSlots[(filledSlots.IndexOf(cur) + 1) % filledSlots.Count];
            foreach (var s in Slots) s.IsSelected = false;
            nx.IsSelected = true;
        }

        private void SelectPreviousSlot()
        {
            if (IsMoveMode)
            {
                var filled = Slots.Where(s => !s.IsEmpty).ToList();
                var current = filled.FirstOrDefault(s => s.IsSelected && s != _pendingSwapSlot);
                var prev = current == null
                    ? filled.Last(s => s != _pendingSwapSlot)
                    : filled[(filled.IndexOf(current) - 1 + filled.Count) % filled.Count];
                if (prev == _pendingSwapSlot)
                    prev = filled[(filled.IndexOf(prev) - 1 + filled.Count) % filled.Count];
                foreach (var s in Slots) s.IsSelected = s == _pendingSwapSlot;
                prev.IsSelected = true;
                return;
            }

            if (IsActionMenuOpen)
            {
                var actions = GetAvailableActions();
                if (actions.Count == 0) return;
                ActionMenuIndex = (ActionMenuIndex - 1 + actions.Count) % actions.Count;
                return;
            }

            var filledSlots = Slots.Where(s => !s.IsEmpty).ToList();
            if (!filledSlots.Any()) return;
            var cur = filledSlots.FirstOrDefault(s => s.IsSelected);
            var pr = cur == null
                ? filledSlots[1]
                : filledSlots[(filledSlots.IndexOf(cur) - 1 + filledSlots.Count) % filledSlots.Count];
            foreach (var s in Slots) s.IsSelected = false;
            pr.IsSelected = true;
        }

        private void ConfirmCurrentSelection()
        {
            if (IsMoveMode)
            {
                var target = Slots.FirstOrDefault(s => s.IsSelected && s != _pendingSwapSlot);
                if (target != null) CompleteMove(target);
                return;
            }

            if (IsActionMenuOpen)
            {
                var actions = GetAvailableActions();
                if (actions.Count == 0) return;
                actions[ActionMenuIndex].Invoke();
                return;
            }

            var selected = Slots.FirstOrDefault(s => s.IsSelected && !s.IsEmpty);
            if (selected == null) return;
            ActionMenuIndex = 0;
            OnSlotSelected(selected);
        }

        public int SwitchActionIndex => GetAvailableActionNames().IndexOf("SWITCH");
        public int MoveActionIndex => GetAvailableActionNames().IndexOf("MOVE");
        public int SummaryActionIndex => GetAvailableActionNames().IndexOf("SUMMARY");
        public int CancelActionIndex => GetAvailableActionNames().IndexOf("CANCEL");

        private List<string> GetAvailableActionNames()
        {
            var names = new List<string>();
            if (Options.CanSwitch) names.Add("SWITCH");
            if (Options.CanMove) names.Add("MOVE");
            if (Options.CanSummary) names.Add("SUMMARY");
            names.Add("CANCEL");
            return names;
        }

        private List<Action> GetAvailableActions()
        {
            var selected = Slots.FirstOrDefault(s => s.IsSelected && !s.IsEmpty);
            if (selected == null) return new();

            var actions = new List<Action>();
            if (Options.CanSwitch) actions.Add(() => OnSwitch(selected));
            if (Options.CanMove) actions.Add(() => OnMovePokemon(selected));
            if (Options.CanSummary) actions.Add(() => OnSummary(selected));
            actions.Add(CloseActionMenu);
            return actions;
        }
        private bool CanConfirmSelection()
        {
            return Slots.Any(s =>
                s.IsSelected &&
                !s.IsEmpty);
        }
        private void LoadTeam(UserStore userStore)
        {
            Slots.Clear();

            PokemonTeam? team =
                userStore.BattleSesion.ResolvedPlayerTeam;

            int capacity = team?.getAllPokemonCount() ?? 0;

            for (int i = 0; i < 6; i++)
            {
                var slot =
                    new PokemonSlotViewModel(OnSlotSelected)
                    {
                        SlotIndex = i
                    };

                if (team != null && i < capacity)
                    MapPokemonToSlot(team.GetPokemonAt(i), slot);

                Slots.Add(slot);
            }
        }
        private void LoadTeam(PlayerDomain player)
        {
            Slots.Clear();

            PlayerTeamDomain? team = player.Team;

            for (int i = 0; i < 6; i++)
            {
                var slot = new PokemonSlotViewModel(OnSlotSelected) { SlotIndex = i };

                var pokemon = team?.GetAt(i);  // returns null if empty, never throws
                if (pokemon != null)
                    MapPokemonToSlot(pokemon, slot);

                Slots.Add(slot);
            }
        }

        private static void MapPokemonToSlot(
            PokemonState pokemon,
            PokemonSlotViewModel slot)
        {
            slot.PokedexId = pokemon.PokedexId;
            slot.PokemonName = pokemon.Name;
            slot.Level = pokemon.Level;
            slot.CurrentHp = pokemon.CurrentHP;
            slot.MaxHp = pokemon.MaxHP;
            slot.Gender = pokemon.gender.ToString() ?? "";
            slot.IsEmpty = false;
        }
        private static void MapPokemonToSlot(
           PokemonPlayerDomain pokemon,
           PokemonSlotViewModel slot)
        {
            slot.PokedexId = pokemon.PokedexId;
            slot.PokemonName = pokemon.PokemonState.Name;
            slot.Level = pokemon.PokemonState.Level;
            slot.CurrentHp = pokemon.CurrentHP;
            slot.MaxHp = pokemon.PokemonState.MaxHP;
            slot.Gender = pokemon.PokemonState.gender.ToString() ?? "";
            slot.IsEmpty = false;
        }
        private void ConfirmSelection()
        {
            var selected =
                Slots.FirstOrDefault(s =>
                    s.IsSelected && !s.IsEmpty);

            if (selected == null)
                return;

            _onSwitchChosen?.Invoke(selected.SlotIndex);

            _navigationStore.CurrentViewModel =
                _createCancelViewModel();
        }
        private void OnSlotSelected(PokemonSlotViewModel selected)
        {
            foreach (var slot in Slots)
                slot.IsSelected = false;

            selected.IsSelected = true;

            IsActionMenuOpen = true;
        }
        private void OnSwitch(PokemonSlotViewModel? slot)
        {
            if (slot == null) return;

            if (_onSwitchChosen != null)
            {
                // Battle context — notify caller and navigate back
                _onSwitchChosen.Invoke(slot.SlotIndex);
                IsActionMenuOpen = false;
                _navigationStore.CurrentViewModel = _createCancelViewModel();
            }
            else
            {
                // No battle callback — this is team reorder, go to move mode
                IsActionMenuOpen = false;
                StartMove(slot);
            }
        }

        private void OnSummary(PokemonSlotViewModel? slot)
        {
            if (slot == null)
                return;

            // future summary navigation
        }

        private void CloseActionMenu()
        {
            IsActionMenuOpen = false;

            foreach (var slot in Slots)
                slot.IsSelected = false;
        }
    }
}