// Server/Controllers/MatchmakingController.cs

using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Packets;

namespace PokemonGame.Server.Controllers
{
    [ApiController]
    [Route("api/matchmaking")]
    public class MatchmakingController : ControllerBase
    {

        // ── In-memory queue — replace with Redis when you scale ───────────────
        private static readonly Queue<MatchmakingEntry> _waitingQueue = new();
        private static readonly object _lock = new();

        public MatchmakingController()
        {
        }

        [HttpPost("find")]
        public IActionResult FindMatch([FromBody] FindMatchPacket packet)
        {
            lock (_lock)
            {
                // Check if there's already someone waiting
                if (_waitingQueue.TryDequeue(out var opponent))
                {
                    // Pair found — create a session ID and return to both
                    var sessionId = Guid.NewGuid().ToString();

                    return Ok(new MatchFoundResponse
                    {
                        SessionId = sessionId,
                        OpponentId = opponent.PlayerId,
                        OpponentName = opponent.PlayerName
                    });
                }

                // No one waiting — add to queue
                _waitingQueue.Enqueue(new MatchmakingEntry
                {
                    PlayerId = packet.PlayerId,
                    PlayerName = packet.PlayerName,
                    Packet = packet
                });

                return Accepted(new { Status = "Waiting" });
            }
        }

        [HttpDelete("cancel/{playerId}")]
        public IActionResult CancelSearch(int playerId)
        {
            lock (_lock)
            {
                var remaining = _waitingQueue
                    .Where(e => e.PlayerId != playerId)
                    .ToList();

                _waitingQueue.Clear();
                foreach (var e in remaining)
                    _waitingQueue.Enqueue(e);
            }
            return Ok();
        }
    }

    public class MatchmakingEntry
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public FindMatchPacket Packet { get; set; } = new();
    }

    public class MatchFoundResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public int OpponentId { get; set; }
        public string OpponentName { get; set; } = string.Empty;
    }
}