using System.Text;
using System.Text.Json;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.ApiClients
{
    public interface IProfileApiClient
    {
        ProfileDataTree? GetFullProfile(int battlePlayerId);
        void UpdateSetting(int battlePlayerId, string columnName, int value);
        void SetFavoriteTeam(int battlePlayerId, int? teamId);
    }
    public class ProfileApiClient : IProfileApiClient
    {
        private readonly HttpClient _http;

        public ProfileApiClient(string serverBaseUrl)
        {   
            _http = new HttpClient { BaseAddress = new Uri(serverBaseUrl) };
        }

        public ProfileDataTree? GetFullProfile(int battlePlayerId)
        {
            var response = _http.GetAsync($"api/profile/{battlePlayerId}").Result;
            if (!response.IsSuccessStatusCode) return null;
            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<ProfileDataTree>(json);
        }

        public void UpdateSetting(int battlePlayerId, string columnName, int value)
        {
            var body = JsonSerializer.Serialize(new { ColumnName = columnName, Value = value });
            _http.PostAsync($"api/profile/{battlePlayerId}/setting",
                new StringContent(body, Encoding.UTF8, "application/json")).Wait();
        }

        public void SetFavoriteTeam(int battlePlayerId, int? teamId)
        {   
            var body = JsonSerializer.Serialize(new { TeamId = teamId });
            _http.PostAsync($"api/profile/{battlePlayerId}/favteam",
                new StringContent(body, Encoding.UTF8, "application/json")).Wait();
        }

        public void SyncToLocal(int battlePlayerId, ProfileDataTree dto)
        {
            // Write the server data into local DB via your existing repos
            // This runs on the client side — no server call needed
            var factory = ServiceFactory.Instance;
            // ... mirror dto fields into local repos
        }
    }
}
