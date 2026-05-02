using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Data.Repositories.PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Handler;

namespace PokemonGame.Services.Factory
{
    public sealed class ServiceFactory
    {
        // Client-side singleton (uses bundled DB path)
        private static readonly Lazy<ServiceFactory> _instance = new(() =>
            new ServiceFactory("..\\..\\..\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db"));
        public static ServiceFactory Instance => _instance.Value;

        // Repositories
        private readonly IDbConnectionService _db;
        public string GetConnectionString() => _db.ConnectionString;
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

        // Single constructor — everything flows through here
        public ServiceFactory(string dbPath)
        {
            var db = new SQLiteConnectionService(dbPath);
            _db = db;

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

        // ── Service factory methods (instance, no duplication) ────────────────

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

    }
}