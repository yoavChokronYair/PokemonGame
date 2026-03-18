using Microsoft.Data.Sqlite;

namespace PokemonGame.Services.Data.ConnectionsService
{
    public class SQLiteConnectionService : BaseDbConnectionService
    {
        private readonly string _connectionString;

        public SQLiteConnectionService(string dbPath)
            => _connectionString = $"Data Source={dbPath}";

        public override T QuerySingle<T>(string sql, object parameters = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            AddParameters(cmd, parameters);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToObject<T>(reader) : default!;
        }

        public override List<T> Query<T>(string sql) => Query<T>(sql, null);

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

        public override int Execute(string sql, object parameters = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }

        private static void AddParameters(SqliteCommand cmd, object parameters)
        {
            if (parameters == null) return;
            foreach (var prop in parameters.GetType().GetProperties())
                cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(parameters) ?? DBNull.Value);
        }
    }
}