using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories;

namespace PokemonGame.Services.Handler
{
    public class SyncService
    {
        private readonly string _serverBaseUrl;
        private readonly SyncQueueRepository _queue;
        private readonly HttpClient _http;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SyncService(string serverBaseUrl, string localDbConnectionString)
        {
            _serverBaseUrl = serverBaseUrl.TrimEnd('/');
            _queue = new SyncQueueRepository(localDbConnectionString);
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        }

        public async Task PushSignUpAsync(UserData user)
            => await PostAsync("/auth/signup", user).ConfigureAwait(false);

        public async Task<bool> ValidateLoginAsync(string username, string password)
        {
            try
            {
                var payload = new { username, password };
                string json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync($"{_serverBaseUrl}/auth/login", content).ConfigureAwait(false);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task PushTeamSavedAsync(TeamData team, List<BattlerPokemon> members)
            => await PostAsync("/teams", new { team, members }).ConfigureAwait(false);

        public async Task PushTeamDeletedAsync(int teamId)
        {
            try
            {
                var resp = await _http.DeleteAsync($"{_serverBaseUrl}/teams/{teamId}").ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new HttpRequestException($"DELETE /teams/{teamId} → {resp.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SyncService] DELETE /teams/{teamId} failed — queuing. {ex.Message}");
                _queue.Enqueue($"/teams/{teamId}:DELETE", string.Empty);
            }
        }

        public async Task<int> PushBattleResultAsync()
        {
            try
            {
                var resp = await _http.PostAsync($"{_serverBaseUrl}/battle/result", null).ConfigureAwait(false);
                if (resp.IsSuccessStatusCode)
                {
                    string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var doc = JsonDocument.Parse(body);
                    return doc.RootElement.GetProperty("battleId").GetInt32();
                }
                throw new HttpRequestException($"POST /battle/result → {resp.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SyncService] /battle/result failed — queuing. {ex.Message}");
                _queue.Enqueue("/battle/result", string.Empty);
                return -1;
            }
        }

        public async Task PushParticipantAsync(BattleParticipantData participant)
            => await PostAsync("/battle/participant", participant).ConfigureAwait(false);

        public async Task RetryPendingAsync()
        {
            var pending = _queue.GetPending();
            foreach (var item in pending)
            {
                try
                {
                    if (item.Endpoint.EndsWith(":DELETE"))
                    {
                        string path = item.Endpoint.Replace(":DELETE", "");
                        var resp = await _http.DeleteAsync($"{_serverBaseUrl}{path}").ConfigureAwait(false);
                        if (resp.IsSuccessStatusCode) _queue.Delete(item.Id);
                        else _queue.IncrementRetry(item.Id);
                    }
                    else
                    {
                        var content = new StringContent(item.JsonBody, Encoding.UTF8, "application/json");
                        var resp = await _http.PostAsync($"{_serverBaseUrl}{item.Endpoint}", content).ConfigureAwait(false);
                        if (resp.IsSuccessStatusCode) _queue.Delete(item.Id);
                        else _queue.IncrementRetry(item.Id);
                    }
                }
                catch { _queue.IncrementRetry(item.Id); }
            }
        }

        private async Task PostAsync(string endpoint, object payload)
        {
            string json = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var resp = await _http.PostAsync($"{_serverBaseUrl}{endpoint}", content).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new HttpRequestException($"POST {endpoint} → {resp.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SyncService] {endpoint} failed — queuing. {ex.Message}");
                _queue.Enqueue(endpoint, json);
            }
        }
    }
}