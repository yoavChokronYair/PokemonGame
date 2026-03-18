using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Repositories;

namespace PokemonGame.Services.Factory
{
    public sealed class ServiceFactory
    {
        private static readonly Lazy<ServiceFactory> _instance = new(() => new ServiceFactory());
        public static ServiceFactory Instance => _instance.Value;

        // Player & Auth
        internal UserRepository UserRepository { get; }
        internal OnlinePlayerRepository OnlinePlayerRepository { get; }

        // Social
        internal FriendRepository FriendRepository { get; }

        // Teams & Battling
        internal TeamRepository TeamRepository { get; }
        internal TeamMemberRepository TeamMemberRepository { get; }
        internal BattleRepository BattleRepository { get; }
        internal ParticipantRepository ParticipantRepository { get; }

        // Pokemon & Moves
        internal PokemonRepository PokemonRepository { get; }
        internal BattlerPokemonRepository BattlerPokemonRepository { get; }
        internal MoveRepository MoveRepository { get; }
        internal PokemonStatsRepository pokemonStatsRepository { get; }

        // Static Lookups (Cached)
        internal AbilityRepository AbilityRepository { get; }
        internal ItemRepository ItemRepository { get; }
        internal BreedingRepository BreedingRepository { get; }
        internal PokedexEntryRepository PokedexEntryRepository { get; }
        internal MoveLearnsetRepository MoveLearnsetRepository { get; }

        private ServiceFactory()
        {
            var db = new SQLiteConnectionService(
                "C:\\Users\\yoav\\Source\\Repos\\PokemonGame\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db");

            // Initialize all repositories with the shared connection service
            UserRepository = new UserRepository(db);
            OnlinePlayerRepository = new OnlinePlayerRepository(db);
            FriendRepository = new FriendRepository(db);

            TeamRepository = new TeamRepository(db);
            TeamMemberRepository = new TeamMemberRepository(db);
            BattleRepository = new BattleRepository(db);
            ParticipantRepository = new ParticipantRepository(db);

            PokemonRepository = new PokemonRepository(db);
            BattlerPokemonRepository = new BattlerPokemonRepository(db);
            MoveRepository = new MoveRepository(db);
            pokemonStatsRepository = new PokemonStatsRepository(db);

            AbilityRepository = new AbilityRepository(db);
            ItemRepository = new ItemRepository(db);
            BreedingRepository = new BreedingRepository(db);
            PokedexEntryRepository = new PokedexEntryRepository(db);
            MoveLearnsetRepository = new MoveLearnsetRepository(db);
        }
    }
}