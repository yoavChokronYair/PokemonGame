using PokemonGame.Services.ApiClients;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;
using PokemonGame.Services.Services;

namespace PokemonGame.Services.Factory
{
    public class ServiceResolver
    {
        public bool IsOnline { get; private set; }
        private string? _serverUrl;

        public ServiceResolver(bool isOnline, string? serverUrl = null)
        {
            IsOnline = isOnline;
            _serverUrl = serverUrl;
        }

        public void SetOnline(string serverUrl)
        {
            IsOnline = true;
            _serverUrl = serverUrl;
        }

        public void SetOffline()
        {
            IsOnline = false;
        }

        // ── Regular services ──────────────────────────────────────────────────
        public IUserService GetUserService() => IsOnline
            ? new OnlineUserService(new UserApiClient(_serverUrl!))
            : new LocalUserService();

        public IProfileService GetProfileService() => IsOnline
            ? new OnlineProfileService(new ProfileApiClient(_serverUrl!))
            : new LocalProfileService();

        public ITeamService GetTeamService() => IsOnline
            ? new OnlineTeamService(new TeamApiClient(_serverUrl!))
            : new LocalTeamService();

        public IGameModeChooserService GetGameModeChooserService() => IsOnline
            ? new OnlineGameModeChooserService(new GameModeApiClient(_serverUrl!))
            : new LocalGameModeChooserService();

        public IBattleHistoryService GetBattleHistoryService() => IsOnline
            ? new OnlineBattleHistoryService(new BattleHistoryApiClient(_serverUrl!))
            : new LocalBattleHistoryService();

        // ── Matchmaking — only valid in online mode ────────────────────────────
        // Note: BattleConnectorViewModel reads UserStore.Matchmaking directly
        // (set once in App.InitialiseOnlineServices) rather than calling this
        // getter every time, so a new connection isn't created on each call.
        public IMatchmakingService MatchmakingService => IsOnline
            ? new OnlineMatchmakingService(_serverUrl!)
            : throw new InvalidOperationException("Matchmaking requires online mode.");

        // ── Battle service ────────────────────────────────────────────────────
        // FIXED: was a public field (`public IBattleService BattleService;`).
        // A field can't be part of an interface contract and is harder to mock.
        // BattleConnectorViewModel.OnMatchFound assigns UserStore.BattleService
        // (not this property directly), but UserStore.BattleService falls back
        // to Resolver.BattleService when its own backing field is null, so this
        // must be a real property, not a field.
        public IBattleService? BattleService { get; set; }

        // ── Pure game-data services — always local ────────────────────────────
        public IPokedexService GetPokedexService() => new LocalPokedexService();
        public IAbilityService GetAbilityService() => new LocalAbilityService();
        public IItemService GetItemService() => new LocalItemService();
        public IMoveService GetMoveService() => new LocalMoveService();
        public IPokemonService GetPokemonService() => new LocalPokemonService();
    }
}