
using Microsoft.AspNetCore.SignalR.Client;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Services
{
    public class OnlineMatchmakingService : IMatchmakingService
    {
        private readonly HubConnection _connection;

        public event Action<MatchFoundData>? OnMatchFound;
        public event Action? OnQueued;
        public event Action? OnCancelled;

        public OnlineMatchmakingService(string serverBaseUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"{serverBaseUrl}/hubs/matchmaking")
                .WithAutomaticReconnect()
                .Build();

            // FIX #4: MatchFoundData must have the same property names as
            // MatchFoundMessage sent by the hub (SessionId, OpponentId,
            // OpponentName, BattleMode, IsOneVOne).  See MatchFoundData below.
            _connection.On<MatchFoundData>("MatchFound", data =>
                OnMatchFound?.Invoke(data));

            _connection.On("Queued", () =>
                OnQueued?.Invoke());

            _connection.On("SearchCancelled", () =>
                OnCancelled?.Invoke());
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                    await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OnlineMatchmakingService] ConnectAsync failed: {ex.Message}");
                throw;
            }
        }

        public async Task FindMatchAsync(MatchmakingRequest request)
        {
            await ConnectAsync();

            try
            {
                // Pass the whole request; the hub receives it as MatchmakingEntry
                await _connection.InvokeAsync("FindMatch", new
                {
                    request.PlayerId,
                    request.PlayerName,
                    request.BattleMode,
                    request.IsOneVOne,
                    request.TeamId,
                    request.SelectedPokemonIds
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OnlineMatchmakingService] FindMatchAsync failed: {ex.Message}");
                throw;
            }
        }

        public async Task CancelAsync(int playerId)
        {
            try
            {
                await _connection.InvokeAsync("CancelSearch", playerId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OnlineMatchmakingService] CancelAsync failed: {ex.Message}");
                throw;
            }
        }

        public void Disconnect()
        {
            _ = _connection.DisposeAsync();
        }
    }

   
}
