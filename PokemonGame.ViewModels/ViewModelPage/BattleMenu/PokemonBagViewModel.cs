using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Helper;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class PokemonBagViewModel : ViewModelBase
    {
        private readonly PlayerDomain _player;
        private readonly NavigationStore _navigationStore;
        private readonly Func<ViewModelBase> _onExit;

        // ── Category tabs ─────────────────────────────────────────────────────
        private enum BagTab { Items, Pokeballs, HeldItems, TMs, HMs, KeyItems }

        private static readonly BagTab[] Tabs =
        {
            BagTab.Items,
            BagTab.Pokeballs,
            BagTab.HeldItems,
            BagTab.TMs,
            BagTab.HMs,
            BagTab.KeyItems,
        };

        private int _tabIndex = 0;

        public string CurrentCategoryName => Tabs[_tabIndex] switch
        {
            BagTab.Items => "Items",
            BagTab.Pokeballs => "Poké Balls",
            BagTab.HeldItems => "Held Items",
            BagTab.TMs => "TMs",
            BagTab.HMs => "HMs",
            BagTab.KeyItems => "Key Items",
            _ => string.Empty
        };

        // ── Scroll / selection state ──────────────────────────────────────────
        private const int VisibleCount = 5;
        private int _scrollIndex = 0;
        private int _selectedIndex = 0;

        public BagItemEntryViewModel? SelectedEntry =>
            PokemonEntries.ElementAtOrDefault(_selectedIndex);

        public string SelectedDescription => SelectedEntry?.Description ?? string.Empty;

        // ── Action menu ───────────────────────────────────────────────────────
        private bool _isActionMenuOpen;
        public bool IsActionMenuOpen
        {
            get => _isActionMenuOpen;
            set
            {
                if (SetProperty(ref _isActionMenuOpen, value))
                {
                    OnPropertyChanged(nameof(IsUseSelected));
                    OnPropertyChanged(nameof(IsDeleteSelected));
                    OnPropertyChanged(nameof(IsCancelSelected));
                }
            }
        }

        private int _actionMenuIndex;
        public int ActionMenuIndex
        {
            get => _actionMenuIndex;
            set
            {
                if (SetProperty(ref _actionMenuIndex, value))
                {
                    OnPropertyChanged(nameof(IsUseSelected));
                    OnPropertyChanged(nameof(IsDeleteSelected));
                    OnPropertyChanged(nameof(IsCancelSelected));
                }
            }
        }

        private const int ActionCount = 3;
        private const int UseIndex = 0;
        private const int DeleteIndex = 1;
        private const int CancelIndex = 2;

        public bool IsUseSelected => IsActionMenuOpen && ActionMenuIndex == UseIndex;
        public bool IsDeleteSelected => IsActionMenuOpen && ActionMenuIndex == DeleteIndex;
        public bool IsCancelSelected => IsActionMenuOpen && ActionMenuIndex == CancelIndex;

        // ── Bindable list ─────────────────────────────────────────────────────
        public ObservableCollection<BagItemEntryViewModel> PokemonEntries { get; } = new();

        // ── Commands ──────────────────────────────────────────────────────────
        // Unified — bound to BOTH W/S AND Up/Down in XAML
        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }

        // Unified — bound to BOTH A/D AND Left/Right in XAML
        public ICommand CategoryLeftCommand { get; }
        public ICommand CategoryRightCommand { get; }

        public ICommand ConfirmCommand { get; }   // Z / Enter
        public ICommand CancelCommand { get; }   // X / Esc
        public ICommand UseCommand { get; }
        public ICommand DeleteCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────
        public PokemonBagViewModel(NavigationStore navigationStore, Func<ViewModelBase> onExit)
        {
            _player = PlayerDomain.Instance;
            _navigationStore = navigationStore;
            _onExit = onExit;

            SelectNextCommand = new RelayCommand(SelectNext);
            SelectPreviousCommand = new RelayCommand(SelectPrevious);
            CategoryLeftCommand = new RelayCommand(() => ChangeTab(-1));
            CategoryRightCommand = new RelayCommand(() => ChangeTab(1));
            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(OnCancel);
            UseCommand = new RelayCommand(OnUse);
            DeleteCommand = new RelayCommand(OnDelete);

            LoadEntries();
        }

        // ── Unified navigation (mirrors TeamSelection SelectNextSlot) ─────────
        private void SelectNext()
        {
            if (IsActionMenuOpen)
            {
                ActionMenuIndex = (ActionMenuIndex + 1) % ActionCount;
                return;
            }

            if (PokemonEntries.Count == 0) return;

            int maxIndex = PokemonEntries.Count - 1;
            _selectedIndex = MathHelper.Clamp(_selectedIndex + 1, 0, maxIndex);

            int scrollMax = Math.Max(0, PokemonEntries.Count - VisibleCount);
            _scrollIndex = MathHelper.Clamp(_scrollIndex + 1, 0, scrollMax);

            RefreshSelection();
            OnPropertyChanged(nameof(ScrollOffset));
            OnPropertyChanged(nameof(SelectedEntry));
            OnPropertyChanged(nameof(SelectedDescription));
        }

        private void SelectPrevious()
        {
            if (IsActionMenuOpen)
            {
                ActionMenuIndex = (ActionMenuIndex - 1 + ActionCount) % ActionCount;
                return;
            }

            if (PokemonEntries.Count == 0) return;

            int maxIndex = PokemonEntries.Count - 1;
            _selectedIndex = MathHelper.Clamp(_selectedIndex - 1, 0, maxIndex);

            int scrollMax = Math.Max(0, PokemonEntries.Count - VisibleCount);
            _scrollIndex = MathHelper.Clamp(_scrollIndex - 1, 0, scrollMax);

            RefreshSelection();
            OnPropertyChanged(nameof(ScrollOffset));
            OnPropertyChanged(nameof(SelectedEntry));
            OnPropertyChanged(nameof(SelectedDescription));
        }

        // ── Confirm / Cancel ──────────────────────────────────────────────────
        private void OnConfirm()
        {
            if (IsActionMenuOpen)
            {
                switch (ActionMenuIndex)
                {
                    case UseIndex: OnUse(); break;
                    case DeleteIndex: OnDelete(); break;
                    case CancelIndex: CloseActionMenu(); break;
                }
                return;
            }

            if (SelectedEntry == null) return;
            ActionMenuIndex = 0;
            IsActionMenuOpen = true;
        }

        private void OnCancel()
        {
            if (IsActionMenuOpen)
            {
                CloseActionMenu();
                return;
            }
            _navigationStore.CurrentViewModel = _onExit();
        }

        private void CloseActionMenu()
        {
            IsActionMenuOpen = false;
            ActionMenuIndex = 0;
        }

        // ── Item actions ──────────────────────────────────────────────────────
        private void OnUse()
        {
            // TODO: wire to item-use service
            CloseActionMenu();
        }

        private void OnDelete()
        {
            var entry = SelectedEntry;
            if (entry == null) return;

            var key = _player.trainerItemDomain.BagInventory
                .Keys.FirstOrDefault(k => k.Name == entry.Name);

            if (key != null)
            {
                var qty = _player.trainerItemDomain.BagInventory[key];
                if (qty <= 1)
                    _player.trainerItemDomain.BagInventory.Remove(key);
                else
                    _player.trainerItemDomain.BagInventory[key] = qty - 1;
            }

            CloseActionMenu();
            LoadEntries();
        }

        // ── Tab switching ─────────────────────────────────────────────────────
        private void ChangeTab(int direction)
        {
            if (IsActionMenuOpen) return;

            _tabIndex = (_tabIndex + direction + Tabs.Length) % Tabs.Length;
            OnPropertyChanged(nameof(CurrentCategoryName));
            LoadEntries();
        }

        // ── Load / filter ─────────────────────────────────────────────────────
        private void LoadEntries()
        {
            PokemonEntries.Clear();
            _selectedIndex = 0;
            _scrollIndex = 0;

            var tab = Tabs[_tabIndex];

            foreach (var kv in _player.trainerItemDomain.BagInventory
                         .Where(kv => MatchesTab(kv.Key, tab))
                         .OrderBy(kv => kv.Key.Name))
            {
                PokemonEntries.Add(new BagItemEntryViewModel
                {
                    Name = kv.Key.Name,
                    Amount = kv.Value,
                    Description = kv.Key.Description,
                });
            }

            RefreshSelection();
            OnPropertyChanged(nameof(ScrollOffset));
            OnPropertyChanged(nameof(SelectedEntry));
            OnPropertyChanged(nameof(SelectedDescription));
        }

        private static bool MatchesTab(itemsDomain item, BagTab tab) => tab switch
        {
            BagTab.Items => item is not TmHmState
                                && item is not PokeballState
                                && item is not HeldItemState
                                && item is not KeyItemState
                                && item.Type == ItemType.Consumable,
            BagTab.Pokeballs => item is PokeballState,
            BagTab.HeldItems => item is HeldItemState,
            BagTab.TMs => item is TmHmState tm && !tm.IsHm,
            BagTab.HMs => item is TmHmState hm && hm.IsHm,
            BagTab.KeyItems => item is KeyItemState,
            _ => false,
        };

        public double ScrollOffset => _scrollIndex * 64;

        private void RefreshSelection()
        {
            for (int i = 0; i < PokemonEntries.Count; i++)
                PokemonEntries[i].IsSelected = i == _selectedIndex;
        }
    }

    // ── Entry display model ───────────────────────────────────────────────────
    public class BagItemEntryViewModel : ViewModelBase
    {
        public string Name { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
