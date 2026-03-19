using Microsoft.Data.Sqlite;

namespace PokemonGame.Services.Data.ConnectionsService
{
    /// <summary>
    /// <see cref="IDbConnectionService"/> implementation for SQLite databases
    /// using <see cref="Microsoft.Data.Sqlite"/>.
    /// </summary>
    /// <remarks>
    /// Cross-platform and file-based. Parameters use named <c>@Name</c> syntax,
    /// which is also the syntax expected by repositories — no translation needed.
    /// </remarks>
    public class SQLiteConnectionService : BaseDbConnectionService
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initialises a new instance targeting a SQLite database file.
        /// </summary>
        /// <param name="dbPath">Full path to the .db file.</param>
        public SQLiteConnectionService(string dbPath)
            => _connectionString = $"Data Source={dbPath}";

        /// <inheritdoc/>
        public override T QuerySingle<T>(string sql, object parameters = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            AddParameters(cmd, parameters);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToObject<T>(reader) : default!;
        }

        /// <inheritdoc/>
        public override List<T> Query<T>(string sql) => Query<T>(sql, null);

        /// <inheritdoc/>
        public override List<T> Query<T>(string sql, object parameters)
        {
            var list = new List<T>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            AddParameters(cmd, parameters);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapReaderToObject<T>(reader));
            return list;
        }

        /// <inheritdoc/>
        public override int Execute(string sql, object parameters = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Binds properties of <paramref name="parameters"/> to <paramref name="cmd"/>
        /// as named SQLite parameters.
        /// </summary>
        /// <remarks>
        /// Each property on the anonymous object becomes a <c>@PropertyName</c> parameter.
        /// <see langword="null"/> property values are bound as <see cref="DBNull.Value"/>.
        /// </remarks>
        /// <param name="cmd">The command to add parameters to.</param>
        /// <param name="parameters">An anonymous object whose properties map to SQL parameters. Pass <see langword="null"/> for parameter-free queries.</param>
        private static void AddParameters(SqliteCommand cmd, object parameters)
        {
            if (parameters == null) return;
            foreach (var prop in parameters.GetType().GetProperties())
                cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(parameters) ?? DBNull.Value);
        }
        public override int ExecuteAndGetLastId(string sql, object parameters = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            AddParameters(cmd, parameters);
            cmd.ExecuteNonQuery();

            using var idCmd = new SqliteCommand("SELECT last_insert_rowid();", conn);
            return (int)(long)idCmd.ExecuteScalar();
        }
    }
}