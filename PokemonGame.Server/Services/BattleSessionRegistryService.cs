using PokemonGame.Model.Model.Managers;
using PokemonGame.Server.Hubs;

namespace PokemonGame.Server.Services
{
    public interface IBattleSessionRegistry
    {
        ServerBattleSession GetOrCreate(
            string sessionId,
            Func<ServerBattleSession> factory);

        bool TryGet(string sessionId, out ServerBattleSession session);

        bool TryFindByConnection(
            string connectionId,
            out ServerBattleSession? session,
            out int playerId);

        void Remove(string sessionId);

        void Touch(string sessionId);

        IReadOnlyList<ServerBattleSession> GetTimedOutSessions(TimeSpan timeout);
    }

    public class InMemoryBattleSessionRegistryService : IBattleSessionRegistry
    {
        private sealed class SessionRecord
        {
            public ServerBattleSession Session { get; set; } = null!;
            public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;
        }

        private readonly Dictionary<string, SessionRecord> _sessions = new();
        private readonly object _lock = new();

        public ServerBattleSession GetOrCreate(
            string sessionId,
            Func<ServerBattleSession> factory)
        {
            lock (_lock)
            {
                if (!_sessions.TryGetValue(sessionId, out var record))
                {
                    record = new SessionRecord
                    {
                        Session = factory(),
                        LastActivityUtc = DateTime.UtcNow
                    };

                    _sessions[sessionId] = record;
                }

                record.LastActivityUtc = DateTime.UtcNow;
                return record.Session;
            }
        }

        public bool TryGet(string sessionId, out ServerBattleSession session)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(sessionId, out var record))
                {
                    record.LastActivityUtc = DateTime.UtcNow;
                    session = record.Session;
                    return true;
                }
            }

            session = null!;
            return false;
        }

        public bool TryFindByConnection(
            string connectionId,
            out ServerBattleSession? session,
            out int playerId)
        {
            lock (_lock)
            {
                foreach (var record in _sessions.Values)
                {
                    if (record.Session.HasConnection(connectionId))
                    {
                        session = record.Session;
                        playerId = record.Session.GetPlayerByConnection(connectionId);
                        record.LastActivityUtc = DateTime.UtcNow;
                        return true;
                    }
                }
            }

            session = null;
            playerId = 0;
            return false;
        }

        public void Remove(string sessionId)
        {
            lock (_lock)
            {
                _sessions.Remove(sessionId);
            }
        }

        public void Touch(string sessionId)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(sessionId, out var record))
                    record.LastActivityUtc = DateTime.UtcNow;
            }
        }

        public IReadOnlyList<ServerBattleSession> GetTimedOutSessions(TimeSpan timeout)
        {
            var now = DateTime.UtcNow;

            lock (_lock)
            {
                return _sessions.Values
                    .Where(r => now - r.LastActivityUtc > timeout)
                    .Select(r => r.Session)
                    .ToList();
            }
        }
    }
}