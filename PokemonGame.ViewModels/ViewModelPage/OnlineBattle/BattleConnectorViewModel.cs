using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.Translators;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;


namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class BattleConnectorViewModel : ViewModelBase
    {
        private readonly UserStore _userStore;
        private readonly NavigationStore _rootNavigationStore;
        private readonly ITeamService _teamService;
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

            var session = _userStore.BattleSesion;

            RequiredCount = session.BattleMode switch
            {
                BattleMode.halfTeam => 3,
                BattleMode.TwoThirdsTeam => 4,
                BattleMode.fullTeam => 6,
                _ => 6
            };

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

            var service = _userStore.Resolver.GetPokemonService();
            var rivalResults = service.GenerateRandomTeam(count: RequiredCount, level: 50);

            session.RivalPokemonIds = rivalResults.Select(r => r.Battler.PokedexID).ToList();

            foreach (var r in rivalResults)
                RivalSlots.Add(new ConnectorSlotEntry(r.Battler.PokedexID, r.Battler.Name));

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

                    foreach (var s in TeamSlots.Where(s => s.PickOrder > removedOrder))
                        s.PickOrder--;
                }
                else if (SelectedCount < RequiredCount)
                {
                    slot.IsSelected = true;
                    slot.PickOrder = SelectedCount;
                }
            });

            ConfirmCommand = new RelayCommand(() =>
            {
                var selectedIds = TeamSlots
                    .Where(s => s.IsSelected)
                    .OrderBy(s => s.PickOrder)
                    .Select(s => s.PokedexId)
                    .ToList();

                _userStore.BattleSesion.SelectedPokemonIds = selectedIds;
                _userStore.BattleSesion.BotDifficulty = BotDifficulty;

                // ── Build and store player team ───────────────────────────────────
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
                    playerRoster = Enumerable.Range(0, fullTeam.getAllPokemonCount())
                        .Select(i => fullTeam.GetPokemonAt(i))
                        .Where(p => selectedIds.Contains(p.PokedexId))
                        .ToList();
                }

                while (playerRoster.Count < 6)
                    playerRoster.Add(fullTeam.GetPokemonAt(0));

                _userStore.BattleSesion.ResolvedPlayerTeam = PokemonTeam.Create(playerRoster);

                // ── Build and store bot team ──────────────────────────────────────
                var rivalRoster = session.RivalPokemonIds
                    .Select(id => translator.TranslateToDomain(
                        rivalResults.First(r => r.Battler.PokedexID == id)))
                    .ToList();

                while (rivalRoster.Count < 6)
                    rivalRoster.Add(rivalRoster[0]);

                _userStore.BattleSesion.ResolvedBotTeam = PokemonTeam.Create(rivalRoster);

                // ── Navigate ──────────────────────────────────────────────────────
                _rootNavigationStore.CurrentViewModel = _createBattleViewModel();

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