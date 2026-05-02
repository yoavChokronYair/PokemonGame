// PokemonGame.Server/Controllers/ApiEndpoints.cs

using System.Text.Json;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Handler;

namespace PokemonGame.Server.Controllers
{
    public static class ApiEndpoints
    {
        // ── Registration helper ───────────────────────────────────────────────

        public static void Map(WebApplication app)
        {
            app.MapPost("/auth/signup", SignUpAsync);
            app.MapPost("/auth/login", LoginAsync);
            app.MapPost("/teams", UpsertTeamAsync);
            app.MapDelete("/teams/{id}", DeleteTeamAsync);
            app.MapPost("/battle/result", SaveBattleResultAsync);
            app.MapPost("/battle/participant", SaveParticipantAsync);
            app.MapPost("/sync/full", FullSyncAsync);

        }
        private static async Task<IResult> FullSyncAsync(HttpRequest req, TeamBuilderService teamService)
        {
            using var reader = new StreamReader(req.Body);
            string json = await reader.ReadToEndAsync();
            var payload = JsonSerializer.Deserialize<FullSyncPayload>(json,
                              new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload == null) return Results.BadRequest();

            foreach (var team in payload.Teams)
                teamService.SaveTeam(team.TeamName, team.BattlePlayerId, team.Members ?? new());

            return Results.Ok();
        }

        private record FullSyncPayload(List<SyncTeamEntry> Teams);

        private record SyncTeamEntry(
            string TeamName,
            int BattlePlayerId,
            List<BattlerPokemon>? Members);
        private record SyncUserEntry(string Username, string HashedPassword);
        // ── /auth/signup ──────────────────────────────────────────────────────

        private static async Task<IResult> SignUpAsync(HttpRequest req,
                                                        SignUpService signUpService)
        {
            using var reader = new StreamReader(req.Body);
            string json = await reader.ReadToEndAsync();

            var payload = JsonSerializer.Deserialize<SignUpPayload>(json,
                              new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload == null) return Results.BadRequest("Invalid payload");

            bool ok = signUpService.CreateUser(payload.Username, payload.Password);
            return ok ? Results.Ok() : Results.Conflict("Username already taken");
        }

        // ── /auth/login ───────────────────────────────────────────────────────

        private static async Task<IResult> LoginAsync(HttpRequest req,
                                                       LogInService logInService)
        {
            using var reader = new StreamReader(req.Body);
            string json = await reader.ReadToEndAsync();

            var payload = JsonSerializer.Deserialize<LoginPayload>(json,
                              new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload == null) return Results.BadRequest("Invalid payload");

            // Real method is Login(), not ValidateCredentials()
            bool valid = logInService.Login(payload.Username, payload.Password);
            return valid ? Results.Ok() : Results.Unauthorized();
        }


        // ── /teams ────────────────────────────────────────────────────────────

        private static async Task<IResult> UpsertTeamAsync(HttpRequest req,
                                                             TeamBuilderService teamService)
        {
            using var reader = new StreamReader(req.Body);
            string json = await reader.ReadToEndAsync();

            var payload = JsonSerializer.Deserialize<TeamUpsertPayload>(json,
                              new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (payload == null
                || string.IsNullOrWhiteSpace(payload.TeamName)
                || payload.BattlePlayerId <= 0)
                return Results.BadRequest("TeamName and BattlePlayerId are required.");

            // Real SaveTeam signature: (string teamName, int battlePlayerId, List<BattlerPokemon> slots)
            teamService.SaveTeam(payload.TeamName, payload.BattlePlayerId, payload.Members ?? new());
            return Results.Ok();
        }

        private static IResult DeleteTeamAsync(int id, TeamBuilderService teamService)
        {
            teamService.DeleteTeam(id);
            return Results.NoContent();
        }

        // ── /battle/result ────────────────────────────────────────────────────

        private static IResult SaveBattleResultAsync(BattleHistoryService historyService)
        {
            int battleId = historyService.SaveBattleRecord();
            return Results.Created($"/battle/{battleId}", new { battleId });
        }
        private static async Task<IResult> SaveParticipantAsync(HttpRequest req,
                                                         BattleHistoryService historyService)
        {
            using var reader = new StreamReader(req.Body);
            string json = await reader.ReadToEndAsync();
            var participant = JsonSerializer.Deserialize<BattleParticipantData>(json,
                                  new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (participant == null) return Results.BadRequest();
            historyService.SaveParticipant(participant);
            return Results.Ok();
        }

        // ── Payload DTOs ──────────────────────────────────────────────────────

        private record SignUpPayload(string Username, string Password);
        private record LoginPayload(string Username, string Password);

        // TeamData object replaced with flat fields matching SaveTeam()'s real signature
        private record TeamUpsertPayload(
            string? TeamName,
            int BattlePlayerId,
            List<BattlerPokemon>? Members);
    }
}