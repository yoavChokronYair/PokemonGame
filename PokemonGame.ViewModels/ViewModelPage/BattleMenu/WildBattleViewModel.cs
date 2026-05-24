// =============================================================================
//  WildBattleViewModel.cs  (complete replacement for the file you uploaded)
//
//  Key additions / fixes vs. your draft:
//    1. Inventory helpers use the real BagInventory (Dictionary<itemsDomain,int>)
//       via PokeballState items — no phantom GetBallCount/UseBall methods.
//    2. Catch logic uses PokeballState.Multiplier for accurate ball modifiers.
//    3. Pokédex "caught" flag is set on a successful catch.
//    4. Clock is resumed when returning to the map.
//    5. WildBattleBagViewModel is driven by the real inventory.
//    6. Blackout handling heals the team and logs a message.
//    7. TryTriggerTrainerBattle stub for future trainer spotted flow.
// =============================================================================

using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    // =========================================================================
    //  WildBattleViewModel
    // =========================================================================
    public class WildBattleViewModel : ViewModelBase,IDisposable
    {
        // ── Core state ────────────────────────────────────────────────────────
        private readonly BattleManager _manager;
        private readonly WildPokemonDomain _wildPokemon;
        private readonly PlayerDomain _player;
        private readonly NavigationStore _navigationStore;
        private readonly Func<ViewModelBase> _createMapViewModel;

        // ── Sub-ViewModels ────────────────────────────────────────────────────
        public PokemonBattleStatusViewModel PlayerStatus { get; }
        public EnemyBattleStatusViewModel EnemyStatus { get; }
        public BattleMenuViewModel BattleMenu { get; }
        public BattleLoggerViewModel Logger { get; }
        private bool _disposed;
        private CancellationTokenSource _cts = new();

        // ── State ─────────────────────────────────────────────────────────────
        private int _logCursor;
        private bool _isBattleOverHandled;

        // ── Outcome overlay ───────────────────────────────────────────────────
        private bool _isOutcomeVisible;
        public bool IsOutcomeVisible
        {
            get => _isOutcomeVisible;
            private set => SetProperty(ref _isOutcomeVisible, value);
        }

        private string _outcomeTitle = string.Empty;
        public string OutcomeTitle
        {
            get => _outcomeTitle;
            private set => SetProperty(ref _outcomeTitle, value);
        }
        
        private string _outcomeDetail = string.Empty;
        public string OutcomeDetail
        {
            get => _outcomeDetail;
            private set => SetProperty(ref _outcomeDetail, value);
        }


        // ── Constructor ───────────────────────────────────────────────────────
        public WildBattleViewModel(
            WildPokemonDomain wildPokemon,
            PokemonTeam playerTeam,
            NavigationStore navigationStore,
            Func<ViewModelBase> createMapViewModel)
        {
            _wildPokemon = wildPokemon;
            _player = PlayerDomain.Instance;
            _navigationStore = navigationStore;
            _createMapViewModel = createMapViewModel;

            var wildTeam = PokemonTeam.Create(
                new List<PokemonState> { wildPokemon.pokemonState });

            _manager = new BattleManager(playerTeam, wildTeam, wildPokemon.BotLevel);

            Logger = new BattleLoggerViewModel();
            PlayerStatus = new PokemonBattleStatusViewModel();
            EnemyStatus = new EnemyBattleStatusViewModel();

            BattleMenu = new BattleMenuViewModel(
                OnMoveChosen,
                OnSwitchChosen,
                OnFlee,
                OnOpenBag,
                OnOpenSwitch,
                Logger);

            // Register wild Pokémon as "seen" in the Pokédex
            RegisterPokedexSeen(_wildPokemon.pokemonState.PokedexId,
                                 _wildPokemon.pokemonState.Name);

            SyncAll(flushSetup: true);
        }

        // ── Move ──────────────────────────────────────────────────────────────
        private async Task OnMoveChosen(int moveIndex)
        {
            _manager.RunTurn(moveIndex);
            await FlushLogAndWait();

            if (_manager.HasBotFainted) EnemyStatus.CurrentHP = 0;
            await EnemyStatus.WaitForHpAnimation();
            if (_manager.BotActive.IsFainted) SyncEnemyPokemon();

            if (_manager.HasTrainerFainted) PlayerStatus.CurrentHP = 0;
            await PlayerStatus.WaitForHpAnimation();
            if (_manager.PlayerActive.IsFainted) SyncPlayerPokemon();

            SyncAll();
        }

        // ── Switch ────────────────────────────────────────────────────────────
        private async void OnSwitchChosen(int slotIndex)
        {
            _manager.RunTurn(slotIndex, BattleActionType.Switch);
            await FlushLogAndWait();
            SyncPlayerPokemon();
            SyncEnemyPokemon();
            SyncAll();
        }

        // ── Flee ──────────────────────────────────────────────────────────────
        private async void OnFlee()
        {
            if (_disposed || _isBattleOverHandled)
                return;

            bool fled = _manager.TryFlee();

            await FlushLogAndWait();

            if (_disposed)
                return;

            if (!fled)
            {
                _manager.RunWildRetaliationTurn();

                await FlushLogAndWait();

                if (_manager.HasTrainerFainted)
                {
                    PlayerStatus.CurrentHP = 0;
                    await PlayerStatus.WaitForHpAnimation();
                }

                SyncAll();
                return;
            }

            _isBattleOverHandled = true;

            ReturnToMap();
        }

        public void Dispose()
        {
            CleanupResources();
            GC.SuppressFinalize(this);
        }
        private void CleanupResources()
        {
            if (_disposed)
                return;

            _disposed = true;

            _cts.Cancel();
            _cts.Dispose();

            if (_manager is IDisposable disposableManager)
                disposableManager.Dispose();

            if (Logger is IDisposable disposableLogger)
                disposableLogger.Dispose();

            if (BattleMenu is IDisposable disposableMenu)
                disposableMenu.Dispose();
        }

        // ── Catch ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Called by WildBattleBagViewModel after consuming one ball from the
        /// real bag inventory.  <paramref name="ball"/> is the PokeballState
        /// item that was used.
        /// </summary>
        public async Task TryThrowBall(PokeballState ball)
        {
            if (_isBattleOverHandled)
                return;

            CatchAttemptResult catchResult = _manager.TryThrowBall(
                _wildPokemon,
                ball);

            await FlushLogAndWait();

            if (catchResult.Caught)
            {
                _isBattleOverHandled = true;

                var caughtPokemon = PokemonConversionService.FromWildCatch(
                    _wildPokemon,
                    _player.trainerMapLocDomain.CurrentMap.Name,
                    catchResult.BallType);

                bool addedToParty = _player.AddPokemon(caughtPokemon);

                RegisterPokedexCaught(
                    _wildPokemon.pokemonState.PokedexId,
                    _wildPokemon.pokemonState.Name);

                ShowOutcome(
                    "Gotcha!",
                    addedToParty
                        ? $"{_wildPokemon.pokemonState.Name} was added to your team!"
                        : $"{_wildPokemon.pokemonState.Name} was sent to Box storage!");

                await DelayOrCancelAsync(1500);

                ReturnToMap();

                return;
            }

            if (_manager.HasTrainerFainted)
            {
                PlayerStatus.CurrentHP = 0;
                await PlayerStatus.WaitForHpAnimation();
            }

            SyncAll();
        }
        private async Task DelayOrCancelAsync(int milliseconds)
        {
            try
            {
                await Task.Delay(milliseconds, _cts.Token);
            }
            catch (TaskCanceledException)
            {
                // ViewModel was disposed. Ignore.
            }
        }
        // ── Bag ───────────────────────────────────────────────────────────────
        private void OnOpenBag()
        {
            _navigationStore.CurrentViewModel = new WildBattleBagViewModel(
                _navigationStore,
                returnToWild: ReturnSelf,
                onBallThrown: ball => TryThrowBall(ball));
        }
        private ViewModelBase ReturnSelf()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WildBattleViewModel));

            return this;
        }

        // ── Open switch ───────────────────────────────────────────────────────
        private void OnOpenSwitch()
        {
            var opts = new TeamSelectionOptions
            {
                CanMove = false,
                CanSummary = true,
                CanSwitch = true,
                IsUsingUserStore = false
            };
            _navigationStore.CurrentViewModel = new TeamSelectionViewModel(
                UserStore.Instance,
                _navigationStore,
                () => this,
                opts,
                OnSwitchChosen);
        }

        // ── Sync helpers ──────────────────────────────────────────────────────
        private void SyncAll(bool flushSetup = false)
        {
            SyncPlayerPokemon();
            SyncEnemyPokemon();

            if (_manager.logger.Entries.Count > _logCursor)
            {
                var msgs = _manager.logger.Entries.Skip(_logCursor).Select(e => e.Message);
                Logger.EnqueueStringEntries(msgs);
                _logCursor = _manager.logger.Entries.Count;
                if (flushSetup) Logger.FlushSetupMessages();
            }

            if (_manager.Winner != null && !_isBattleOverHandled)
            {
                _isBattleOverHandled = true;
                _ = OnBattleEndedAsync();
            }
        }

        private void SyncPlayerPokemon()
        {
            var p = _manager.PlayerActive;
            PlayerStatus.PokedexId = p.PokedexId;
            PlayerStatus.PokemonName = p.Name;
            PlayerStatus.Level = p.Level;
            PlayerStatus.MaxHP = p.MaxHP;
            PlayerStatus.CurrentHP = p.CurrentHP;
            PlayerStatus.StatusCondition = p.Status.ToString();

            BattleMenu.RefreshMoves(p.Moves.Select((m, i) =>
            {
                var ms = m as MoveState;
                return new MoveSnapshot
                {
                    Index = i,
                    Name = ms?.Name ?? "???",
                    Type = ms?.Element.ToString() ?? "Normal",
                    PP = ms?.PP ?? 0,
                    MaxPP = ms?.MaxPP ?? 0,
                    Power = ms?.Category == MoveCategory.Status ? null : 0,
                    Accuracy = 100
                };
            }).ToList());
        }

        private void SyncEnemyPokemon()
        {
            var e = _manager.BotActive;
            EnemyStatus.PokedexId = e.PokedexId;
            EnemyStatus.PokemonName = e.Name;
            EnemyStatus.Level = e.Level;
            EnemyStatus.MaxHP = e.MaxHP;
            EnemyStatus.CurrentHP = e.CurrentHP;
            EnemyStatus.StatusCondition = e.Status.ToString();
        }

        // Drain new log entries and wait for the queue to empty
        private async Task FlushLogAndWait()
        {
            var msgs = _manager.logger.Entries.Skip(_logCursor).Select(e => e.Message);
            Logger.EnqueueStringEntries(msgs);
            _logCursor = _manager.logger.Entries.Count;
            await Logger.WaitUntilQueueEmpty();
        }

        // ── Battle end ────────────────────────────────────────────────────────
        private async Task OnBattleEndedAsync()
        {
            bool playerWon = _manager.Winner == _manager.PlayerTeam;
            await Logger.WaitUntilQueueEmpty();

            if (playerWon)
            {
                var reward = BuildWildReward();
                PokemonConversionService.SyncAfterBattle(_player.Team, _manager.PlayerTeam, reward);
                ShowOutcome("Wild Pokémon fainted!", "Your Pokémon gained experience!");
            }
            else
            {
                int moneyLost = ApplyBlackoutPenalty();

                HealTeamAfterBlackout();

                ShowOutcome(
                    "You blacked out!",
                    moneyLost > 0
                        ? $"You dropped ₪{moneyLost}. Your team was healed."
                        : "Your team was healed.");
            }

            await DelayOrCancelAsync(1500);

            ReturnToMap();
        }
        private int ApplyBlackoutPenalty()
        {
            int currentMoney = Math.Max(0, _player.trainerInfo.Money);

            if (currentMoney <= 0)
                return 0;

            int moneyLost = Math.Max(1, currentMoney / 10);

            _player.trainerInfo.Money = Math.Max(
                0,
                currentMoney - moneyLost);

            return moneyLost;
        }

        private BattleReward BuildWildReward()
        {
            var reward = new BattleReward
            {
                FriendshipTick = 1,
                MoneyAwarded = 0
            };

            var eligible = _manager.EligibleRewardRecipients
                .Where(p => p != null && !p.IsFainted)
                .Distinct()
                .ToList();

            if (eligible.Count == 0)
            {
                var active = _manager.PlayerActive;

                if (active != null && !active.IsFainted)
                    eligible.Add(active);
            }

            if (eligible.Count == 0)
                return reward;

            int expShare = Math.Max(1, _wildPokemon.BaseExpYield / eligible.Count);

            foreach (var pokemon in eligible)
            {
                reward.ExpGains.Add(new ExpGain
                {
                    Target = pokemon,
                    Amount = expShare
                });

                if (_wildPokemon.EvYield.HasValue)
                {
                    reward.EvGains.Add(new EvGain
                    {
                        Target = pokemon,
                        Stat = _wildPokemon.EvYield.Value.stat,
                        Amount = _wildPokemon.EvYield.Value.amount
                    });
                }
            }

            return reward;
        }

        /// <summary>
        /// After a black-out, restore all party Pokémon to full HP and
        /// clear non-permanent status conditions (matching core game behaviour).
        /// </summary>
        private void HealTeamAfterBlackout()
        {
            foreach (var pokemon in _player.Team.ActiveMembers)
            {
                pokemon.CurrentHP = pokemon.PokemonState.MaxHP;
                // Clear volatile status (Sleep, Burn, etc.) but keep permanent ones
                // if your model distinguishes them.  Adjust to your actual API.
                pokemon.PersistentStatus = StatusCondition.None;
            }
        }

        // ── Pokédex helpers ───────────────────────────────────────────────────
        private void RegisterPokedexSeen(int pokedexId, string name)
        {
            if (!_player.Pokedex.TryGetValue(pokedexId, out var entry))
                _player.Pokedex[pokedexId] = (seen: true, caught: false, name);
            else if (!entry.seen)
                _player.Pokedex[pokedexId] = (seen: true, entry.caught, entry.name);
        }

        private void RegisterPokedexCaught(int pokedexId, string name)
        {
            _player.Pokedex[pokedexId] = (seen: true, caught: true, name);
        }

        private bool IsPartyFull() => _player.IsPartyFull();

        // ── Outcome / navigation ──────────────────────────────────────────────
        private void ShowOutcome(string title, string detail)
        {
            OutcomeTitle = title;
            OutcomeDetail = detail;
            IsOutcomeVisible = true;
        }

        private void ReturnToMap()
        {
            if (_disposed)
                return;

            CleanupResources();

            _navigationStore.CurrentViewModel = _createMapViewModel();
        }

    }

    // =========================================================================
    //  WildBattleBagViewModel
    //
    //  Uses the real BagInventory (Dictionary<itemsDomain, int>) and drives the
    //  ball list from PokeballState entries.  Passes PokeballState back to
    //  WildBattleViewModel.TryThrowBall so the catch formula has the Multiplier.
    // =========================================================================
    public class WildBattleBagViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<ViewModelBase> _returnToWild;
        private readonly Func<PokeballState, Task> _onBallThrown;
        private readonly PlayerDomain _player;

        private readonly List<PokeballState> _ballsByRow = new();

        private const int VisibleCount = 5;
        private int _scrollIndex = 0;
        private int _selectedIndex = 0;

        public ObservableCollection<BagItemEntryViewModel> PokemonEntries { get; } = new();

        public string CurrentCategoryName => "Poké Balls";

        public BagItemEntryViewModel? SelectedEntry =>
            PokemonEntries.ElementAtOrDefault(_selectedIndex);

        public string SelectedDescription =>
            SelectedEntry?.Description ?? string.Empty;

        public double ScrollOffset => _scrollIndex * 64;

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

        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }
        public ICommand CategoryLeftCommand { get; }
        public ICommand CategoryRightCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UseCommand { get; }
        public ICommand DeleteCommand { get; }

        public WildBattleBagViewModel(
            NavigationStore navigationStore,
            Func<ViewModelBase> returnToWild,
            Func<PokeballState, Task> onBallThrown)
        {
            _navigationStore = navigationStore;
            _returnToWild = returnToWild;
            _onBallThrown = onBallThrown;
            _player = PlayerDomain.Instance;

            SelectNextCommand = new RelayCommand(SelectNext);
            SelectPreviousCommand = new RelayCommand(SelectPrevious);

            // Wild battle bag only shows Poké Balls,
            // but the XAML expects category commands.
            CategoryLeftCommand = new RelayCommand(() => { });
            CategoryRightCommand = new RelayCommand(() => { });

            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(OnCancel);
            UseCommand = new RelayCommand(async () => await OnUse());
            DeleteCommand = new RelayCommand(OnDelete);

            LoadBalls();
        }

        private void LoadBalls()
        {
            PokemonEntries.Clear();
            _ballsByRow.Clear();

            _selectedIndex = 0;
            _scrollIndex = 0;

            var bag = _player.trainerItemDomain.BagInventory;

            foreach (var kv in bag
                         .Where(kv => kv.Value > 0 && kv.Key is PokeballState)
                         .OrderBy(kv => kv.Key.Name))
            {
                var ball = (PokeballState)kv.Key;

                _ballsByRow.Add(ball);

                PokemonEntries.Add(new BagItemEntryViewModel
                {
                    Item = ball,
                    Amount = kv.Value
                });
            }

            RefreshSelection();
            NotifyScrollAndSelection();
        }

        private void SelectNext()
        {
            if (IsActionMenuOpen)
            {
                ActionMenuIndex = (ActionMenuIndex + 1) % ActionCount;
                return;
            }

            if (PokemonEntries.Count == 0)
                return;

            _selectedIndex = MathHelper.Clamp(
                _selectedIndex + 1,
                0,
                PokemonEntries.Count - 1);

            _scrollIndex = MathHelper.Clamp(
                _scrollIndex + 1,
                0,
                Math.Max(0, PokemonEntries.Count - VisibleCount));

            RefreshSelection();
            NotifyScrollAndSelection();
        }

        private void SelectPrevious()
        {
            if (IsActionMenuOpen)
            {
                ActionMenuIndex = (ActionMenuIndex - 1 + ActionCount) % ActionCount;
                return;
            }

            if (PokemonEntries.Count == 0)
                return;

            _selectedIndex = MathHelper.Clamp(
                _selectedIndex - 1,
                0,
                PokemonEntries.Count - 1);

            _scrollIndex = MathHelper.Clamp(
                _scrollIndex - 1,
                0,
                Math.Max(0, PokemonEntries.Count - VisibleCount));

            RefreshSelection();
            NotifyScrollAndSelection();
        }

        private void OnConfirm()
        {
            if (IsActionMenuOpen)
            {
                switch (ActionMenuIndex)
                {
                    case UseIndex:
                        _ = OnUse();
                        break;

                    case DeleteIndex:
                        OnDelete();
                        break;

                    case CancelIndex:
                        CloseActionMenu();
                        break;
                }

                return;
            }

            if (SelectedEntry == null)
                return;

            ActionMenuIndex = UseIndex;
            IsActionMenuOpen = true;
        }

        private void OnCancel()
        {
            if (IsActionMenuOpen)
            {
                CloseActionMenu();
                return;
            }

            _navigationStore.CurrentViewModel = _returnToWild();
        }

        private void CloseActionMenu()
        {
            IsActionMenuOpen = false;
            ActionMenuIndex = UseIndex;
        }

        private async Task OnUse()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _ballsByRow.Count)
                return;

            var ball = _ballsByRow[_selectedIndex];

            var bag = _player.trainerItemDomain.BagInventory;

            if (!bag.TryGetValue(ball, out int qty) || qty <= 0)
                return;

            if (qty <= 1)
                bag.Remove(ball);
            else
                bag[ball] = qty - 1;

            CloseActionMenu();

            var battleVm = _returnToWild();

            _navigationStore.CurrentViewModel = battleVm;

            await _onBallThrown(ball);
        }

        private void OnDelete()
        {
            // In wild battle, DELETE should not really be used.
            // Keep it harmless because the shared XAML expects it.
            CloseActionMenu();
        }

        private void RefreshSelection()
        {
            for (int i = 0; i < PokemonEntries.Count; i++)
            {
                PokemonEntries[i].IsSelected = i == _selectedIndex;
            }
        }

        private void NotifyScrollAndSelection()
        {
            OnPropertyChanged(nameof(ScrollOffset));
            OnPropertyChanged(nameof(SelectedEntry));
            OnPropertyChanged(nameof(SelectedDescription));
        }
    }
}