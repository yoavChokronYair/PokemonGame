using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Battle;
using PokemonGame.Model.Model.Helper.MoveHelper;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.Translators;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    // ── Root page VM — set as DataContext on PokemonBattlePage ───────────────
    public class BattleViewModel : ViewModelBase
    {
        private readonly BattleManager _manager;

        // ── Child ViewModels (bound by the UserControls) ─────────────────────
        public PokemonBattleStatusViewModel PlayerStatus { get; }
        public EnemyBattleStatusViewModel EnemyStatus { get; }
        public BattleMenuViewModel BattleMenu { get; }

        // ── Battle log ───────────────────────────────────────────────────────
        public ObservableCollection<string> Log { get; } = new();

        // ── Phase helpers ────────────────────────────────────────────────────
        public bool IsBattleOver => _manager.IsBattleOver;
        public bool IsAwaitingSwitch => _manager.Phase == BattlePhase.AwaitingPlayerSwitch;
        public string? WinnerName => _manager.Winner?.Active.Name;

        // ── Constructor ──────────────────────────────────────────────────────
        public BattleViewModel(UserStore playerUserStore, UserStore botUserStore)
        {
            var translator = new TeamTranslator();

            var playerTeam = translator.LoadTeam(playerUserStore.BattlePlayerID);
            var botTeam = translator.LoadTeam(botUserStore.BattlePlayerID);

            _manager = new BattleManager(playerTeam, botTeam);

            PlayerStatus = new PokemonBattleStatusViewModel();
            EnemyStatus = new EnemyBattleStatusViewModel();
            BattleMenu = new BattleMenuViewModel(OnMoveChosen, OnSwitchChosen, _manager);

            SyncAll();
        }
        public BattleViewModel(UserStore playerBattlePlayerId)
        {
            var translator = new TeamTranslator();
            var service = new PokemonService();

            // 1. Load the Player's set team
            var playerTeam = translator.LoadTeam(playerBattlePlayerId.BattlePlayerID);

            PokemonTeam botTeam;
            
            // 2. Generate a random team and translate each result to a Domain object
            var randomResults = service.GenerateRandomTeam(count: 6, level: 50);
            var roster = randomResults
                .Select(r => translator.TranslateToDomain(r))
                .ToList();

            botTeam = PokemonTeam.Create(roster);
            

            _manager = new BattleManager(playerTeam, botTeam);

            // Standard UI Initialization
            PlayerStatus = new PokemonBattleStatusViewModel();
            EnemyStatus = new EnemyBattleStatusViewModel();
            BattleMenu = new BattleMenuViewModel(OnMoveChosen, OnSwitchChosen, _manager);

            SyncAll();
        }

        // ── Called by BattleMenuViewModel when player picks a move ───────────
        private void OnMoveChosen(int moveIndex)
        {
            if (_manager.Phase != BattlePhase.AwaitingPlayerAction) return;

            _manager.RunTurn(moveIndex, botDecides: true);
            SyncAll();
        }

        // ── Called by BattleMenuViewModel when player picks a switch slot ────
        private void OnSwitchChosen(int slotIndex)
        {
            if (_manager.Phase != BattlePhase.AwaitingPlayerSwitch) return;

            _manager.PlayerSwitch(slotIndex);
            SyncAll();
        }

        // ── Push all state down to child VMs ─────────────────────────────────
        private void SyncAll()
        {
            var p = _manager.PlayerActive;
            PlayerStatus.PokemonName = p.Name;
            PlayerStatus.Level = p.Level;
            PlayerStatus.CurrentHP = p.CurrentHP;
            PlayerStatus.MaxHP = p.MaxHP;

            var e = _manager.BotActive;
            EnemyStatus.PokemonName = e.Name;
            EnemyStatus.Level = e.Level;
            EnemyStatus.CurrentHP = e.CurrentHP;
            EnemyStatus.MaxHP = e.MaxHP;

            // Feed current moves into the menu
            BattleMenu.RefreshMoves(_manager.PlayerActive.Moves);

            // Append new log lines
            foreach (var line in _manager.BattleLog.Skip(Log.Count))
                Log.Add(line);

            OnPropertyChanged(nameof(IsBattleOver));
            OnPropertyChanged(nameof(IsAwaitingSwitch));
            OnPropertyChanged(nameof(WinnerName));
        }
    }

    // ── BattleMenuViewModel — owns fight/bag/pokemon/run + moveset panel ─────
   

    // ── BattlePokemonMovesetChooserViewModel — the 4 move buttons ────────────
    public class BattlePokemonMovesetChooserViewModel : ViewModelBase
    {
        private readonly Action<int> _onMoveClicked;
        private readonly Action<IMove?> _onMoveHovered;

        public MoveSlotViewModel Move0 { get; }
        public MoveSlotViewModel Move1 { get; }
        public MoveSlotViewModel Move2 { get; }
        public MoveSlotViewModel Move3 { get; }

        public BattlePokemonMovesetChooserViewModel(Action<int> onMoveClicked, Action<IMove?> onMoveHovered)
        {
            _onMoveClicked = onMoveClicked;
            _onMoveHovered = onMoveHovered;

            Move0 = new MoveSlotViewModel(0, onMoveClicked, onMoveHovered);
            Move1 = new MoveSlotViewModel(1, onMoveClicked, onMoveHovered);
            Move2 = new MoveSlotViewModel(2, onMoveClicked, onMoveHovered);
            Move3 = new MoveSlotViewModel(3, onMoveClicked, onMoveHovered);
        }

        public void LoadMoves(IReadOnlyList<IMove> moves)
        {
            var slots = new[] { Move0, Move1, Move2, Move3 };
            for (int i = 0; i < 4; i++)
            {
                if (i < moves.Count)
                    slots[i].SetMove(moves[i]);
                else
                    slots[i].Clear();
            }
        }
    }

    // ── One move button slot ─────────────────────────────────────────────────
    public class MoveSlotViewModel : ViewModelBase
    {
        private readonly int _index;
        private readonly Action<int> _onClick;
        private readonly Action<IMove?> _onHover;

        private string _moveName = "-";
        private bool _isEnabled = false;
        private IMove? _move;

        public string MoveName
        {
            get => _moveName;
            private set => SetProperty(ref _moveName, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            private set => SetProperty(ref _isEnabled, value);
        }

        public ICommand ClickCommand { get; }
        public ICommand HoverCommand { get; }
        public ICommand LeaveCommand { get; }

        public MoveSlotViewModel(int index, Action<int> onClick, Action<IMove?> onHover)
        {
            _index = index;
            _onClick = onClick;
            _onHover = onHover;

            ClickCommand = new RelayCommand(() => _onClick(_index), () => IsEnabled);
            HoverCommand = new RelayCommand(() => _onHover(_move));
            LeaveCommand = new RelayCommand(() => _onHover(null));
        }

        public void SetMove(IMove move)
        {
            _move = move;
            MoveName = (move as MoveState)?.Name ?? "-";
            IsEnabled = true;
        }

        public void Clear()
        {
            _move = null;
            MoveName = "-";
            IsEnabled = false;
        }
    }

    // ── PokemonBattleStatusViewModel (player) — unchanged shape, live data ───
   

    // ── EnemyBattleStatusViewModel — unchanged shape, live data ─────────────
   
}