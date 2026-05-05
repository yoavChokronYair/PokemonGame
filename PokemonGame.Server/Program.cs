using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Factory;

namespace PokemonGame.Server
{
    internal class Program
    {
        public static void Main(string[] args) 
        {
        
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // Register ServiceFactory with the server DB path from config
            var dbPath = builder.Configuration["Database:Path"]
                ?? throw new InvalidOperationException("Database:Path is not configured.");

            builder.Services.AddSingleton(new ServiceFactory(new SQLiteConnectionService(dbPath)));

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }
}
