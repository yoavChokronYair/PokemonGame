// Server/Hubs/MatchmakingHub.cs

using Microsoft.AspNetCore.SignalR;
using PokemonGame.Server.Controllers;
using PokemonGame.Server.Services;

namespace PokemonGame.Server.Hubs
{
    public class MatchmakingHub : Hub
    {
        private readonly IServerMatchmakingService _matchmaking;
        private readonly IMatchRegistry _matchRegistry;

        private static readonly Dictionary<int, string> _connections = new();
        private static readonly object _connLock = new();

        public MatchmakingHub(
            IServerMatchmakingService matchmaking,
            IMatchRegistry matchRegistry)
        {
            _matchmaking = matchmaking;
            _matchRegistry = matchRegistry;
        }

        public async Task FindMatch(MatchmakingEntry entry)
        {
            if (entry == null)
                throw new HubException("Matchmaking entry cannot be null.");

            if (entry.PlayerId <= 0)
                throw new HubException("Invalid player id.");

            entry.SelectedPokemonIds ??= new List<int>();

            lock (_connLock)
            {
                _connections[entry.PlayerId] = Context.ConnectionId;
            }

            string? sessionId = _matchmaking.TryMatch(entry, out var opponent);

            if (sessionId is not null && opponent is not null)
            {
                opponent.SelectedPokemonIds ??= new List<int>();

                // This is the important server-side preservation point.
                // BattleHub must later read these entries from the registry.
                _matchRegistry.StoreMatch(sessionId, entry, opponent);

                var callerMatchData = new MatchFoundMessage
                {
                    SessionId = sessionId,
                    OpponentId = opponent.PlayerId,
                    OpponentName = opponent.PlayerName,
                    BattleMode = entry.BattleMode,
                    IsOneVOne = entry.IsOneVOne,

                    // The current player's selected Pokémon.
                    YourSelectedPokemonIds = entry.SelectedPokemonIds.ToList(),

                    // The opponent's selected Pokémon.
                    OpponentSelectedPokemonIds = opponent.SelectedPokemonIds.ToList()
                };

                await Clients.Caller.SendAsync("MatchFound", callerMatchData);

                string? opponentConnectionId;

                lock (_connLock)
                {
                    _connections.TryGetValue(opponent.PlayerId, out opponentConnectionId);
                }

                if (opponentConnectionId is not null)
                {
                    var opponentMatchData = new MatchFoundMessage
                    {
                        SessionId = sessionId,
                        OpponentId = entry.PlayerId,
                        OpponentName = entry.PlayerName,
                        BattleMode = entry.BattleMode,
                        IsOneVOne = entry.IsOneVOne,

                        // From the opponent client's perspective, this is their own selection.
                        YourSelectedPokemonIds = opponent.SelectedPokemonIds.ToList(),

                        // And this is the player who just triggered the match.
                        OpponentSelectedPokemonIds = entry.SelectedPokemonIds.ToList()
                    };

                    await Clients.Client(opponentConnectionId)
                        .SendAsync("MatchFound", opponentMatchData);
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
            {
                _connections.Remove(playerId);
            }

            await Clients.Caller.SendAsync("SearchCancelled");
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            int? disconnectedPlayerId = null;

            lock (_connLock)
            {
                foreach (var pair in _connections)
                {
                    if (pair.Value == Context.ConnectionId)
                    {
                        disconnectedPlayerId = pair.Key;
                        break;
                    }
                }

                if (disconnectedPlayerId.HasValue)
                {
                    _connections.Remove(disconnectedPlayerId.Value);
                }
            }

            if (disconnectedPlayerId.HasValue)
            {
                _matchmaking.Dequeue(disconnectedPlayerId.Value);
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

        public List<int> YourSelectedPokemonIds { get; init; } = new();
        public List<int> OpponentSelectedPokemonIds { get; init; } = new();
    }
}