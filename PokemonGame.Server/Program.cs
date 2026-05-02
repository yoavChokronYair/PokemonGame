using PokemonGame.Server.BattleRoom;
using PokemonGame.Server.Controllers;
using PokemonGame.Server.Network;
using PokemonGame.Services.Factory;

var builder = WebApplication.CreateBuilder(args);

string dbPath = Path.Combine(AppContext.BaseDirectory, "resources", "ServerDB.db");
using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
conn.Open();
using var cmd = conn.CreateCommand();
cmd.CommandText = "PRAGMA foreign_keys = ON;";
cmd.ExecuteNonQuery();
// One factory for the server, registered as singleton
var factory = new ServiceFactory(dbPath);
TeamBuilder.Initialize(factory);
builder.Services.AddSingleton(factory);

// Services are scoped and created from the factory
builder.Services.AddScoped(sp => sp.GetRequiredService<ServiceFactory>().CreateSignUpService());
builder.Services.AddScoped(sp => sp.GetRequiredService<ServiceFactory>().CreateLogInService());
builder.Services.AddScoped(sp => sp.GetRequiredService<ServiceFactory>().CreateTeamBuilderService());
builder.Services.AddScoped(sp => sp.GetRequiredService<ServiceFactory>().CreateBattleHistoryService());

var app = builder.Build();

ApiEndpoints.Map(app);

int tcpPort = int.Parse(app.Configuration["TcpPort"] ?? "5001");
var tcpServer = new TcpBattleServer(tcpPort);
var cts = new CancellationTokenSource();
app.Lifetime.ApplicationStopping.Register(() => cts.Cancel());
_ = Task.Run(() => tcpServer.StartAsync(cts.Token));

await app.RunAsync();