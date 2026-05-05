using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Factory
{
    public class ServiceResolver
    {
        private readonly bool _isOnline;

        public ServiceResolver(bool isOnline)
        {
            _isOnline = isOnline;
        }

        public IProfileService ProfileService =>
            _isOnline
                ? new OnlineProfileService(new ProfileApiClient())
                : new LocalProfileService();

        public IUserService UserService =>
            _isOnline
                ? new OnlineUserService(new UserApiClient())
                : new LocalUserService();
        public IGameModeChooserService GameModeChooserService =>
            new LocalGameModeChooserService();   // online impl added when server exists
        public IBattleHistoryService BattleHistoryService =>
            new LocalBattleHistoryService();   // online impl added when server exists
        public ITeamService TeamService =>
            new LocalTeamService();       // online impl added when server exists

        public IPokedexService PokedexService =>
            new LocalPokedexService();    // read-only game data, likely always local
        public IAbilityService AbilityService =>
            new LocalAbilityService();   // pure game data — always local
        public IItemService ItemService =>
            new LocalItemService();   // pure game data — always local
        public IMoveService MoveService =>
            new LocalMoveService();   // pure game data — always local
        public IPokemonService PokemonService =>
            new LocalPokemonService();   // online impl added when server exists

        // add other services here as you build them
    }
}
