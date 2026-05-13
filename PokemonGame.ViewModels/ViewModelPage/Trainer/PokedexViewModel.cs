using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Helper;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Trainer
{
    public class PokedexPageViewModel : ViewModelBase
    {
        private readonly PlayerDomain _player;
        private readonly NavigationStore _navigationStore;
        public PokedexEntryViewModel? SelectedEntry =>
            PokemonEntries.ElementAtOrDefault(_selectedIndex);
        // ── Scroll state ──────────────────────────────────────────────────────
        private const int VisibleCount = 5;
        private int _scrollIndex = 0;
        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        // ── Bindable collections ──────────────────────────────────────────────
        public ObservableCollection<PokedexEntryViewModel> PokemonEntries { get; } = new();

        // ── Commands ──────────────────────────────────────────────────────────
        public ICommand ScrollUpCommand { get; }
        public ICommand ScrollDownCommand { get; }
        public ICommand ExitCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────
        public PokedexPageViewModel(NavigationStore navigationStore, Func<ViewModelBase> onExit)
        {
            _player = PlayerDomain.Instance;
            _navigationStore = navigationStore;
            ScrollUpCommand = new RelayCommand(() => Scroll(-1));
            ScrollDownCommand = new RelayCommand(() => Scroll(1));
            ExitCommand = new RelayCommand(() => _navigationStore.CurrentViewModel = onExit());

            LoadEntries();
        }

        // ── Private helpers ───────────────────────────────────────────────────
        private void LoadEntries()
        {
            PokemonEntries.Clear();

            var seen = _player.Pokedex
                .Where(kv => kv.Value.seen)
                .OrderBy(kv => kv.Key)
                .ToList();

            foreach (var kv in seen)
            {
                PokemonEntries.Add(new PokedexEntryViewModel
                {
                    Id = kv.Key,
                    Name = kv.Value.caught ? kv.Value.name : "?????",
                    Caught = kv.Value.caught,
                });
            }
            RefreshSelection();
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
        }
        /// <summary>
        /// Pixel offset for the ScrollViewer — bind ScrollViewer.VerticalOffset in code-behind
        /// or use a behaviour that watches this property.
        /// </summary>
        public double ScrollOffset => _scrollIndex * 60; // 60 px = approx row height
        private void RefreshSelection()
        {
            for (int i = 0; i < PokemonEntries.Count; i++)
                PokemonEntries[i].IsSelected = i == _selectedIndex;
        }
    }

    // ── Entry display model ───────────────────────────────────────────────────
    public class PokedexEntryViewModel : ViewModelBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Caught { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
