using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Helper;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Online
{
    // ── Status VM shared by player and enemy bars ─────────────────────────────
    public class OnlineBattleStatusViewModel : ViewModelBase
    {
        private string _pokemonName = "…";
        private int _level = 1;
        private int _currentHP;
        private int _maxHP = 1;
        private string _statusCondition = "None";
        private int _pokedexId;

        public string PokemonName { get => _pokemonName; set => SetProperty(ref _pokemonName, value); }
        public int Level { get => _level; set => SetProperty(ref _level, value); }
        public int CurrentHP { get => _currentHP; set { SetProperty(ref _currentHP, value); OnPropertyChanged(nameof(HpPercentage)); OnPropertyChanged(nameof(HPColor)); } }
        public int MaxHP { get => _maxHP; set { SetProperty(ref _maxHP, value); OnPropertyChanged(nameof(HpPercentage)); OnPropertyChanged(nameof(HPColor)); } }
        public string StatusCondition { get => _statusCondition; set => SetProperty(ref _statusCondition, value); }
        public int PokedexId { get => _pokedexId; set => SetProperty(ref _pokedexId, value); }

        public double HpPercentage => MaxHP <= 0 ? 0 : MathHelper.Clamp((double)CurrentHP / MaxHP, 0, 1);
        public string GenderSymbol => "—";
        public Brush GenderColor => Brushes.White;

        public Brush HPColor => HpPercentage > 0.5 ? Brushes.LimeGreen
                              : HpPercentage > 0.2 ? Brushes.Yellow
                              : Brushes.Red;
    }

    // ── Online move slot VM ───────────────────────────────────────────────────
    public class OnlineMoveSlotViewModel : ViewModelBase
    {
        private string _moveName = "-";
        private bool _isEnabled;

        public string MoveName { get => _moveName; set => SetProperty(ref _moveName, value); }
        public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }

        public ICommand ClickCommand { get; }
        public ICommand HoverCommand { get; }
        public ICommand LeaveCommand { get; }

        public OnlineMoveSlotViewModel(int index, Action<int> onClick)
        {
            ClickCommand = new RelayCommand(() => onClick(index), () => IsEnabled);
            HoverCommand = new RelayCommand(() => { });
            LeaveCommand = new RelayCommand(() => { });
        }

        public void SetMove(string name, bool enabled)
        {
            MoveName = name;
            IsEnabled = enabled;
            ((RelayCommand)ClickCommand).NotifyCanExecuteChanged();
        }
    }

    // ── Online moveset chooser VM (matches BattlePokemonMovesetChooserUserControl) ──
    public class OnlineMovesetChooserViewModel : ViewModelBase
    {
        public OnlineMoveSlotViewModel Move0 { get; }
        public OnlineMoveSlotViewModel Move1 { get; }
        public OnlineMoveSlotViewModel Move2 { get; }
        public OnlineMoveSlotViewModel Move3 { get; }

        public OnlineMovesetChooserViewModel(Action<int> onMoveChosen)
        {
            Move0 = new OnlineMoveSlotViewModel(0, onMoveChosen);
            Move1 = new OnlineMoveSlotViewModel(1, onMoveChosen);
            Move2 = new OnlineMoveSlotViewModel(2, onMoveChosen);
            Move3 = new OnlineMoveSlotViewModel(3, onMoveChosen);
        }

        public void LoadMoves(List<string> names, bool enabled)
        {
            var slots = new[] { Move0, Move1, Move2, Move3 };
            for (int i = 0; i < 4; i++)
                slots[i].SetMove(i < names.Count ? names[i] : "-", enabled && i < names.Count);
        }

        public void SetEnabled(bool enabled)
        {
            foreach (var s in new[] { Move0, Move1, Move2, Move3 })
                s.IsEnabled = enabled && s.MoveName != "-";
        }
    }

    // ── Online battle menu VM (matches BattleSelectionMenuUserControl) ────────
    public class OnlinesBattleMenuViewModel : ViewModelBase
    {
        private bool _isMainMenuVisible = true;
        private bool _isMovesetVisible;
        private string _selectedMovePP = string.Empty;
        private string _selectedMoveType = string.Empty;

        public bool IsMainMenuVisible { get => _isMainMenuVisible; set => SetProperty(ref _isMainMenuVisible, value); }
        public bool IsMovesetVisible { get => _isMovesetVisible; set => SetProperty(ref _isMovesetVisible, value); }
        public string SelectedMovePP { get => _selectedMovePP; set => SetProperty(ref _selectedMovePP, value); }
        public string SelectedMoveType { get => _selectedMoveType; set => SetProperty(ref _selectedMoveType, value); }

        public OnlineMovesetChooserViewModel MovesetChooser { get; }
        public OnlineBattleLoggerViewModel Logger { get; }

        public ICommand OpenMovesetCommand { get; }
        public ICommand CloseMovesetCommand { get; }

        public OnlinesBattleMenuViewModel(Action<int> onMoveChosen,
                                         OnlineBattleLoggerViewModel logger)
        {
            Logger = logger;
            MovesetChooser = new OnlineMovesetChooserViewModel(onMoveChosen);

            OpenMovesetCommand = new RelayCommand(() => { IsMainMenuVisible = false; IsMovesetVisible = true; });
            CloseMovesetCommand = new RelayCommand(() => { IsMainMenuVisible = true; IsMovesetVisible = false; });
        }

        public void LoadMoves(List<string> names) =>
            MovesetChooser.LoadMoves(names, enabled: true);

        public void SetActionsEnabled(bool enabled)
        {
            MovesetChooser.SetEnabled(enabled);
            if (!enabled) { IsMainMenuVisible = true; IsMovesetVisible = false; }
        }
    }

    // ── Online logger VM (matches LoggerUserControl) ──────────────────────────
    public class OnlineBattleLoggerViewModel : ViewModelBase
    {
        private readonly Queue<string> _queue = new();
        private string _currentMessage = string.Empty;
        private bool _hasMore;

        public string CurrentMessage { get => _currentMessage; set => SetProperty(ref _currentMessage, value); }
        public bool HasMore { get => _hasMore; set => SetProperty(ref _hasMore, value); }
        public string PhaseLabel { get; private set; } = string.Empty;

        public ICommand NextCommand { get; }

        public OnlineBattleLoggerViewModel()
        {
            NextCommand = new RelayCommand(ShowNext, () => HasMore);
        }

        public void EnqueueLines(IEnumerable<string> lines)
        {
            foreach (var l in lines) _queue.Enqueue(l);
            if (_queue.Count > 0) ShowNext();
            HasMore = _queue.Count > 0;
        }

        public void Show(string message)
        {
            CurrentMessage = message;
            HasMore = false;
        }

        private void ShowNext()
        {
            if (_queue.Count > 0)
                CurrentMessage = _queue.Dequeue();
            HasMore = _queue.Count > 0;
            ((RelayCommand)NextCommand).NotifyCanExecuteChanged();
        }
    }

}