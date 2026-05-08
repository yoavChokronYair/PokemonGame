
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

            _connection.On<BattleSnapshot>("StateUpdated", snapshot =>
            {
                _lastSnapshot = snapshot;
                OnStateUpdated?.Invoke();
            });

            _ = ConnectAsync();
        }

        // ── string action — no BattleAction reference ─────────────────────
        public void RunTurn(int index, string action = "Move")
        {
            if (IsOver) return;
            _ = _connection.InvokeAsync("SendAction", new BattleActionMessage
            {
                SessionId = _sessionId,
                PlayerId = _playerId,
                ActionType = action,   // already "Move" or "Switch" — pass through directly
                Index = index
            });
        }

        public void Forfeit()
        {
            _ = _connection.InvokeAsync("Forfeit", _sessionId, _playerId);
        }

        public BattleSnapshot GetState() => _lastSnapshot;

        public void Disconnect() => _ = _connection.DisposeAsync();

        private async Task ConnectAsync()
        {
            if (_connection.State == HubConnectionState.Disconnected)
                await _connection.StartAsync();
            await _connection.InvokeAsync("JoinSession", _sessionId, _playerId);
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