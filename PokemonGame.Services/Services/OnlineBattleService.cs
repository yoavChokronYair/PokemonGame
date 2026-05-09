
using Microsoft.AspNetCore.SignalR.Client;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Services
{
    public class OnlineBattleService : IBattleService
    {
        private readonly HubConnection _connection;
        private readonly string _sessionId;
        private readonly int _playerId;

        private BattleSnapshot _lastSnapshot = new();

        public event Action? OnStateUpdated;

        public bool IsOver => _lastSnapshot.IsOver;
        public string? WinnerName => _lastSnapshot.WinnerName;

        public OnlineBattleService(string sessionId, int playerId, string serverBaseUrl)
        {
            _sessionId = sessionId;
            _playerId = playerId;

            _connection = new HubConnectionBuilder()
                .WithUrl($"{serverBaseUrl}/hubs/battle")
                .WithAutomaticReconnect()
                .Build();

            // Register handlers BEFORE connecting so no events are missed
            _connection.On<BattleSnapshot>("StateUpdated", snapshot =>
            {
                _lastSnapshot = snapshot;
                OnStateUpdated?.Invoke();
            });

            // FIX #6: surface exceptions instead of silently swallowing them
            _ = ConnectAsync();
        }

        // FIX #7-related: action string maps "Move"/"Switch" straight through
        public void RunTurn(int index, string action = "Move")
        {
            if (IsOver) return;
            _ = SendActionAsync(index, action);
        }

        private async Task SendActionAsync(int index, string action)
        {
            try
            {
                await _connection.InvokeAsync("SendAction", new BattleActionMessage
                {
                    SessionId = _sessionId,
                    PlayerId = _playerId,
                    ActionType = action,
                    Index = index
                });
            }
            catch (Exception ex)
            {
                // Propagate to UI layer or log — never swallow silently
                System.Diagnostics.Debug.WriteLine($"[OnlineBattleService] SendAction failed: {ex.Message}");
                throw;
            }
        }

        public void Forfeit()
        {
            _ = ForfeitAsync();
        }

        private async Task ForfeitAsync()
        {
            try
            {
                await _connection.InvokeAsync("Forfeit", _sessionId, _playerId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OnlineBattleService] Forfeit failed: {ex.Message}");
                throw;
            }
        }

        public BattleSnapshot GetState() => _lastSnapshot;

        public void Disconnect() => _ = _connection.DisposeAsync();

        private async Task ConnectAsync()
        {
            // FIX #6: wrap in try/catch so callers/logs see the failure
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                    await _connection.StartAsync();

                await _connection.InvokeAsync("JoinSession", _sessionId, _playerId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OnlineBattleService] ConnectAsync failed: {ex.Message}");
                // Optionally raise an event here so the UI can show a connection-error dialog
                throw;
            }
        }
    }

    public class BattleActionMessage
    {
        public string SessionId { get; set; } = string.Empty;
        public int PlayerId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public int Index { get; set; }
    }
}
