using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.DataCache;
using PokemonGame.Services.Data.DataProvider.SQLite;
using PokemonGame.Services.Data.Interfaces;

namespace PokemonGame.Services.Factory
{
    public sealed class ServiceFactory
    {
        private static readonly Lazy<ServiceFactory> _instance = new(() => new ServiceFactory());
        public static ServiceFactory Instance => _instance.Value;

        // --- Repositories (internal) ---
        internal ISQLiteConnectionService DbConnection { get; }
        internal IPokemonRepository PokemonRepository { get; }
        internal IUserRepository UserRepository { get; }
        internal IOnlinePlayerRepository OnlinePlayerRepository { get; }
        internal IBattleRepository BattleRepository { get; }
        internal IMoveRepository MoveRepository { get; }

        // --- Cache services (public) ---
        public PokemonCacheService PokemonCache { get; }
        public UserCacheService UserCache { get; }
        public OnlinePlayerCacheService OnlinePlayerCache { get; }
        public BattleCacheService BattleCache { get; }
        public MoveCacheService MoveCache { get; }

        private ServiceFactory()
        {
            // 1. create the DB connection (singleton)
            DbConnection = new SQLiteConnectionService("C:\\Users\\yoav\\Source\\Repos\\PokemonGame\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db");

            // 2. create repositories (internal)
            PokemonRepository = new SQLitePokemonRepository(DbConnection);
            UserRepository = new SQLiteUserRepository(DbConnection);
            OnlinePlayerRepository = new SQLiteOnlinePlayerRepository(DbConnection);
            BattleRepository = new SQLiteBattleRepository(DbConnection);
            MoveRepository = new SQLiteMoveRepository(DbConnection);

            // 3. create cache services (public)
            PokemonCache = new PokemonCacheService(PokemonRepository);
            UserCache = new UserCacheService(UserRepository);
            OnlinePlayerCache = new OnlinePlayerCacheService(OnlinePlayerRepository);
            BattleCache = new BattleCacheService(BattleRepository);
            MoveCache = new MoveCacheService(MoveRepository);
        }
    }
}
