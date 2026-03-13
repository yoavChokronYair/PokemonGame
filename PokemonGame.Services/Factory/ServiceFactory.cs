using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Repositories.SQLite;

namespace PokemonGame.Services.Factory
{
    public sealed class ServiceFactory
    {
        private static readonly Lazy<ServiceFactory> _instance = new(() => new ServiceFactory());
        public static ServiceFactory Instance => _instance.Value;

        internal SQLiteUserRepository UserRepository { get; }
        internal SQLitePokemonRepository PokemonRepository { get; }
        internal SQLiteOnlinePlayerRepository OnlinePlayerRepository { get; }
        internal SQLiteBattleRepository BattleRepository { get; }
        internal SQLiteMoveRepository MoveRepository { get; }

        private ServiceFactory()
        {
            var db = new SQLiteConnectionService(
                "C:\\Users\\yoav\\Source\\Repos\\PokemonGame\\PokemonGame.Services\\resources\\DB\\PokemonGameDB.db");

            UserRepository = new SQLiteUserRepository(db);
            PokemonRepository = new SQLitePokemonRepository(db);
            OnlinePlayerRepository = new SQLiteOnlinePlayerRepository(db);
            BattleRepository = new SQLiteBattleRepository(db);
            MoveRepository = new SQLiteMoveRepository(db);
        }
    }
}