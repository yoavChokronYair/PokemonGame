using System.Text;
using System.Text.Json;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.ApiClients
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public UserData? UserData { get; set; }
    }
    public interface IUserApiClient
    {
        LoginResult? Login(string username, int hashedPassword);
        bool CreateUser(string username, int hashedPassword);
        bool? UserExists(string username);
        UserData? GetUser(string username);
    }
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _http;

        public UserApiClient(string serverBaseUrl)
        {
            _http = new HttpClient { BaseAddress = new Uri(serverBaseUrl) };
        }

        public LoginResult? Login(string username, int hashedPassword)
        {
            var body = JsonSerializer.Serialize(new { Username = username, HashedPassword = hashedPassword });
            var response = _http.PostAsync("api/user/login",
                new StringContent(body, Encoding.UTF8, "application/json")).Result;

            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<LoginResult>(json);
        }

        public bool CreateUser(string username, int hashedPassword)
        {
            var body = JsonSerializer.Serialize(new { Username = username, HashedPassword = hashedPassword });
            var response = _http.PostAsync("api/user/create",
                new StringContent(body, Encoding.UTF8, "application/json")).Result;

            return response.IsSuccessStatusCode;
        }

        public bool? UserExists(string username)
        {
            var response = _http.GetAsync($"api/user/exists/{username}").Result;

            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<bool>(json);
        }

        public UserData? GetUser(string username)
        {
            var response = _http.GetAsync($"api/user/{username}").Result;

            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<UserData>(json);
        }
    }
}
