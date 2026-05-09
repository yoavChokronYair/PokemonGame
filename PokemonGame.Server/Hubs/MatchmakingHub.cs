// Server/Hubs/MatchmakingHub.cs

using Microsoft.AspNetCore.SignalR;
using PokemonGame.Server.Controllers;
using PokemonGame.Server.Services;

namespace PokemonGame.Server.Hubs
{
    public class MatchmakingHub : Hub
    {
        private readonly IServerMatchmakingService _matchmaking;
        private readonly IMatchRegistry _matchRegistry;   // FIX #5: inject registry

        // Maps playerId → connectionId so we can push to waiting players
        private static readonly Dictionary<int, string> _connections = new();
        private static readonly object _connLock = new();

        public MatchmakingHub(
            IServerMatchmakingService matchmaking,
            IMatchRegistry matchRegistry)
        {
            _matchmaking = matchmaking;
            _matchRegistry = matchRegistry;
        }

        // Called by client when they enter the queue
        public async Task FindMatch(MatchmakingEntry entry)
        {
            lock (_connLock)
                _connections[entry.PlayerId] = Context.ConnectionId;

            var sessionId = _matchmaking.TryMatch(entry, out var opponent);

            if (sessionId is not null && opponent is not null)
            {
                // FIX #5: Store match in registry BEFORE notifying clients so
                // BattleHub.JoinSession can retrieve the entries immediately.
                _matchRegistry.StoreMatch(sessionId, entry, opponent);

                var matchData = new MatchFoundMessage
                {
                    SessionId = sessionId,
                    BattleMode = entry.BattleMode,
                    IsOneVOne = entry.IsOneVOne
                };

                // Notify the player who just triggered the match
                await Clients.Caller.SendAsync("MatchFound", matchData with
                {
                    OpponentId = opponent.PlayerId,
                    OpponentName = opponent.PlayerName
                });

                // Notify the opponent who was already waiting
                string? opponentConnectionId;
                lock (_connLock)
                    _connections.TryGetValue(opponent.PlayerId, out opponentConnectionId);

                if (opponentConnectionId is not null)
                {
                    await Clients.Client(opponentConnectionId).SendAsync("MatchFound", matchData with
                    {
                        OpponentId = entry.PlayerId,
                        OpponentName = entry.PlayerName
                    });
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Queued");
            }
        }

        public async Task CancelSearch(int playerId)
        {
            _matchmaking.Dequeue(playerId);

            lock (_connLock)
                _connections.Remove(playerId);

            await Clients.Caller.SendAsync("SearchCancelled");
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_connLock)
            {
                var entry = _connections.FirstOrDefault(kv => kv.Value == Context.ConnectionId);
                if (entry.Value is not null)
                {
                    _matchmaking.Dequeue(entry.Key);
                    _connections.Remove(entry.Key);
                }
            }
            return base.OnDisconnectedAsync(exception);
        }
    }

    // FIX #4: same shape as client-side MatchFoundData
    public record MatchFoundMessage
    {
        public string SessionId { get; init; } = string.Empty;
        public int OpponentId { get; init; }
        public string OpponentName { get; init; } = string.Empty;
        public string BattleMode { get; init; } = string.Empty;
        public bool IsOneVOne { get; init; }
    }
}

   
