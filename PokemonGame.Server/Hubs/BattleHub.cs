// Server/Hubs/BattleHub.cs

using Microsoft.AspNetCore.SignalR;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Server.Services;
using PokemonGame.Services.Factory;
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
        private readonly Dictionary<string, List<MatchmakingEntry>> _pendingMatches = new();
        private readonly object _lock = new();

        public void StoreMatch(string sessionId, MatchmakingEntry p1, MatchmakingEntry p2)
        {
            lock (_lock)
            {
                _pendingMatches[sessionId] = new List<MatchmakingEntry> { p1, p2 };
            }
        }

        public MatchmakingEntry? GetEntry(string sessionId, int playerId)
        {
            lock (_lock)
            {
                if (_pendingMatches.TryGetValue(sessionId, out var entries))
                    return entries.FirstOrDefault(e => e.PlayerId == playerId);

                return null;
            }
        }
    }

    public class BattleHub : Hub
    {
        private readonly IMatchRegistry _matchRegistry;
        private readonly IBattleSessionRegistry _sessionRegistry;
        private readonly ServiceFactory _serviceFactory;

        public BattleHub(
            IMatchRegistry matchRegistry,
            IBattleSessionRegistry sessionRegistry,
            ServiceFactory serviceFactory)
        {
            _matchRegistry = matchRegistry;
            _sessionRegistry = sessionRegistry;
            _serviceFactory = serviceFactory;
        }

        public async Task JoinSession(string sessionId, int playerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

            MatchmakingEntry? entry = _matchRegistry.GetEntry(sessionId, playerId);

            if (entry == null)
            {
                await Clients.Caller.SendAsync(
                    "Error",
                    "Session not found or you are not a participant.");

                return;
            }

            ServerBattleSession session = _sessionRegistry.GetOrCreate(
                sessionId,
                () => new ServerBattleSession(sessionId, _serviceFactory));

            session.RegisterPlayer(playerId, Context.ConnectionId, entry);
            _sessionRegistry.Touch(sessionId);

            if (session.BothPlayersReady && session.Manager != null)
            {
                await PushStateAsync(sessionId, session);
            }
        }

        public async Task SendAction(BattleActionMessage msg)
        {
            if (!_sessionRegistry.TryGet(msg.SessionId, out var session))
                return;

            if (session.IsOver)
                return;

            session.RecordAction(msg.PlayerId, msg.ActionType, msg.Index);
            _sessionRegistry.Touch(msg.SessionId);

            if (!session.BothActionsReady)
            {
                await Clients.Caller.SendAsync("WaitingForOpponent");
                return;
            }

            session.RunPvPTurn();
            session.ClearActions();

            await PushStateAsync(msg.SessionId, session);

            if (session.IsOver)
                _sessionRegistry.Remove(msg.SessionId);
        }

        public async Task Forfeit(string sessionId, int playerId)
        {
            if (!_sessionRegistry.TryGet(sessionId, out var session))
                return;

            session.Manager.ForceWinner(
                session.IsPlayer1(playerId)
                    ? session.Manager.BotTeam
                    : session.Manager.PlayerTeam);

            await PushStateAsync(sessionId, session);

            _sessionRegistry.Remove(sessionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_sessionRegistry.TryFindByConnection(
                    Context.ConnectionId,
                    out var session,
                    out int playerId) &&
                session is not null)
            {
                session.Manager.ForceWinner(
                    session.IsPlayer1(playerId)
                        ? session.Manager.BotTeam
                        : session.Manager.PlayerTeam);

                await PushStateAsync(session.SessionId, session);

                _sessionRegistry.Remove(session.SessionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task PushStateAsync(string sessionId, ServerBattleSession session)
        {
            if (!string.IsNullOrWhiteSpace(session.P1ConnectionId))
            {
                await Clients.Client(session.P1ConnectionId)
                    .SendAsync("StateUpdated", session.BuildSnapshot(session.Player1Id));
            }

            if (!string.IsNullOrWhiteSpace(session.P2ConnectionId))
            {
                await Clients.Client(session.P2ConnectionId)
                    .SendAsync("StateUpdated", session.BuildSnapshot(session.Player2Id));
            }
        }
    }

    public class BattleActionMessage
    {
        public string SessionId { get; set; } = string.Empty;
        public int PlayerId { get; set; }
        public BattleActionType ActionType { get; set; } = BattleActionType.Move;
        public int Index { get; set; }
    }
}