// PokemonGame.Services/Factory/ServiceFactory.cs

using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Data.Sync;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Factory
{
    public sealed class ServiceFactory : IDisposable
    {
        // ── Legacy singleton (local DB only) ──────────────────────────────────
        private static readonly Lazy<ServiceFactory> _instance = new(() =>
            CreateLocal("..\\..\\..\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db"));

        public static ServiceFactory Instance => _instance.Value;
        public SyncFactory? Sync { get; private set; }

        // ── Internal state ────────────────────────────────────────────────────
        private readonly IDbConnectionService _db;
        private DbSyncService? _syncService;

        public string ConnectionString => _db.ConnectionString;

        // ── Repositories (internal — only services inside this assembly use them)
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
        internal BattleTeamSnapshotRepository BattleTeamSnapshotRepository { get; }
        // ── Public services for server use ────────────────────────────────────
        public IUserService UserService { get; }
        public IProfileService ProfileService { get; }
        public IGameModeChooserService GameModeService { get; }
        public ITeamService TeamService { get; }
        public IBattleHistoryService BattleHistoryService { get; }

        // ── Constructor ───────────────────────────────────────────────────────
        public ServiceFactory(IDbConnectionService db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));

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
            BattleTeamSnapshotRepository = new BattleTeamSnapshotRepository(db);


            UserService = new LocalUserService(UserRepository);
            ProfileService = new LocalProfileService(OnlinePlayerRepository, BattlePlayerSettingsRepository,
                                                     BattlePlayerStatsRepository, TeamRepository,
                                                     TeamMemberRepository, BattlerPokemonRepository,
                                                     PokemonRepository);
            GameModeService = new LocalGameModeChooserService(OnlinePlayerRepository,BattlePlayerSettingsRepository);
            TeamService = new LocalTeamService(
                PokemonRepository, AbilityRepository, ItemRepository,
                MoveLearnsetRepository, MoveRepository, AttemptRepository,
                EffectRepository, NumberRepository, SequenceStepRepository,
                PokemonStatsRepository, TeamRepository, TeamMemberRepository,
                BattlerPokemonRepository
            ); 
            BattleHistoryService = new LocalBattleHistoryService(BattleRepository, ParticipantRepository,
                                                                  TeamMemberRepository, BattlerPokemonRepository,
                                                                  PokemonRepository, ItemRepository,
                                                                  OnlinePlayerRepository, BattleTeamSnapshotRepository);

        }

        // ── Factory methods ───────────────────────────────────────────────────
        public static ServiceFactory CreateLocal(string localDbPath) =>
            new ServiceFactory(new SQLiteConnectionService(localDbPath));


        public static ServiceFactory CreateOnline(
            string localDbPath,
            string remoteDbPath,
            int syncIntervalSeconds = 60)
        {
            var local = new SQLiteConnectionService(localDbPath);
            var remote = new SQLiteConnectionService(remoteDbPath);
            var factory = new ServiceFactory(new DualDbConnectionService(local, remote));

            factory.Sync = new SyncFactory(local, remote, syncIntervalSeconds);
            return factory;
        }

        // ── Sync control ──────────────────────────────────────────────────────
        public void StartSync() => _syncService?.Start();
        public void StopSync() => _syncService?.Stop();

        public Task SyncNowAsync() =>
            _syncService?.SyncNowAsync() ?? Task.CompletedTask;

        public void SubscribeToSyncEvents(Action<string> onSuccess, Action<string> onFailure)
        {
            if (_syncService == null) return;
            _syncService.OnSyncCompleted += onSuccess;
            _syncService.OnSyncFailed += onFailure;
        }

        // ── IDisposable ───────────────────────────────────────────────────────
        public void Dispose() => _syncService?.Dispose();
    }
}