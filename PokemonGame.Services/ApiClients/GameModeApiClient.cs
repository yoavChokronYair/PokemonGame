using System.Text;
using System.Text.Json;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.ApiClients
{
    public interface IGameModeApiClient
    {
        BattlePlayerData? GetOnlinePlayer(string username, int userId);
        List<BattlePlayerData>? GetAllOnlinePlayers(int userId);
        bool CreateOnlinePlayer(string username, int userId);
        bool? PlayerExists(string username, int userId);
        BattlePlayerSettingsData? GetSettings(int battlePlayerId);
    }

    public class GameModeApiClient : IGameModeApiClient
    {
        private readonly HttpClient _http;

        public GameModeApiClient(string serverBaseUrl)
        {
            _http = new HttpClient { BaseAddress = new Uri(serverBaseUrl) };
        }

        public BattlePlayerData? GetOnlinePlayer(string username, int userId)
        {
            var response = _http.GetAsync($"api/gamemode/player/{username}/{userId}").Result;
            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<BattlePlayerData>(json);
        }

        public List<BattlePlayerData>? GetAllOnlinePlayers(int userId)
        {
            var response = _http.GetAsync($"api/gamemode/players/{userId}").Result;
            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<List<BattlePlayerData>>(json);
        }

        public bool CreateOnlinePlayer(string username, int userId)
        {
            var body = JsonSerializer.Serialize(new { Username = username, UserId = userId });
            var response = _http.PostAsync("api/gamemode/player",
                new StringContent(body, Encoding.UTF8, "application/json")).Result;

            return response.IsSuccessStatusCode;
        }

        public bool? PlayerExists(string username, int userId)
        {
            var response = _http.GetAsync($"api/gamemode/exists/{username}/{userId}").Result;
            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<bool>(json);
        }

        public BattlePlayerSettingsData? GetSettings(int battlePlayerId)
        {
            var response = _http.GetAsync($"api/gamemode/settings/{battlePlayerId}").Result;
            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<BattlePlayerSettingsData>(json);
        }
    }
}