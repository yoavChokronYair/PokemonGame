namespace PokemonGame.Services.Data.ConnectionsService
{
    /// <summary>
    /// Wraps a local and a remote <see cref="IDbConnectionService"/> so that:
    /// <list type="bullet">
    ///   <item>All <b>reads</b> come from the local DB (fast, always available).</item>
    ///   <item>All <b>writes</b> hit the local DB first, then the remote DB
    ///         (fire-and-forget, failures are logged but never thrown to the caller).</item>
    /// </list>
    /// This means the game is always playable offline; the remote DB is a
    /// best-effort replica that the local DB is periodically seeded from via
    /// <see cref="DbSyncService"/>.
    /// </summary>
    public sealed class DualDbConnectionService : IDbConnectionService
    {
        private readonly IDbConnectionService _local;
        private readonly IDbConnectionService _remote;

        /// <summary>The connection string of the local (primary) database.</summary>
        public string ConnectionString => _local.ConnectionString;

        /// <param name="local">Local SQLite database — used for all reads and as primary write target.</param>
        /// <param name="remote">Remote database — receives writes asynchronously after the local write succeeds.</param>
        public DualDbConnectionService(IDbConnectionService local, IDbConnectionService remote)
        {
            _local = local ?? throw new ArgumentNullException(nameof(local));
            _remote = remote ?? throw new ArgumentNullException(nameof(remote));
        }

        // ── Reads — local only ────────────────────────────────────────────────

        public T QuerySingle<T>(string sql, object parameters = null) where T : new()
            => _local.QuerySingle<T>(sql, parameters);

        public T QueryScalar<T>(string sql, object parameters = null)
            => _local.QueryScalar<T>(sql, parameters);

        public List<T> QueryScalarList<T>(string sql, object parameters = null)
            => _local.QueryScalarList<T>(sql, parameters);

        public List<T> Query<T>(string sql) where T : new()
            => _local.Query<T>(sql);

        public List<T> Query<T>(string sql, object parameters) where T : new()
            => _local.Query<T>(sql, parameters);

        // ── Writes — local first, then remote (fire-and-forget) ───────────────

        public int Execute(string sql, object parameters = null)
        {
            int result = _local.Execute(sql, parameters);
            FireAndForget(() => _remote.Execute(sql, parameters), sql);
            return result;
        }

        public int ExecuteAndGetLastId(string sql, object parameters = null)
        {
            // The returned ID is the local row ID.
            // The remote row ID may differ if rows diverge, which is acceptable
            // because the local ID is the one the game uses at runtime.
            int localId = _local.ExecuteAndGetLastId(sql, parameters);
            FireAndForget(() => _remote.ExecuteAndGetLastId(sql, parameters), sql);
            return localId;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Executes <paramref name="action"/> on a <see cref="ThreadPool"/> thread.
        /// Any exception is swallowed and logged so that a remote outage can never
        /// crash or block the game.
        /// </summary>
        private static void FireAndForget(Action action, string context)
        {
            Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    // Intentionally swallowed — remote failures must not surface to the game.
                    Console.WriteLine($"[DualDb] Remote write failed ({context}): {ex.Message}");
                }
            });
        }
    }
}