// PokemonGame.Services/Data/Repositories/SyncQueueRepository.cs
// Persists outbound HTTP sync operations locally so they can be
// retried if the server is unavailable when the event fires.

using Microsoft.Data.Sqlite;

namespace PokemonGame.Services.Data.Repositories
{
    public class SyncQueueItem
    {
        public int Id { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string JsonBody { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class SyncQueueRepository
    {
        private readonly string _connectionString;

        public SyncQueueRepository(string connectionString)
        {
            _connectionString = connectionString;
            EnsureTable();
        }

        // ── DDL ───────────────────────────────────────────────────────────────

        private void EnsureTable()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS SyncQueue (
                    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    Endpoint   TEXT    NOT NULL,
                    JsonBody   TEXT    NOT NULL,
                    RetryCount INTEGER NOT NULL DEFAULT 0,
                    CreatedAt  TEXT    NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public void Enqueue(string endpoint, string jsonBody)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO SyncQueue (Endpoint, JsonBody, RetryCount, CreatedAt)
                VALUES (@ep, @body, 0, @now);";
            cmd.Parameters.AddWithValue("@ep", endpoint);
            cmd.Parameters.AddWithValue("@body", jsonBody);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public void IncrementRetry(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE SyncQueue SET RetryCount = RetryCount + 1 WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM SyncQueue WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ── Read ──────────────────────────────────────────────────────────────

        /// <summary>Returns all items that have not yet exceeded the retry limit.</summary>
        public List<SyncQueueItem> GetPending(int maxRetries = 5)
        {
            var result = new List<SyncQueueItem>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Endpoint, JsonBody, RetryCount, CreatedAt FROM SyncQueue WHERE RetryCount < @max ORDER BY Id;";
            cmd.Parameters.AddWithValue("@max", maxRetries);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(new SyncQueueItem
                {
                    Id = reader.GetInt32(0),
                    Endpoint = reader.GetString(1),
                    JsonBody = reader.GetString(2),
                    RetryCount = reader.GetInt32(3),
                    CreatedAt = reader.GetString(4),
                });
            return result;
        }
    }
}