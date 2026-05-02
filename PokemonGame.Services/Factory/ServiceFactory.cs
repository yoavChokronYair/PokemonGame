using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Data.Repositories.PokemonGame.Services.Data.Repositories;

namespace PokemonGame.Services.Factory
{
    public sealed class ServiceFactory
    {
        private static readonly Lazy<ServiceFactory> _instance = new(() => new ServiceFactory());
        public static ServiceFactory Instance => _instance.Value;

        // Player & Auth
        internal UserRepository UserRepository { get; }
        internal OnlinePlayerRepository OnlinePlayerRepository { get; }

        // Teams & Battling
        internal TeamRepository TeamRepository { get; }
        internal TeamMemberRepository TeamMemberRepository { get; }
        internal BattleRepository BattleRepository { get; }
        internal ParticipantRepository ParticipantRepository { get; }
        internal BattlePlayerStatsRepository BattlePlayerStatsRepository { get; }
        internal BattlePlayerSettingsRepository BattlePlayerSettingsRepository { get; }

        // Pokemon & Moves
        internal PokemonRepository PokemonRepository { get; }
        internal BattlerPokemonRepository BattlerPokemonRepository { get; }
        internal PokemonStatsRepository PokemonStatsRepository { get; }

        // Move tree repositories
        internal MoveRepository MoveRepository { get; }
        internal AttemptRepository AttemptRepository { get; }
        internal CascadeStepRepository CascadeStepRepository { get; }
        internal EffectRepository EffectRepository { get; }
        internal SequenceStepRepository SequenceStepRepository { get; }
        internal MultiStatChangeRepository MultiStatChangeRepository { get; }
        internal NumberRepository NumberRepository { get; }
        internal WeightedEntryRepository WeightedEntryRepository { get; }
        internal ConditionRepository ConditionRepository { get; }

        // Static Lookups (Cached)
        internal AbilityRepository AbilityRepository { get; }
        internal ItemRepository ItemRepository { get; }
        internal BreedingRepository BreedingRepository { get; }
        internal PokedexEntryRepository PokedexEntryRepository { get; }
        internal MoveLearnsetRepository MoveLearnsetRepository { get; }

        private ServiceFactory()
        {
            var db = new SQLiteConnectionService(
                "..\\..\\..\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db");

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
    }
}