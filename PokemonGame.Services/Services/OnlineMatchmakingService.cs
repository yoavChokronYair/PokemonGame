
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

            // ── Wire up server → client events ────────────────────────────────
            _connection.On<MatchFoundData>("MatchFound", data =>
                OnMatchFound?.Invoke(data));

            _connection.On("Queued", () =>
                OnQueued?.Invoke());

            _connection.On("SearchCancelled", () =>
                OnCancelled?.Invoke());
        }

        public async Task ConnectAsync()
        {
            if (_connection.State == HubConnectionState.Disconnected)
                await _connection.StartAsync();
        }

        public async Task FindMatchAsync(MatchmakingRequest request)
        {
            await ConnectAsync();
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

        public async Task CancelAsync(int playerId)
        {
            await _connection.InvokeAsync("CancelSearch", playerId);
        }

        public void Disconnect()
        {
            _ = _connection.DisposeAsync();
        }
    }
}
