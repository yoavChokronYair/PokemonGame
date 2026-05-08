// Server/Hubs/MatchmakingHub.cs

using Microsoft.AspNetCore.SignalR;
using PokemonGame.Server.Controllers;
using PokemonGame.Server.Services;

namespace PokemonGame.Server.Hubs
{
    public class MatchmakingHub : Hub
    {
        private readonly IMatchmakingService _matchmaking;

        // ── Maps playerId → connectionId for pushing to specific players ─────
        private static readonly Dictionary<int, string> _connections = new();
        private static readonly object _connLock = new();

        public MatchmakingHub(IMatchmakingService matchmaking)
        {
            _matchmaking = matchmaking;
        }

        // ── Called by client when they enter the matchmaking queue ────────────
        public async Task FindMatch(MatchmakingEntry entry)
        {
            // Track this player's connection
            lock (_connLock)
                _connections[entry.PlayerId] = Context.ConnectionId;

            var sessionId = _matchmaking.TryMatch(entry, out var opponent);

            if (sessionId is not null && opponent is not null)
            {
                // ── Match found — notify both players ─────────────────────────
                var matchData = new MatchFoundMessage
                {
                    SessionId = sessionId,
                    BattleMode = entry.BattleMode,
                    IsOneVOne = entry.IsOneVOne
                };

                // Push to the player who just joined
                await Clients.Caller.SendAsync("MatchFound", matchData with
                {
                    OpponentId = opponent.PlayerId,
                    OpponentName = opponent.PlayerName
                });

                // Push to the opponent who was already waiting
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
                // ── Queued — tell client to show waiting screen ───────────────
                await Clients.Caller.SendAsync("Queued");
            }
        }

        // ── Called by client when they cancel the search ──────────────────────
        public async Task CancelSearch(int playerId)
        {
            _matchmaking.Dequeue(playerId);

            lock (_connLock)
                _connections.Remove(playerId);

            await Clients.Caller.SendAsync("SearchCancelled");
        }

        // ── Clean up if connection drops ──────────────────────────────────────
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

    public record MatchFoundMessage
    {
        public string SessionId { get; init; } = string.Empty;
        public int OpponentId { get; init; }
        public string OpponentName { get; init; } = string.Empty;
        public string BattleMode { get; init; } = string.Empty;
        public bool IsOneVOne { get; init; }
    }
}