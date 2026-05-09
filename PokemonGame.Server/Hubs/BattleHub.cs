// Server/Hubs/BattleHub.cs

using Microsoft.AspNetCore.SignalR;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Server.Services;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Services;

namespace PokemonGame.Server.Hubs
{
    public interface IMatchRegistry
    {
        void StoreMatch(string sessionId, MatchmakingEntry p1, MatchmakingEntry p2);
        MatchmakingEntry? GetEntry(string sessionId, int playerId);
    }

    public class MatchRegistry : IMatchRegistry
    {
        // Key: SessionId, Value: List of entries in that match
        private readonly Dictionary<string, List<MatchmakingEntry>> _pendingMatches = new();

        public void StoreMatch(string sessionId, MatchmakingEntry p1, MatchmakingEntry p2)
        {
            _pendingMatches[sessionId] = new List<MatchmakingEntry> { p1, p2 };
        }

        public MatchmakingEntry? GetEntry(string sessionId, int playerId)
        {
            if (_pendingMatches.TryGetValue(sessionId, out var entries))
            {
                return entries.FirstOrDefault(e => e.PlayerId == playerId);
            }
            return null;
        }
    }

    public class BattleHub : Hub
    {
        private static readonly Dictionary<string, ServerBattleSession> _sessions = new();
        private static readonly object _lock = new();
        private readonly IMatchRegistry _matchRegistry;
        public BattleHub(IMatchRegistry matchRegistry)
        {
            _matchRegistry = matchRegistry;
        }

        public async Task JoinSession(string sessionId, int playerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

            // FIX: Retrieve the data from the server-side registry, not the client-service
            var entry = _matchRegistry.GetEntry(sessionId, playerId);

            if (entry == null)
            {
                // Handle error: Player tried to join a session they aren't part of
                return;
            }

            lock (_lock)
            {
                if (!_sessions.TryGetValue(sessionId, out var session))
                {
                    session = new ServerBattleSession(sessionId);
                    _sessions[sessionId] = session;
                }

                session.RegisterPlayer(playerId, Context.ConnectionId, entry);
            }

            ServerBattleSession? ready;
            lock (_lock)
                ready = _sessions.TryGetValue(sessionId, out var s) && s.BothPlayersReady ? s : null;

            if (ready is not null)
                await PushStateAsync(sessionId, ready);
        }

        public async Task SendAction(BattleActionMessage msg)
        {
            ServerBattleSession? session;
            lock (_lock) _sessions.TryGetValue(msg.SessionId, out session);
            if (session is null || session.IsOver) return;

            session.RecordAction(msg.PlayerId, msg.ActionType, msg.Index);
            if (!session.BothActionsReady) return;

            lock (_lock)
            {
                var (_, playerIdx) = session.GetAction(session.Player1Id);
                session.Manager.RunTurn(playerIdx);
                session.ClearActions();
            }

            await PushStateAsync(msg.SessionId, session);

            if (session.IsOver)
                lock (_lock) _sessions.Remove(msg.SessionId);
        }

        public async Task Forfeit(string sessionId, int playerId)
        {
            ServerBattleSession? session;
            lock (_lock) _sessions.TryGetValue(sessionId, out session);
            if (session is null) return;

            lock (_lock)
                session.Manager.ForceWinner(
                    session.IsPlayer1(playerId)
                        ? session.Manager.BotTeam
                        : session.Manager.PlayerTeam);

            await PushStateAsync(sessionId, session);
            lock (_lock) _sessions.Remove(sessionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_lock)
            {
                var session = _sessions.Values
                    .FirstOrDefault(s => s.HasConnection(Context.ConnectionId));

                if (session is not null)
                {
                    var pid = session.GetPlayerByConnection(Context.ConnectionId);
                    session.Manager.ForceWinner(
                        session.IsPlayer1(pid)
                            ? session.Manager.BotTeam
                            : session.Manager.PlayerTeam);

                    _ = PushStateAsync(session.SessionId, session);
                    _sessions.Remove(session.SessionId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        private async Task PushStateAsync(string sessionId, ServerBattleSession session) =>
            await Clients.Group(sessionId).SendAsync("StateUpdated", session.BuildSnapshot());
    }
}