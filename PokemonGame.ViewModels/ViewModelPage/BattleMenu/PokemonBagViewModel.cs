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

        // ── Category tabs (order = display order) ─────────────────────────────
        private static readonly ItemType[] Categories =
        {
            ItemType.Consumable,
            ItemType.Pokeball,
            ItemType.HeldItem,
            ItemType.Hm,
            ItemType.KeyItem,
        };

        private int _categoryIndex = 0;

        public string CurrentCategoryName =>
            Categories[_categoryIndex] switch
            {
                ItemType.Consumable => "Items",
                ItemType.Pokeball => "Poké Balls",
                ItemType.HeldItem => "Held Items",
                ItemType.Hm => "HMs",
                ItemType.KeyItem => "Key Items",
                _ => Categories[_categoryIndex].ToString()
            };

        // ── Scroll / selection state ──────────────────────────────────────────
        private const int VisibleCount = 5;
        private int _scrollIndex = 0;
        private int _selectedIndex = 0;

        public BagItemEntryViewModel? SelectedEntry =>
            PokemonEntries.ElementAtOrDefault(_selectedIndex);

        public string SelectedDescription => SelectedEntry?.Description ?? string.Empty;

        // ── Bindable list ─────────────────────────────────────────────────────
        public ObservableCollection<BagItemEntryViewModel> PokemonEntries { get; } = new();

        // ── Commands ──────────────────────────────────────────────────────────
        public ICommand ScrollUpCommand { get; }
        public ICommand ScrollDownCommand { get; }
        public ICommand CategoryLeftCommand { get; }
        public ICommand CategoryRightCommand { get; }
        public ICommand ExitCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────
        public PokemonBagViewModel(NavigationStore navigationStore, Func<ViewModelBase> onExit)
        {
            _player = PlayerDomain.Instance;
            _navigationStore = navigationStore;

            ScrollUpCommand = new RelayCommand(() => Scroll(-1));
            ScrollDownCommand = new RelayCommand(() => Scroll(1));
            CategoryLeftCommand = new RelayCommand(() => ChangeCategory(-1));
            CategoryRightCommand = new RelayCommand(() => ChangeCategory(1));
            ExitCommand = new RelayCommand(() => _navigationStore.CurrentViewModel = onExit());

            LoadEntries();
        }

        // ── Private helpers ───────────────────────────────────────────────────
        private void LoadEntries()
        {
            PokemonEntries.Clear();
            _selectedIndex = 0;
            _scrollIndex = 0;

            var currentType = Categories[_categoryIndex];

            foreach (var kv in _player.trainerItemDomain.BagInventory
                         .Where(kv => kv.Key.Type == currentType)
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

        private void ChangeCategory(int direction)
        {
            _categoryIndex = (_categoryIndex + direction + Categories.Length) % Categories.Length;

            OnPropertyChanged(nameof(CurrentCategoryName));
            LoadEntries();
        }

        private void Scroll(int direction)
        {
            int maxIndex = Math.Max(0, PokemonEntries.Count - 1);
            _selectedIndex = MathHelper.Clamp(_selectedIndex + direction, 0, maxIndex);

            int scrollMax = Math.Max(0, PokemonEntries.Count - VisibleCount);
            _scrollIndex = MathHelper.Clamp(_scrollIndex + direction, 0, scrollMax);

            RefreshSelection();
            OnPropertyChanged(nameof(ScrollOffset));
            OnPropertyChanged(nameof(SelectedEntry));
            OnPropertyChanged(nameof(SelectedDescription));
        }

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
