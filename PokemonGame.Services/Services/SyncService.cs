using System.Text.Json;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class SyncService
    {
        private readonly HttpClient _http;
        private readonly SyncQueueRepository _queue;
        private readonly ServiceFactory _factory;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public SyncService(string serverBaseUrl, ServiceFactory factory)
        {
            _http = new HttpClient { BaseAddress = new Uri(serverBaseUrl) };
            _factory = factory;
            _queue = new SyncQueueRepository(factory.GetConnectionString());
        }

        // ── Called on app start — flush any queued failed syncs ───────────────

        public async Task RetryPendingAsync()
        {
            var pending = _queue.GetPending();
            Console.WriteLine($"[SYNC] RetryPending — {pending.Count} queued items");
            foreach (var item in pending)
            {
                Console.WriteLine($"[SYNC] Retrying {item.Endpoint} (id={item.Id})");
                try
                {
                    var content = new StringContent(item.JsonBody, System.Text.Encoding.UTF8, "application/json");
                    var response = await _http.PostAsync(item.Endpoint, content);
                    Console.WriteLine($"[SYNC] Retry {item.Endpoint} → {(int)response.StatusCode} {response.StatusCode}");
                    if (response.IsSuccessStatusCode)
                        _queue.Delete(item.Id);
                    else
                        _queue.IncrementRetry(item.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SYNC] Retry {item.Endpoint} FAILED: {ex.Message}");
                    _queue.IncrementRetry(item.Id);
                }
            }
        }

        // ── Full player sync — called on login when online ────────────────────

        public async Task SyncPlayerToServerAsync(int battlePlayerId)
        {
            Console.WriteLine($"[SYNC] SyncPlayer start — playerId={battlePlayerId}");
            var payload = BuildPlayerPayload(battlePlayerId);
            Console.WriteLine($"[SYNC] SyncPlayer payload — {payload.Teams.Count} teams");
            await TryPushAsync("/sync/player", payload);
            Console.WriteLine($"[SYNC] SyncPlayer done");
        }

        // ── Team sync — called after save/delete ──────────────────────────────

        public void PushTeamSync(int battlePlayerId)
        {
            var payload = BuildPlayerPayload(battlePlayerId);
            _ = TryPushAsync("/sync/player", payload);
        }

        // ── Battle result sync ────────────────────────────────────────────────

        public void PushBattleResult(int battlePlayerId)
        {
            var payload = BuildPlayerPayload(battlePlayerId);
            _ = TryPushAsync("/sync/player", payload);
        }

        // ── Auth ──────────────────────────────────────────────────────────────

        public async Task<bool> ValidateLoginAsync(string username, string password)
        {
            Console.WriteLine($"[SYNC] ValidateLogin — user={username}");
            try
            {
                string json = JsonSerializer.Serialize(new { username, password }, _json);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("/auth/login", content);
                Console.WriteLine($"[SYNC] ValidateLogin → {(int)response.StatusCode} {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SYNC] ValidateLogin ERROR: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PushSignUpAsync(string username, string hashedPassword)
        {
            Console.WriteLine($"[SYNC] PushSignUp — user={username}");
            try
            {
                string json = JsonSerializer.Serialize(new { username, password = hashedPassword }, _json);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("/auth/signup", content);
                Console.WriteLine($"[SYNC] PushSignUp → {(int)response.StatusCode} {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SYNC] PushSignUp ERROR: {ex.Message}");
                return false;
            }
        }

        // ── Build full player payload ─────────────────────────────────────────

        private PlayerSyncPayload BuildPlayerPayload(int battlePlayerId)
        {
            var teamService = _factory.CreateTeamBuilderService();
            var historyService = _factory.CreateBattleHistoryService();

            var teams = teamService.GetTeamsByBattlePlayer(battlePlayerId);
            var teamEntries = teams.Select(t => new SyncTeamEntry
            {
                TeamName = t.TeamName,
                BattlePlayerId = battlePlayerId,
                Members = teamService.GetTeamMembers(t.Id)
            }).ToList();

            return new PlayerSyncPayload
            {
                BattlePlayerId = battlePlayerId,
                Teams = teamEntries
            };
        }

        // ── Fire and forget with queue fallback ───────────────────────────────

        private async Task TryPushAsync(string endpoint, object payload)
        {
            string json = JsonSerializer.Serialize(payload, _json);
            Console.WriteLine($"[SYNC] Pushing {endpoint} — {json.Length} chars");
            try
            {
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync(endpoint, content);
                Console.WriteLine($"[SYNC] Push {endpoint} → {(int)response.StatusCode} {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SYNC] Push failed — queuing for retry");
                    _queue.Enqueue(endpoint, json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SYNC] Push {endpoint} EXCEPTION: {ex.Message} — queuing");
                _queue.Enqueue(endpoint, json);
            }
        }
        public async Task<int> PushBattleResultAsync()
        {
            Console.WriteLine($"[SYNC] PushBattleResult — posting to /battle/result");
            try
            {
                var response = await _http.PostAsync("/battle/result",
                    new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
                Console.WriteLine($"[SYNC] PushBattleResult → {(int)response.StatusCode} {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    int battleId = doc.RootElement.GetProperty("battleId").GetInt32();
                    Console.WriteLine($"[SYNC] PushBattleResult — got battleId={battleId}");
                    return battleId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SYNC] PushBattleResult ERROR: {ex.Message}");
            }
            return 0;
        }

        public async Task PushParticipantAsync(BattleParticipantData participant)
        {
            Console.WriteLine($"[SYNC] PushParticipant — battleId={participant.BattleID} playerId={participant.BattlePlayerID} isWinner={participant.IsWinner}");
            try
            {
                string json = JsonSerializer.Serialize(participant, _json);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("/battle/participant", content);
                Console.WriteLine($"[SYNC] PushParticipant → {(int)response.StatusCode} {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SYNC] PushParticipant ERROR: {ex.Message}");
            }
        }
    }

    // ── Payload DTOs ──────────────────────────────────────────────────────────

    public class PlayerSyncPayload
    {
        public int BattlePlayerId { get; set; }
        public List<SyncTeamEntry> Teams { get; set; } = new();
    }

    public class SyncTeamEntry
    {
        public string TeamName { get; set; } = string.Empty;
        public int BattlePlayerId { get; set; }
        public List<BattlerPokemon> Members { get; set; } = new();
    }
}