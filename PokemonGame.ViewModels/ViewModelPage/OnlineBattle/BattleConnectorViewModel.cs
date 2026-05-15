using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Interfaces;
using PokemonGame.Services.Services;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.Translators;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class BattleConnectorViewModel : ViewModelBase, IDisposable
    {
        private readonly UserStore _userStore;
        private readonly NavigationStore _rootNavigationStore;
        private readonly ITeamService _teamService;
        private readonly Func<BattleViewModel> _createBattleViewModel;
        private readonly IMatchmakingService? _matchmaking;

        // Needed so OnMatchFound can create OnlineBattleService with the right URL
        private readonly string _serverBaseUrl;

        public int RequiredCount { get; }
        public string RequiredCountLabel => $"{SelectedCount} / {RequiredCount} selected";

        public ObservableCollection<ConnectorSlotEntry> TeamSlots { get; } = new();
        public ObservableCollection<ConnectorSlotEntry> RivalSlots { get; } = new();

        public bool IsOffline => !_userStore.BattleSesion.IsOnlineMode;

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        private bool _isRivalReady;
        public bool IsRivalReady
        {
            get => _isRivalReady;
            set => SetProperty(ref _isRivalReady, value);
        }

        private BotDifficulty _botDifficulty = BotDifficulty.Medium;
        public BotDifficulty BotDifficulty
        {
            get => _botDifficulty;
            set
            {
                SetProperty(ref _botDifficulty, value);
                OnPropertyChanged(nameof(IsEasy));
                OnPropertyChanged(nameof(IsMedium));
                OnPropertyChanged(nameof(IsHard));
            }
        }

        public bool IsEasy { get => BotDifficulty == BotDifficulty.Easy; set { if (value) BotDifficulty = BotDifficulty.Easy; } }
        public bool IsMedium { get => BotDifficulty == BotDifficulty.Medium; set { if (value) BotDifficulty = BotDifficulty.Medium; } }
        public bool IsHard { get => BotDifficulty == BotDifficulty.Hard; set { if (value) BotDifficulty = BotDifficulty.Hard; } }

        public int SelectedCount => TeamSlots.Count(s => s.IsSelected);
        public bool CanConfirm => SelectedCount == RequiredCount;
        public string ConfirmLabel => IsOffline ? "▶  Fight Bot" : "🔍  Find Match";

        public RelayCommand<ConnectorSlotEntry> ToggleCommand { get; }
        public RelayCommand ConfirmCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand CancelSearchCommand { get; }

        public BattleConnectorViewModel(
            UserStore userStore,
            NavigationStore rootNavigationStore,
            Func<BattleViewModel> createBattleViewModel,
            Func<OnlineBattleShellViewModel> createOnlineBattleShellViewModel)
        {
            _userStore = userStore;
            _rootNavigationStore = rootNavigationStore;
            _teamService = userStore.Resolver.GetTeamService();
            _createBattleViewModel = createBattleViewModel;
            _serverBaseUrl = _userStore.ServerBaseUrl;

            // FIX #7: read directly from UserStore.Matchmaking rather than
            // Resolver.MatchmakingService, which may not exist on the Resolver
            // class and would cause a compile error or return null silently.
            _matchmaking = userStore.Matchmaking;

            // Subscribe to matchmaking events
            if (_matchmaking is not null)
            {
                _matchmaking.OnMatchFound += OnMatchFound;
                _matchmaking.OnQueued += OnQueued;
                _matchmaking.OnCancelled += OnCancelled;
            }

            var session = _userStore.BattleSesion;

            RequiredCount = session.BattleMode switch
            {
                BattleMode.halfTeam => 3,
                BattleMode.TwoThirdsTeam => 4,
                BattleMode.fullTeam => 6,
                _ => 6
            };

            // ── Load player team slots ────────────────────────────────────────
            if (session.SelectedTeamId.HasValue)
            {
                var members = _teamService.GetTeamMembers(session.SelectedTeamId.Value);
                foreach (var m in members)
                {
                    var slot = new ConnectorSlotEntry(m);
                    slot.PropertyChanged += (_, _) =>
                    {
                        OnPropertyChanged(nameof(SelectedCount));
                        OnPropertyChanged(nameof(CanConfirm));
                        OnPropertyChanged(nameof(RequiredCountLabel));
                        ConfirmCommand?.NotifyCanExecuteChanged();
                    };
                    TeamSlots.Add(slot);
                }
            }

            // ── Generate rival preview (offline) / placeholder slots (online) ─
            var pokemonService = _userStore.Resolver.GetPokemonService();
            var rivalResults = pokemonService.GenerateRandomTeam(count: RequiredCount, level: 50);

            session.RivalPokemonIds = rivalResults.Select(r => r.Battler.PokedexID).ToList();

            foreach (var r in rivalResults)
                RivalSlots.Add(new ConnectorSlotEntry(r.Battler.PokedexID, r.Battler.Name));

            for (int i = RivalSlots.Count; i < 6; i++)
                RivalSlots.Add(new ConnectorSlotEntry());

            // ── Toggle command ────────────────────────────────────────────────
            ToggleCommand = new RelayCommand<ConnectorSlotEntry>(slot =>
            {
                if (slot == null) return;

                if (slot.IsSelected)
                {
                    int removedOrder = slot.PickOrder ?? 0;
                    slot.IsSelected = false;
                    slot.PickOrder = null;

                    foreach (var s in TeamSlots.Where(s => s.PickOrder > removedOrder))
                        s.PickOrder--;
                }
                else if (SelectedCount < RequiredCount)
                {
                    slot.IsSelected = true;
                    slot.PickOrder = SelectedCount;
                }
            });

            // ── Confirm command ───────────────────────────────────────────────
            ConfirmCommand = new RelayCommand(() =>
            {
                var selectedIds = TeamSlots
                    .Where(s => s.IsSelected)
                    .OrderBy(s => s.PickOrder)
                    .Select(s => s.PokedexId)
                    .ToList();

                _userStore.BattleSesion.SelectedPokemonIds = selectedIds;
                _userStore.BattleSesion.BotDifficulty = BotDifficulty;

                var translator = new TeamTranslator(_userStore);
                var fullTeam = translator.LoadTeamByID(_userStore.BattlePlayerID);

                List<PokemonState> playerRoster;

                if (session.BattleMode == BattleMode.fullTeam || selectedIds.Count == 0)
                {
                    playerRoster = Enumerable.Range(0, fullTeam.getAllPokemonCount())
                        .Select(i => fullTeam.GetPokemonAt(i))
                        .ToList();
                }
                else
                {
                    // Build roster in pick-order, not team-order
                    var allPokemon = Enumerable.Range(0, fullTeam.getAllPokemonCount())
                        .Select(i => fullTeam.GetPokemonAt(i))
                        .ToList();

                    playerRoster = selectedIds
                        .Select(id => allPokemon.FirstOrDefault(p => p.PokedexId == id))
                        .Where(p => p != null)
                        .ToList()!;
                }

                while (playerRoster.Count < 6)
                    playerRoster.Add(fullTeam.GetPokemonAt(0));

                _userStore.BattleSesion.ResolvedPlayerTeam = PokemonTeam.Create(playerRoster);

                // Build bot team only in offline mode — server handles it online
                if (!_userStore.BattleSesion.IsOnlineMode)
                {
                    var rivalRoster = session.RivalPokemonIds
                        .Select(id => translator.TranslateToDomain(
                            rivalResults.First(r => r.Battler.PokedexID == id)))
                        .ToList();

                    while (rivalRoster.Count < 6)
                        rivalRoster.Add(rivalRoster[0]);

                    _userStore.BattleSesion.ResolvedBotTeam = PokemonTeam.Create(rivalRoster);
                }

                if (_userStore.BattleSesion.IsOnlineMode)
                {
                    IsSearching = true;

                    _ = _matchmaking!.FindMatchAsync(new MatchmakingRequest
                    {
                        PlayerId = _userStore.BattlePlayerID,
                        PlayerName = _userStore.Username,
                        BattleMode = _userStore.BattleSesion.BattleMode.ToString(),
                        IsOneVOne = _userStore.BattleSesion.IsOneVOne,
                        TeamId = _userStore.BattleSesion.SelectedTeamId ?? 0,
                        SelectedPokemonIds = selectedIds
                    }).ContinueWith(t =>
                        Console.WriteLine($"[FindMatch] FAILED: {t.Exception?.GetBaseException().Message}"),
                        TaskContinuationOptions.OnlyOnFaulted);
                }
                else
                {
                    _rootNavigationStore.CurrentViewModel = _createBattleViewModel();
                }

            }, () => CanConfirm);

            CancelSearchCommand = new RelayCommand(async () =>
            {
                await _matchmaking!.CancelAsync(_userStore.BattlePlayerID);
                IsSearching = false;
            });

            BackCommand = new RelayCommand(() =>
            {
                _rootNavigationStore.CurrentViewModel = createOnlineBattleShellViewModel();
            });
        }

        // ── Matchmaking callbacks ─────────────────────────────────────────────

        private void OnMatchFound(MatchFoundData data)
        {
            _userStore.ActiveSessionId = data.SessionId;

            // FIX #10 (carried forward): create the battle service here so
            // BattleViewModel sees a non-null BattleService and enters online mode.
            // FIX #1 (carried forward): UserStore.BattleService setter now actually
            // stores the value, so this assignment is no longer silently discarded.
            _userStore.BattleService = new OnlineBattleService(
                data.SessionId,
                _userStore.BattlePlayerID,
                _serverBaseUrl);

            // Switch to the battle screen on the UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsSearching = false;
                _rootNavigationStore.CurrentViewModel = _createBattleViewModel();
            });
        }

        private void OnQueued()
        {
            // IsSearching is already true — nothing extra needed
        }

        private void OnCancelled()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsSearching = false;
            });
        }

        // ── Cleanup ───────────────────────────────────────────────────────────
        public void Dispose()
        {
            if (_matchmaking is null) return;
            _matchmaking.OnMatchFound -= OnMatchFound;
            _matchmaking.OnQueued -= OnQueued;
            _matchmaking.OnCancelled -= OnCancelled;
        }
    }

    // ── ConnectorSlotEntry ────────────────────────────────────────────────────
    public class ConnectorSlotEntry : ViewModelBase
    {
        public int PokedexId { get; }
        public string Name { get; }
        public string SpriteUrl { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private int? _pickOrder;
        public int? PickOrder
        {
            get => _pickOrder;
            set => SetProperty(ref _pickOrder, value);
        }

        public ConnectorSlotEntry(BattlerPokemon pokemon)
        {
            PokedexId = pokemon.PokedexID;
            Name = pokemon.Name ?? $"#{pokemon.PokedexID}";
            SpriteUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokemon.PokedexID}.png";
        }

        public ConnectorSlotEntry(int pokedexId, string name)
        {
            PokedexId = pokedexId;
            Name = name;
            SpriteUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokedexId}.png";
        }

        public ConnectorSlotEntry()
        {
            PokedexId = 0;
            Name = "?";
            SpriteUrl = "";
        }
    }
}