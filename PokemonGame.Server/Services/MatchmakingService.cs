// Server/Services/MatchmakingService.cs

using PokemonGame.Server.Hubs;

namespace PokemonGame.Server.Services
{
    public class MatchmakingEntry
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string BattleMode { get; set; } = string.Empty;
        public bool IsOneVOne { get; set; }
        public int TeamId { get; set; }
        public List<int> SelectedPokemonIds { get; set; } = new();
    }

    public interface IMatchmakingService
    {
        // ── Returns sessionId if a match was found, null if queued ───────────
        string? TryMatch(MatchmakingEntry entry, out MatchmakingEntry? opponent);
        void Dequeue(int playerId);
        bool IsQueued(int playerId);
    }

    public class MatchmakingService : IMatchmakingService
    {
        // ── Swap this list for an ELO-sorted structure later ─────────────────
        private readonly List<MatchmakingEntry> _queue = new();
        private readonly object _lock = new();

        public string? TryMatch(MatchmakingEntry entry, out MatchmakingEntry? opponent)
        {
            lock (_lock)
            {
                // ── Find first eligible opponent ──────────────────────────────
                // Right now: just first in queue
                // Later: replace this predicate with ELO range check
                opponent = _queue.FirstOrDefault(e =>
                    e.PlayerId != entry.PlayerId &&
                    e.BattleMode == entry.BattleMode &&
                    e.IsOneVOne == entry.IsOneVOne);

                if (opponent is not null)
                {
                    _queue.Remove(opponent);
                    var sessionId = Guid.NewGuid().ToString();
                    return sessionId;
                }

                // ── No match — add to queue ───────────────────────────────────
                if (!_queue.Any(e => e.PlayerId == entry.PlayerId))
                    _queue.Add(entry);

                return null;
            }
        }

        public void Dequeue(int playerId)
        {
            lock (_lock)
                _queue.RemoveAll(e => e.PlayerId == playerId);
        }

        public bool IsQueued(int playerId)
        {
            lock (_lock)
                return _queue.Any(e => e.PlayerId == playerId);
        }
    }
}