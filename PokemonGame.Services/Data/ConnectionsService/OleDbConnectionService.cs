using System.Data.OleDb;

namespace PokemonGame.Services.Data.ConnectionsService
{
    public class OleDbConnectionService : BaseDbConnectionService
    {
        private readonly string _connectionString;

        public OleDbConnectionService(string dbPath)
            => _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};";

        public override T QuerySingle<T>(string sql, object parameters = null)
        {
            using var conn = new OleDbConnection(_connectionString);
            conn.Open();
            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, parameters);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToObject<T>(reader) : default!;
        }

        public override List<T> Query<T>(string sql) => Query<T>(sql, null);

        public override List<T> Query<T>(string sql, object parameters)
        {
            var list = new List<T>();
            using var conn = new OleDbConnection(_connectionString);
            conn.Open();
            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, parameters);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapReaderToObject<T>(reader));
            return list;
        }

        public override int Execute(string sql, object parameters = null)
        {
            using var conn = new OleDbConnection(_connectionString);
            conn.Open();
            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }

        // Translates @Name → ? so repositories can use identical SQL strings
        private static void AddParameters(OleDbCommand cmd, object parameters)
        {
            if (parameters == null) return;

            var props = parameters.GetType().GetProperties();

            foreach (var prop in props)
                cmd.CommandText = ReplaceFirstOccurrence(cmd.CommandText, "@" + prop.Name, "?");

            foreach (var prop in props)
                cmd.Parameters.AddWithValue("?", prop.GetValue(parameters) ?? DBNull.Value);
        }

        private static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int pos = source.IndexOf(find, StringComparison.OrdinalIgnoreCase);
            return pos < 0 ? source : source.Substring(0, pos) + replace + source.Substring(pos + find.Length);
        }
    }
}