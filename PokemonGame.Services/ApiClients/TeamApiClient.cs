using System.Text;
using System.Text.Json;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.ApiClients
{
    public interface ITeamApiClient
    {
        List<TeamData>? GetTeamsByBattlePlayer(int battlePlayerId);
        bool DeleteTeam(int teamId);
        TeamData? SaveTeam(string teamName, int battlePlayerId, List<BattlerPokemon> slots);
        bool UpdateTeam(int teamId, string teamName, List<BattlerPokemon> slots);
        bool ReplaceTeamSlot(int teamId, int slotNumber, BattlerPokemon pokemon);
        bool RemoveTeamSlot(int teamId, int pokemonId);
    }
    public class TeamApiClient : ITeamApiClient
    {
        private readonly HttpClient _http;

        public TeamApiClient(string serverBaseUrl)
        {
            _http = new HttpClient { BaseAddress = new Uri(serverBaseUrl) };
        }

        public List<TeamData>? GetTeamsByBattlePlayer(int battlePlayerId)
        {
          
            var response = _http.GetAsync($"api/team/{battlePlayerId}").GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return null;
            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<List<TeamData>>(json);
            
          
        }

        public bool DeleteTeam(int teamId)
        {
            var response = _http.DeleteAsync($"api/team/{teamId}").GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }

        public TeamData? SaveTeam(string teamName, int battlePlayerId, List<BattlerPokemon> slots)
        {
            var body = JsonSerializer.Serialize(new { TeamName = teamName, BattlePlayerId = battlePlayerId, Slots = slots });
            var response = _http.PostAsync("api/team",
                new StringContent(body, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode) return null;

            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<TeamData>(json);
        }

        public bool UpdateTeam(int teamId, string teamName, List<BattlerPokemon> slots)
        {
            var body = JsonSerializer.Serialize(new { TeamName = teamName, Slots = slots });
            var response = _http.PutAsync($"api/team/{teamId}",
                new StringContent(body, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }

        public bool ReplaceTeamSlot(int teamId, int slotNumber, BattlerPokemon pokemon)
        {
            var body = JsonSerializer.Serialize(new { SlotNumber = slotNumber, Pokemon = pokemon });
            var response = _http.PutAsync($"api/team/{teamId}/slot",
                new StringContent(body, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }

        public bool RemoveTeamSlot(int teamId, int pokemonId)
        {
            var response = _http.DeleteAsync($"api/team/{teamId}/slot/{pokemonId}").GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }
    }

}
