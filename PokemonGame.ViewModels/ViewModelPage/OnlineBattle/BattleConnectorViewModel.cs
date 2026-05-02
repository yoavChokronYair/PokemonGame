using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Enums;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;

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

        public BattleConnectorViewModel(
            UserStore userStore,
            NavigationStore rootNavigationStore,
            Func<BattleViewModel> createBattleViewModel)
        {
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

            // Rival placeholder slots
            for (int i = 0; i < 6; i++)
                RivalSlots.Add(new ConnectorSlotEntry());

            // ← THIS was missing — clicking did nothing without it
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
                    slot.PickOrder = SelectedCount; // already incremented by IsSelected change
                }
            });

            ConfirmCommand = new RelayCommand(() =>
            {
                session.SelectedPokemonIds = TeamSlots
                    .Where(s => s.IsSelected)
                    .Select(s => s.PokedexId)
                    .ToList();
                session.BotDifficulty = BotDifficulty;

                if (session.IsOnlineMode)
                {
                    IsSearching = true;
                    // TODO: trigger matchmaking
                }
                else
                {
                    _rootNavigationStore.CurrentViewModel = _createBattleViewModel();
                }
            }, () => CanConfirm);

            BackCommand = new RelayCommand(() =>
            {
                // TODO: navigate back
            });
        }
    }

    public class ConnectorSlotEntry : ViewModelBase
    {
        private int? _pickOrder;
        public int? PickOrder
        {
            get => _pickOrder;
            set => SetProperty(ref _pickOrder, value);
        }
        public int PokedexId { get; }
        public string Name { get; }
        public string SpriteUrl { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ConnectorSlotEntry(BattlerPokemon pokemon)
        {
            PokedexId = pokemon.PokedexID;
            Name = pokemon.Name ?? $"#{pokemon.PokedexID}";
            SpriteUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokemon.PokedexID}.png";
        }

        // Placeholder for rival unknown slots
        public ConnectorSlotEntry()
        {
            PokedexId = 0;
            Name = "?";
            SpriteUrl = "";
        }
    }
}