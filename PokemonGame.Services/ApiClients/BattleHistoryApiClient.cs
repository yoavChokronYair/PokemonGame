using System.Text.Json;
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.ApiClients
{
    public interface IBattleHistoryApiClient
    {
        List<BattleTreeData>? GetBattleHistory(int battlePlayerId, string username);
        int? CreateBattle();
    }
    public class BattleHistoryApiClient : IBattleHistoryApiClient
    {
        private readonly HttpClient _http;

        public BattleHistoryApiClient(string serverBaseUrl)
        {
            _http = new HttpClient { BaseAddress = new Uri(serverBaseUrl) };
        }

        public List<BattleTreeData>? GetBattleHistory(int battlePlayerId, string username)
        {
            var response = _http.GetAsync($"api/battlehistory/{battlePlayerId}?username={username}").Result;
            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<List<BattleTreeData>>(json);
        }

        public int? CreateBattle()
        {
            var response = _http.PostAsync("api/battlehistory/battle", null).Result;
            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<int>(json);
        }
    }
}
