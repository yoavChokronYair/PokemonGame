using PokemonGame.Server.Hubs;
using PokemonGame.Server.Services;
using PokemonGame.Services.Factory;

namespace PokemonGame.Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://localhost:5000");

            var serviceFactory = ServiceFactory.CreateLocal("DB\\PokemonGameDB.db");
            builder.Services.AddSingleton(serviceFactory);
            builder.Services.AddSingleton(serviceFactory.UserService);
            builder.Services.AddSingleton(serviceFactory.ProfileService);
            builder.Services.AddSingleton(serviceFactory.GameModeService);
            builder.Services.AddSingleton(serviceFactory.TeamService);
            builder.Services.AddSingleton(serviceFactory.BattleHistoryService);

            builder.Services.AddSingleton<IServerMatchmakingService, ServerMatchmakingService>();
            builder.Services.AddSingleton<IMatchRegistry, MatchRegistry>();
            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.WithOrigins("http://localhost:5000", "http://localhost")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });

            var app = builder.Build();

            app.UseRouting();  // 1st
            app.UseCors();     // 2nd - must be after UseRouting
            app.MapControllers();
            app.MapHub<MatchmakingHub>("/hubs/matchmaking");
            app.MapHub<BattleHub>("/hubs/battle");

            // Temporary: confirm routes are registered
            foreach (var ep in app.Services
                .GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>().Endpoints)
                Console.WriteLine($"[Route] {ep.DisplayName}");
            app.MapGet("/health", () => Results.Ok("healthy"));
            app.Run();
        }
    }
}