using PokemonGame.Server.Hubs;

namespace PokemonGame.Server.Services
{
    public class BattleSessionCleanupService : BackgroundService
    {
        private readonly IBattleSessionRegistry _sessionRegistry;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

        public BattleSessionCleanupService(IBattleSessionRegistry sessionRegistry)
        {
            _sessionRegistry = sessionRegistry;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IReadOnlyList<ServerBattleSession> timedOutSessions =
                    _sessionRegistry.GetTimedOutSessions(_timeout);

                foreach (var session in timedOutSessions)
                {
                    _sessionRegistry.Remove(session.SessionId);
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}