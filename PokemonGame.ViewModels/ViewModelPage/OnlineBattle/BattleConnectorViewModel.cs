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
        private readonly Func<OnlineBattleShellViewModel> _createOnlineBattleShellViewModel;
        private readonly IMatchmakingService? _matchmaking;
        private readonly string _serverBaseUrl;

        private readonly List<dynamic> _rivalResults = new();
        private bool _matchFoundHandled;
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
                if (SetProperty(ref _botDifficulty, value))
                {
                    OnPropertyChanged(nameof(IsEasy));
                    OnPropertyChanged(nameof(IsMedium));
                    OnPropertyChanged(nameof(IsHard));
                }
            }
        }

        public bool IsEasy
        {
            get => BotDifficulty == BotDifficulty.Easy;
            set
            {
                if (value)
                    BotDifficulty = BotDifficulty.Easy;
            }
        }

        public bool IsMedium
        {
            get => BotDifficulty == BotDifficulty.Medium;
            set
            {
                if (value)
                    BotDifficulty = BotDifficulty.Medium;
            }
        }

        public bool IsHard
        {
            get => BotDifficulty == BotDifficulty.Hard;
            set
            {
                if (value)
                    BotDifficulty = BotDifficulty.Hard;
            }
        }

        public int SelectedCount => TeamSlots.Count(s => s.IsSelected);

        public bool CanConfirm => SelectedCount == RequiredCount;

        public string ConfirmLabel => IsOffline ? "▶  Fight Bot" : "🔍  Find Match";

        public RelayCommand<ConnectorSlotEntry> ToggleCommand { get; }
        public IAsyncRelayCommand ConfirmCommand { get; }
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
            _createOnlineBattleShellViewModel = createOnlineBattleShellViewModel;
            _serverBaseUrl = _userStore.ServerBaseUrl;
            _matchmaking = userStore.Matchmaking;

            SubscribeToMatchmakingEvents();

            var session = _userStore.BattleSesion;

            RequiredCount = session.BattleMode switch
            {
                BattleMode.halfTeam => 3,
                BattleMode.TwoThirdsTeam => 4,
                BattleMode.fullTeam => 6,
                _ => 6
            };

            LoadPlayerTeamSlots(session);
            LoadRivalPreview(session);

            ToggleCommand = new RelayCommand<ConnectorSlotEntry>(ToggleSlot);

            ConfirmCommand = new AsyncRelayCommand(
                ConfirmAsync,
                () => CanConfirm && !IsSearching);

            CancelSearchCommand = new RelayCommand(async () =>
            {
                await CancelSearchAsync();
            });

            BackCommand = new RelayCommand(() =>
            {
                _rootNavigationStore.CurrentViewModel =
                    _createOnlineBattleShellViewModel();
            });
        }

        // ─────────────────────────────────────────────────────────────
        // Setup
        // ─────────────────────────────────────────────────────────────

        private void SubscribeToMatchmakingEvents()
        {
            if (_matchmaking is null)
                return;

            _matchmaking.OnMatchFound += OnMatchFound;
            _matchmaking.OnQueued += OnQueued;
            _matchmaking.OnCancelled += OnCancelled;
            _matchmaking.OnError += OnMatchmakingError;
        }

        private void LoadPlayerTeamSlots(BattleSession session)
        {
            int teamId;

            if (session.SelectedTeamId.HasValue)
            {
                teamId = session.SelectedTeamId.Value;
            }
            else
            {
                return;
            }

            var members = _teamService.GetTeamMembers(teamId);

            foreach (var member in members)
            {
                var slot = new ConnectorSlotEntry(member);

                slot.PropertyChanged += (_, _) =>
                {
                    RefreshSelectionState();
                };

                TeamSlots.Add(slot);
            }
        }

        private void LoadRivalPreview(dynamic session)
        {
            var pokemonService = _userStore.Resolver.GetPokemonService();

            var rivalResults = pokemonService
                .GenerateRandomTeam(count: RequiredCount, level: 50)
                .ToList();

            _rivalResults.Clear();
            _rivalResults.AddRange(rivalResults.Cast<dynamic>());

            session.RivalPokemonIds = rivalResults
                .Select(r => r.Battler.PokedexID)
                .ToList();

            foreach (var result in rivalResults)
            {
                RivalSlots.Add(new ConnectorSlotEntry(
                    result.Battler.PokedexID,
                    result.Battler.Name));
            }

            for (int i = RivalSlots.Count; i < 6; i++)
            {
                RivalSlots.Add(new ConnectorSlotEntry());
            }

            IsRivalReady = true;
        }

        // ─────────────────────────────────────────────────────────────
        // Selection
        // ─────────────────────────────────────────────────────────────
        private void UnsubscribeFromMatchmakingEvents()
        {
            if (_matchmaking is null)
                return;

            _matchmaking.OnMatchFound -= OnMatchFound;
            _matchmaking.OnQueued -= OnQueued;
            _matchmaking.OnCancelled -= OnCancelled;
            _matchmaking.OnError -= OnMatchmakingError;
        }
        private void ToggleSlot(ConnectorSlotEntry? slot)
        {
            if (slot == null)
                return;

            if (slot.IsSelected)
            {
                int removedOrder = slot.PickOrder ?? 0;

                slot.IsSelected = false;
                slot.PickOrder = null;

                foreach (var otherSlot in TeamSlots.Where(s => s.PickOrder > removedOrder))
                {
                    otherSlot.PickOrder--;
                }
            }
            else
            {
                if (SelectedCount >= RequiredCount)
                    return;

                slot.IsSelected = true;
                slot.PickOrder = SelectedCount;
            }

            RefreshSelectionState();
        }

        private void RefreshSelectionState()
        {
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(CanConfirm));
            OnPropertyChanged(nameof(RequiredCountLabel));

            ConfirmCommand.NotifyCanExecuteChanged();
        }

        private List<int> GetSelectedPokemonIds()
        {
            return TeamSlots
                .Where(s => s.IsSelected)
                .OrderBy(s => s.PickOrder)
                .Select(s => s.PokedexId)
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────
        // Confirm
        // ─────────────────────────────────────────────────────────────

        private async Task ConfirmAsync()
        {
            var selectedIds = GetSelectedPokemonIds();

            _userStore.BattleSesion.SelectedPokemonIds = selectedIds;
            _userStore.BattleSesion.BotDifficulty = BotDifficulty;

            if (_userStore.BattleSesion.IsOnlineMode)
            {
                await ConfirmOnlineAsync(selectedIds);
                return;
            }

            ConfirmOffline(selectedIds);
        }

        private async Task ConfirmOnlineAsync(List<int> selectedIds)
        {
            if (_matchmaking == null)
                return;
            if (_userStore.BattlePlayerID <= 0)
            {
                Console.WriteLine(
                    $"[FindMatch] BLOCKED: invalid BattlePlayerID={_userStore.BattlePlayerID}, User={_userStore.Username}");

                IsSearching = false;
                ConfirmCommand.NotifyCanExecuteChanged();
                return;
            }
            try
            {
                IsSearching = true;
                ConfirmCommand.NotifyCanExecuteChanged();

                await _matchmaking.FindMatchAsync(new MatchmakingRequest
                {
                    PlayerId = _userStore.BattlePlayerID,
                    PlayerName = _userStore.Username,
                    BattleMode = _userStore.BattleSesion.BattleMode.ToString(),
                    IsOneVOne = _userStore.BattleSesion.IsOneVOne,
                    TeamId = _userStore.BattleSesion.SelectedTeamId ?? 0,
                    SelectedPokemonIds = selectedIds
                });
            }
            catch (Exception ex)
            {
                IsSearching = false;
                ConfirmCommand.NotifyCanExecuteChanged();

                Console.WriteLine($"[FindMatch] FAILED: {ex.Message}");
            }
        }

        private void ConfirmOffline(List<int> selectedIds)
        {
            var session = _userStore.BattleSesion;
            var translator = new TeamTranslator();

            var fullTeam = translator.LoadTeamByID(_userStore.BattlePlayerID);

            var playerRoster = BuildPlayerRoster(
                fullTeam,
                selectedIds,
                session.BattleMode);

            session.ResolvedPlayerTeam = PokemonTeam.Create(playerRoster);

            var rivalRoster = BuildRivalRoster(translator);

            session.ResolvedBotTeam = PokemonTeam.Create(rivalRoster);

            _rootNavigationStore.CurrentViewModel = _createBattleViewModel();
        }

        private List<PokemonState> BuildPlayerRoster(
            PokemonTeam fullTeam,
            List<int> selectedIds,
            BattleMode battleMode)
        {
            var allPokemon = Enumerable.Range(0, fullTeam.getAllPokemonCount())
                .Select(fullTeam.GetPokemonAt)
                .ToList();

            if (battleMode == BattleMode.fullTeam || selectedIds.Count == 0)
                return allPokemon;

            return selectedIds
                .Select(id => allPokemon.FirstOrDefault(p => p.PokedexId == id))
                .Where(p => p != null)
                .ToList()!;
        }

        private List<PokemonState> BuildRivalRoster(TeamTranslator translator)
        {
            var session = _userStore.BattleSesion;
            var roster = new List<PokemonState>();

            foreach (int id in session.RivalPokemonIds)
            {
                dynamic result = _rivalResults.First(r => r.Battler.PokedexID == id);

                PokemonState pokemon = translator.TranslateToDomain(result);

                roster.Add(pokemon);
            }

            return roster;
        }

        // ─────────────────────────────────────────────────────────────
        // Cancel / matchmaking callbacks
        // ─────────────────────────────────────────────────────────────

        private async Task CancelSearchAsync()
        {
            if (_matchmaking == null)
                return;

            try
            {
                await _matchmaking.CancelAsync(_userStore.BattlePlayerID);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CancelSearch] FAILED: {ex.Message}");
            }
            finally
            {
                IsSearching = false;
                ConfirmCommand.NotifyCanExecuteChanged();
            }
        }

        private async void OnMatchFound(MatchFoundData data)
        {
            if (_matchFoundHandled)
                return;

            _matchFoundHandled = true;

            UnsubscribeFromMatchmakingEvents();

            try
            {
                if (_userStore.BattlePlayerID <= 0)
                {
                    Console.WriteLine(
                        $"[OnMatchFound] BLOCKED: invalid BattlePlayerID={_userStore.BattlePlayerID}, User={_userStore.Username}");

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsSearching = false;
                        ConfirmCommand.NotifyCanExecuteChanged();
                    });

                    return;
                }

                _userStore.ActiveSessionId = data.SessionId;

                if (_userStore.BattleService is not null)
                {
                    try
                    {
                        await _userStore.BattleService.DisconnectAsync();
                    }
                    catch
                    {
                        // ignore old connection cleanup failure
                    }
                }

                _userStore.BattleService = new OnlineBattleService(
                    data.SessionId,
                    _userStore.BattlePlayerID,
                    _serverBaseUrl);

                await _userStore.BattleService.ConnectAsync();

                // Real fix:
                // Do not open BattleViewModel until the first StateUpdated snapshot arrives.
                await _userStore.BattleService.WaitForInitialStateAsync();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    IsSearching = false;
                    ConfirmCommand.NotifyCanExecuteChanged();
                    _rootNavigationStore.CurrentViewModel = _createBattleViewModel();
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    IsSearching = false;
                    ConfirmCommand.NotifyCanExecuteChanged();
                });

                Console.WriteLine($"[OnMatchFound] FAILED: {ex.Message}");
            }
        }

        private void OnQueued()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsSearching = true;
                ConfirmCommand.NotifyCanExecuteChanged();
            });
        }

        private void OnCancelled()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsSearching = false;
                ConfirmCommand.NotifyCanExecuteChanged();
            });
        }

        private void OnMatchmakingError(Exception ex)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsSearching = false;
                ConfirmCommand.NotifyCanExecuteChanged();
            });

            Console.WriteLine($"[Matchmaking] ERROR: {ex.Message}");
        }

        // ─────────────────────────────────────────────────────────────
        // Cleanup
        // ─────────────────────────────────────────────────────────────

        public void Dispose()
        {
            UnsubscribeFromMatchmakingEvents();

            if (_matchmaking is not null)
            {
                _ = _matchmaking.DisconnectAsync();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // ConnectorSlotEntry
    // ─────────────────────────────────────────────────────────────

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
            SpriteUrl =
                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokemon.PokedexID}.png";
        }

        public ConnectorSlotEntry(int pokedexId, string name)
        {
            PokedexId = pokedexId;
            Name = name;
            SpriteUrl =
                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokedexId}.png";
        }

        public ConnectorSlotEntry()
        {
            PokedexId = 0;
            Name = "?";
            SpriteUrl = "";
        }
    }
}