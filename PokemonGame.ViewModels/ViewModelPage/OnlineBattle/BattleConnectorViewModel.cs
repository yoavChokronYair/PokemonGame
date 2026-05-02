using System.Collections.ObjectModel;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Enums;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Network.Packets;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;
using PokemonGame.ViewModels.ViewModelPage.Online;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class BattleConnectorViewModel : ViewModelBase
    {
        private readonly UserStore _userStore;
        private readonly NavigationStore _rootNavigationStore;
        private readonly TeamBuilderService _teamBuilderService;
        private readonly Func<BattleViewModel> _createBattleViewModel;

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

        public bool IsEasy
        {
            get => BotDifficulty == BotDifficulty.Easy;
            set { if (value) BotDifficulty = BotDifficulty.Easy; }
        }
        public bool IsMedium
        {
            get => BotDifficulty == BotDifficulty.Medium;
            set { if (value) BotDifficulty = BotDifficulty.Medium; }
        }
        public bool IsHard
        {
            get => BotDifficulty == BotDifficulty.Hard;
            set { if (value) BotDifficulty = BotDifficulty.Hard; }
        }

        public int SelectedCount => TeamSlots.Count(s => s.IsSelected);
        public bool CanConfirm => SelectedCount == RequiredCount;
        public string ConfirmLabel => IsOffline ? "▶  Fight Bot" : "🔍  Find Match";

        public RelayCommand<ConnectorSlotEntry> ToggleCommand { get; }
        public RelayCommand ConfirmCommand { get; }
        public RelayCommand BackCommand { get; }
        private readonly Func<OnlineServerBattleViewModel> _createOnlineBattleViewModel;

        public BattleConnectorViewModel(
            UserStore userStore,
            NavigationStore rootNavigationStore,
            Func<BattleViewModel> createBattleViewModel,
            Func<OnlineBattleShellViewModel> createOnlineBattleShellViewModel,
            Func<OnlineServerBattleViewModel> createOnlineBattleViewModel)  // ADD
        {
            _createOnlineBattleViewModel = createOnlineBattleViewModel;
            _userStore = userStore;
            _rootNavigationStore = rootNavigationStore;
            _teamBuilderService = new TeamBuilderService();
            _createBattleViewModel = createBattleViewModel;

            var session = _userStore.BattleSesion;

            RequiredCount = session.BattleMode switch
            {
                BattleMode.halfTeam => 3,
                BattleMode.TwoThirdsTeam => 4,
                BattleMode.fullTeam => 6,
                _ => 6
            };

            // Load player team members
            if (session.SelectedTeamId.HasValue)
            {
                var members = _teamBuilderService.GetTeamMembers(session.SelectedTeamId.Value);
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

            // Generate rival team preview
            var service = new PokemonService();
            var rivalResults = service.GenerateRandomTeam(count: RequiredCount, level: 50);

            // Save rival IDs to session for BattleViewModel to use
            session.RivalPokemonIds = rivalResults.Select(r => r.Battler.PokedexID).ToList();

            // Populate rival slots with real pokemon
            foreach (var r in rivalResults)
                RivalSlots.Add(new ConnectorSlotEntry(r.Battler.PokedexID, r.Battler.Name));

            // Pad to 6 slots with empty placeholders for UI symmetry
            for (int i = RivalSlots.Count; i < 6; i++)
                RivalSlots.Add(new ConnectorSlotEntry());

            ToggleCommand = new RelayCommand<ConnectorSlotEntry>(slot =>
            {
                if (slot == null) return;

                if (slot.IsSelected)
                {
                    int removedOrder = slot.PickOrder ?? 0;
                    slot.IsSelected = false;
                    slot.PickOrder = null;

                    // Shift down all slots with higher order
                    foreach (var s in TeamSlots.Where(s => s.PickOrder > removedOrder))
                        s.PickOrder--;
                }
                else if (SelectedCount < RequiredCount)
                {
                    slot.IsSelected = true;
                    slot.PickOrder = SelectedCount; // SelectedCount already updated
                }
            });

            ConfirmCommand = new RelayCommand(() =>
            {
                // 1. Sync the session data from the UI state
                var selectedIds = TeamSlots
                    .Where(s => s.IsSelected)
                    .OrderBy(s => s.PickOrder)
                    .Select(s => s.PokedexId)
                    .ToList();

                _userStore.BattleSesion.SelectedPokemonIds = selectedIds;
                _userStore.BattleSesion.BotDifficulty = BotDifficulty;

                if (_userStore.BattleSesion.IsOnlineMode)
                {
                    IsSearching = true;

                    // 2. Build the packet using UserStore data
                    var packet = new FindMatchPacket
                    {
                        PlayerId = _userStore.BattlePlayerID,
                        PlayerName = _userStore.Username,
                        BattleMode = _userStore.BattleSesion.BattleMode.ToString(),
                        IsOneVOne = _userStore.BattleSesion.IsOneVOne,
                        TeamId = _userStore.BattleSesion.SelectedTeamId ?? 0,
                        // Mapping the list of IDs to the expected DTO list
                        Team = selectedIds.Select(id => new BattlePokemonDto { PokedexId = id }).ToList()
                    };

                    // 3. Setup the Navigation Callback
                    // Use a local variable to prevent multiple subscriptions/memory leaks
                    Action<object> matchHandler = null!;
                    matchHandler = _ =>
                    {
                        _userStore.OnlineBattleService!.OnMatchFound -= matchHandler;

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _rootNavigationStore.CurrentViewModel = _createOnlineBattleViewModel();
                        });
                    };

                    _userStore.OnlineBattleService!.OnMatchFound += matchHandler;

                    // 4. Fire the async request
                    _ = _userStore.OnlineBattleService!.FindMatchAsync(packet);
                }
                else
                {
                    // Offline flow
                    _rootNavigationStore.CurrentViewModel = _createBattleViewModel();
                }
            }, () => CanConfirm);

            BackCommand = new RelayCommand(() =>
            {
                _rootNavigationStore.CurrentViewModel = createOnlineBattleShellViewModel();
            });
        }
    }

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

        // From DB BattlerPokemon
        public ConnectorSlotEntry(BattlerPokemon pokemon)
        {
            PokedexId = pokemon.PokedexID;
            Name = pokemon.Name ?? $"#{pokemon.PokedexID}";
            SpriteUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokemon.PokedexID}.png";
        }

        // From generated rival (id + name known)
        public ConnectorSlotEntry(int pokedexId, string name)
        {
            PokedexId = pokedexId;
            Name = name;
            SpriteUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokedexId}.png";
        }

        // Empty placeholder slot
        public ConnectorSlotEntry()
        {
            PokedexId = 0;
            Name = "?";
            SpriteUrl = "";
        }
    }
}