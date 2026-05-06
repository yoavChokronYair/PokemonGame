using PokemonGame.Services.ApiClients;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Factory
{
    public class ServiceResolver
    {
        public bool IsOnline { get; private set; }
        private readonly string? _serverUrl;

        public ServiceResolver(bool isOnline, string? serverUrl = null)
        {
            IsOnline = isOnline;
            _serverUrl = serverUrl;
        }
        public void SetOnline(string serverUrl)
        {
            IsOnline = true;
            // rebuild services with online implementations
        }

        public void SetOffline()
        {
            IsOnline = false;
        }

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
        public IPokedexService GetPokedexService() => new LocalPokedexService();    // read-only game data, likely always local
        public IAbilityService GetAbilityService() => new LocalAbilityService();   // pure game data — always local
        public IItemService GetItemService() => new LocalItemService();   // pure game data — always local
        public IMoveService GetMoveService() => new LocalMoveService();   // pure game data — always local
        public IPokemonService GetPokemonService() => new LocalPokemonService();   // online impl added when server exists
        // add other services here as you build them
    }
}
