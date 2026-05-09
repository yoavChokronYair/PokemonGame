// Server/Services/MatchmakingService.cs

using PokemonGame.Server.Hubs;

namespace PokemonGame.Server.Services
{
    public class MatchmakingEntry
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string BattleMode { get; set; } = string.Empty;
        public bool IsOneVOne { get; set; }
        public int TeamId { get; set; }
        public List<int> SelectedPokemonIds { get; set; } = new();
    }

    public interface IServerMatchmakingService
    {
        /// <summary>
        /// Attempt to match <paramref name="entry"/> with a waiting player.
        /// Returns the new session ID when a match is made, or null if queued.
        /// </summary>
        string? TryMatch(MatchmakingEntry entry, out MatchmakingEntry? opponent);

        void Dequeue(int playerId);
    }
    public class ServerMatchmakingService : IServerMatchmakingService
    {
        private readonly Queue<MatchmakingEntry> _queue = new();
        private readonly object _lock = new();

        public string? TryMatch(MatchmakingEntry entry, out MatchmakingEntry? opponent)
        {
            lock (_lock)
            {
                // Remove self if already in queue (reconnect / double-click guard)
                var existing = _queue.FirstOrDefault(e => e.PlayerId == entry.PlayerId);
                if (existing is not null)
                {
                    var temp = _queue.ToList();
                    temp.Remove(existing);
                    _queue.Clear();
                    foreach (var e in temp) _queue.Enqueue(e);
                }

                if (_queue.Count > 0)
                {
                    opponent = _queue.Dequeue();
                    var sessionId = Guid.NewGuid().ToString("N");
                    return sessionId;
                }

                _queue.Enqueue(entry);
                opponent = null;
                return null;
            }
        }

        public void Dequeue(int playerId)
        {
            lock (_lock)
            {
                var list = _queue.ToList();
                list.RemoveAll(e => e.PlayerId == playerId);
                _queue.Clear();
                foreach (var e in list) _queue.Enqueue(e);
            }
        }
    }
}
