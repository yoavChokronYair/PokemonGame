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

        // add other services here as you build them
    }
}
