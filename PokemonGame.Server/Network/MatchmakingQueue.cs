// PokemonGame.Server/Network/MatchmakingQueue.cs
// Thread-safe queue that pairs two players with compatible settings.
// A match key is "{BattleMode}:{IsOneVOne}" — players must share the same key.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PokemonGame.Server.Network
{
    public class MatchmakingQueue
    {
        // key → waiting players with that preference
        private readonly ConcurrentDictionary<string, Queue<ConnectedPlayer>> _buckets = new();
        private readonly object _lock = new();

        /// <summary>
        /// Add a player to the queue.
        /// Returns the paired opponent if a match was found, or null if still waiting.
        /// </summary>
        public ConnectedPlayer? Enqueue(ConnectedPlayer player)
        {
            string key = MakeKey(player);

            lock (_lock)
            {
                if (!_buckets.TryGetValue(key, out var bucket))
                {
                    bucket = new Queue<ConnectedPlayer>();
                    _buckets[key] = bucket;
                }

                if (bucket.Count > 0)
                {
                    // Found a waiting opponent
                    return bucket.Dequeue();
                }

                // No opponent yet — wait
                bucket.Enqueue(player);
                return null;
            }
        }

        public void Remove(ConnectedPlayer player)
        {
            // Rebuild the bucket without this player.
            // Called when the TCP connection drops before a match is found.
            string key = MakeKey(player);
            lock (_lock)
            {
                if (!_buckets.TryGetValue(key, out var bucket)) return;
                var updated = new Queue<ConnectedPlayer>();
                foreach (var p in bucket)
                    if (p != player) updated.Enqueue(p);
                _buckets[key] = updated;
            }
        }

        private static string MakeKey(ConnectedPlayer p)
            => $"{p.BattleMode}:{p.IsOneVOne}";
    }
}