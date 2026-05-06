using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Sync;

namespace PokemonGame.Services.Factory
{
    public sealed class SyncFactory : IDisposable
    {
        private readonly DbSyncService _sync;

        internal SyncFactory(IDbConnectionService local, IDbConnectionService remote, int intervalSeconds = 60)
        {
            _sync = new DbSyncService(local, remote, intervalSeconds);
        }

        // ── Full sync ─────────────────────────────────────────────────────────
        public void Start() => _sync.Start();
        public void Stop() => _sync.Stop();
        public Task SyncNowAsync() => _sync.SyncNowAsync();

        // ── Targeted sync ─────────────────────────────────────────────────────
        public Task SyncPlayerAsync(int battlePlayerId) =>
            _sync.SyncPlayerNowAsync(battlePlayerId);
        public Task SyncUserAsync(int userId) =>
            _sync.SyncUserNowAsync(userId);

        // ── Events ────────────────────────────────────────────────────────────
        public void Subscribe(Action<string> onSuccess, Action<string> onFailure)
        {
            _sync.OnSyncCompleted += onSuccess;
            _sync.OnSyncFailed += onFailure;
        }

        public void Unsubscribe(Action<string> onSuccess, Action<string> onFailure)
        {
            _sync.OnSyncCompleted -= onSuccess;
            _sync.OnSyncFailed -= onFailure;
        }

        public void Dispose() => _sync.Dispose();
    }
}