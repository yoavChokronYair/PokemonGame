using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Repositories.SQLite;

namespace PokemonGame.Services.Factory
{
    public sealed class ServiceFactory
    {
        private static readonly Lazy<ServiceFactory> _instance = new(() => new ServiceFactory());
        public static ServiceFactory Instance => _instance.Value;

        // Player & Auth
        internal SQLiteUserRepository UserRepository { get; }
        internal SQLiteOnlinePlayerRepository OnlinePlayerRepository { get; }

        // Social
        internal SQLiteFriendRepository FriendRepository { get; }

        // Teams & Battling
        internal SQLiteTeamRepository TeamRepository { get; }
        internal SQLiteTeamMemberRepository TeamMemberRepository { get; }
        internal SQLiteBattleRepository BattleRepository { get; }
        internal SQLiteParticipantRepository ParticipantRepository { get; }

        // Pokemon & Moves
        internal SQLitePokemonRepository PokemonRepository { get; }
        internal SQLiteBattlerPokemonRepository BattlerPokemonRepository { get; }
        internal SQLiteMoveRepository MoveRepository { get; }

        // Static Lookups (Cached)
        internal SQLiteAbilityRepository AbilityRepository { get; }
        internal SQLiteItemRepository ItemRepository { get; }
        internal SQLiteBreedingRepository BreedingRepository { get; }
        internal SQLitePokedexEntryRepository PokedexEntryRepository { get; }
        internal SQLiteMoveLearnsetRepository MoveLearnsetRepository { get; }

        private ServiceFactory()
        {
            var db = new SQLiteConnectionService(
                "C:\\Users\\yoav\\Source\\Repos\\PokemonGame\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db");

            // Initialize all repositories with the shared connection service
            UserRepository = new SQLiteUserRepository(db);
            OnlinePlayerRepository = new SQLiteOnlinePlayerRepository(db);
            FriendRepository = new SQLiteFriendRepository(db);

            TeamRepository = new SQLiteTeamRepository(db);
            TeamMemberRepository = new SQLiteTeamMemberRepository(db);
            BattleRepository = new SQLiteBattleRepository(db);
            ParticipantRepository = new SQLiteParticipantRepository(db);

            PokemonRepository = new SQLitePokemonRepository(db);
            BattlerPokemonRepository = new SQLiteBattlerPokemonRepository(db);
            MoveRepository = new SQLiteMoveRepository(db);

            AbilityRepository = new SQLiteAbilityRepository(db);
            ItemRepository = new SQLiteItemRepository(db);
            BreedingRepository = new SQLiteBreedingRepository(db);
            PokedexEntryRepository = new SQLitePokedexEntryRepository(db);
            MoveLearnsetRepository = new SQLiteMoveLearnsetRepository(db);
        }
    }
}