using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Data.Repositories.PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Data.Sync;
using PokemonGame.Services.Handler;

namespace PokemonGame.Services.Factory
{
    /// <summary>
    /// Central composition root for all services and repositories.
    /// </summary>
    /// <remarks>
    /// <b>Two creation paths:</b>
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="CreateLocal"/> — offline / story mode.
    ///     All reads and writes go to a single local SQLite file.  
    ///   </item>
    ///   <item>
    ///     <see cref="CreateOnline"/> — online / battle mode.
    ///     Reads come from local; writes go local-first then remote asynchronously.
    ///     A <see cref="DbSyncService"/> pulls remote changes into local on a timer.
    ///     Call <see cref="StartSync"/> after creating an online factory.
    ///   </item>
    /// </list>
    /// The <see cref="Instance"/> singleton remains available for legacy call sites
    /// that use the default local path (e.g., WinForms code-behind).
    /// New code should prefer the explicit factory methods.
    /// </remarks>
    public sealed class ServiceFactory : IDisposable
    {
        // ── Legacy singleton (local DB only) ──────────────────────────────────

        private static readonly Lazy<ServiceFactory> _instance = new(() =>
            CreateLocal("..\\..\\..\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db"));

        /// <summary>
        /// Legacy singleton backed by the bundled local database.
        /// Prefer <see cref="CreateLocal"/> or <see cref="CreateOnline"/> for new code.
        /// </summary>
        public static ServiceFactory Instance => _instance.Value;

        // ── Internal state ────────────────────────────────────────────────────

        private readonly IDbConnectionService _db;
        private DbSyncService? _syncService;

        public string GetConnectionString() => _db.ConnectionString;

        // ── Repositories ──────────────────────────────────────────────────────

        internal UserRepository UserRepository { get; }
        internal OnlinePlayerRepository OnlinePlayerRepository { get; }
        internal TeamRepository TeamRepository { get; }
        internal TeamMemberRepository TeamMemberRepository { get; }
        internal BattleRepository BattleRepository { get; }
        internal ParticipantRepository ParticipantRepository { get; }
        internal BattlePlayerStatsRepository BattlePlayerStatsRepository { get; }
        internal BattlePlayerSettingsRepository BattlePlayerSettingsRepository { get; }
        internal PokemonRepository PokemonRepository { get; }
        internal BattlerPokemonRepository BattlerPokemonRepository { get; }
        internal PokemonStatsRepository PokemonStatsRepository { get; }
        internal MoveRepository MoveRepository { get; }
        internal AttemptRepository AttemptRepository { get; }
        internal CascadeStepRepository CascadeStepRepository { get; }
        internal EffectRepository EffectRepository { get; }
        internal SequenceStepRepository SequenceStepRepository { get; }
        internal MultiStatChangeRepository MultiStatChangeRepository { get; }
        internal NumberRepository NumberRepository { get; }
        internal WeightedEntryRepository WeightedEntryRepository { get; }
        internal ConditionRepository ConditionRepository { get; }
        internal AbilityRepository AbilityRepository { get; }
        internal ItemRepository ItemRepository { get; }
        internal BreedingRepository BreedingRepository { get; }
        internal PokedexEntryRepository PokedexEntryRepository { get; }
        internal MoveLearnsetRepository MoveLearnsetRepository { get; }

        // ── Constructor — everything flows through here ───────────────────────

        /// <param name="db">
        /// The <see cref="IDbConnectionService"/> to use for all repositories.
        /// Pass a <see cref="SQLiteConnectionService"/> for local-only mode, or a
        /// <see cref="DualDbConnectionService"/> for online mode.
        /// </param>
        public ServiceFactory(IDbConnectionService db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            Console.WriteLine($"[ServiceFactory] Created with connection: {db.ConnectionString}");

            UserRepository = new UserRepository(db);
            OnlinePlayerRepository = new OnlinePlayerRepository(db);
            TeamRepository = new TeamRepository(db);
            TeamMemberRepository = new TeamMemberRepository(db);
            BattleRepository = new BattleRepository(db);
            ParticipantRepository = new ParticipantRepository(db);
            BattlePlayerSettingsRepository = new BattlePlayerSettingsRepository(db);
            BattlePlayerStatsRepository = new BattlePlayerStatsRepository(db);
            PokemonRepository = new PokemonRepository(db);
            BattlerPokemonRepository = new BattlerPokemonRepository(db);
            PokemonStatsRepository = new PokemonStatsRepository(db);
            MoveRepository = new MoveRepository(db);
            AttemptRepository = new AttemptRepository(db);
            CascadeStepRepository = new CascadeStepRepository(db);
            EffectRepository = new EffectRepository(db);
            SequenceStepRepository = new SequenceStepRepository(db);
            MultiStatChangeRepository = new MultiStatChangeRepository(db);
            NumberRepository = new NumberRepository(db);
            WeightedEntryRepository = new WeightedEntryRepository(db);
            ConditionRepository = new ConditionRepository(db);
            AbilityRepository = new AbilityRepository(db);
            ItemRepository = new ItemRepository(db);
            BreedingRepository = new BreedingRepository(db);
            PokedexEntryRepository = new PokedexEntryRepository(db);
            MoveLearnsetRepository = new MoveLearnsetRepository(db);
        }

        /// <summary>
        /// Convenience overload — creates a local-only factory from a file path.
        /// Kept for backwards compatibility with the legacy singleton.
        /// </summary>
        public ServiceFactory(string dbPath)
            : this(new SQLiteConnectionService(dbPath)) { }

        // ── Factory methods ───────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="ServiceFactory"/> backed by a single local SQLite file.
        /// Use for offline play, story mode, and unit tests.
        /// </summary>
        /// <param name="localDbPath">Full path to the local .db file.</param>
        public static ServiceFactory CreateLocal(string localDbPath)
        {
            var local = new SQLiteConnectionService(localDbPath);
            return new ServiceFactory(local);
        }

        /// <summary>
        /// Creates a <see cref="ServiceFactory"/> that:
        /// <list type="bullet">
        ///   <item>Reads from the local database (fast, offline-safe).</item>
        ///   <item>Writes to local first, then mirrors to the remote asynchronously.</item>
        ///   <item>Runs a background sync that regularly pulls the remote into local.</item>
        /// </list>
        /// Call <see cref="StartSync"/> on the returned instance to begin periodic sync.
        /// </summary>
        /// <param name="localDbPath">Full path to the local .db file.</param>
        /// <param name="remoteDbPath">Full path (or connection string) for the remote .db file.</param>
        /// <param name="syncIntervalSeconds">How often to pull remote → local (default 60 s).</param>
        public static ServiceFactory CreateOnline(
            string localDbPath,
            string remoteDbPath,
            int syncIntervalSeconds = 60)
        {
            var local = new SQLiteConnectionService(localDbPath);
            var remote = new SQLiteConnectionService(remoteDbPath);
            var dual = new DualDbConnectionService(local, remote);

            var factory = new ServiceFactory(dual);
            factory._syncService = new DbSyncService(local, remote, syncIntervalSeconds);
            return factory;
        }

        // ── Sync control ──────────────────────────────────────────────────────

        /// <summary>
        /// Starts the background sync timer (online mode only).
        /// No-op if this factory was created with <see cref="CreateLocal"/>.
        /// </summary>
        public void StartSync() => _syncService?.Start();

        /// <summary>
        /// Stops the background sync timer without disposing the factory.
        /// No-op in local mode.
        /// </summary>
        public void StopSync() => _syncService?.Stop();

        /// <summary>
        /// Triggers a single remote → local sync immediately and awaits it.
        /// Useful right after login so fresh server data is available instantly.
        /// No-op in local mode.
        /// </summary>
        public Task SyncNowAsync() =>
            _syncService?.SyncNowAsync() ?? Task.CompletedTask;

        /// <summary>
        /// Exposes sync events for UI feedback (e.g., a status bar indicator).
        /// Only meaningful in online mode; safe to subscribe in local mode (events never fire).
        /// </summary>
        public void SubscribeToSyncEvents(Action<string> onSuccess, Action<string> onFailure)
        {
            if (_syncService == null) return;
            _syncService.OnSyncCompleted += onSuccess;
            _syncService.OnSyncFailed += onFailure;
        }

        // ── Service factory methods (unchanged from original) ─────────────────

        public SignUpService CreateSignUpService() =>
            new SignUpService(UserRepository);

        public LogInService CreateLogInService() =>
            new LogInService(UserRepository);

        public TeamBuilderService CreateTeamBuilderService() =>
            new TeamBuilderService(PokemonRepository, AbilityRepository, ItemRepository,
                                   MoveLearnsetRepository, MoveRepository, AttemptRepository,
                                   EffectRepository, NumberRepository, SequenceStepRepository,
                                   PokemonStatsRepository, TeamRepository, TeamMemberRepository,
                                   BattlerPokemonRepository);

        public BattleHistoryService CreateBattleHistoryService() =>
            new BattleHistoryService(BattleRepository, ParticipantRepository, TeamMemberRepository,
                                     BattlerPokemonRepository, PokemonRepository, ItemRepository,
                                     OnlinePlayerRepository);

        public PokemonService CreatePokemonService() =>
            new PokemonService(
                BattlerPokemonRepository,
                PokemonRepository,
                TeamRepository,
                TeamMemberRepository,
                MoveLearnsetRepository,
                PokemonStatsRepository,
                MoveRepository);

        public AbilityService CreateAbilityService() =>
            new AbilityService(AbilityRepository, ConditionRepository, EffectRepository, NumberRepository);

        public ItemService CreateItemService() =>
            new ItemService(ItemRepository, ConditionRepository, EffectRepository, NumberRepository);

        // ── IDisposable ───────────────────────────────────────────────────────

        public void Dispose()
        {
            _syncService?.Dispose();
        }
    }
}