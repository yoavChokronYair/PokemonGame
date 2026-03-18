using System.Data.OleDb;
    
namespace PokemonGame.Services.Data.ConnectionsService
{
    /// <summary>
    /// <see cref="IDbConnectionService"/> implementation for Microsoft Access databases (.accdb / .mdb)
    /// using the OLE DB provider.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires the 64-bit Microsoft Access Database Engine (ACE) redistributable.
    /// Windows-only — <see cref="System.Data.OleDb"/> is not supported on Linux or macOS.
    /// </para>
    /// <para>
    /// OLE DB does not support named parameters; all parameters must be positional <c>?</c>
    /// placeholders in the SQL command. <see cref="AddParameters"/> translates
    /// <c>@Name</c> tokens in the SQL string to <c>?</c> automatically, so repositories
    /// can use the same SQL syntax as the SQLite implementation.
    /// Parameter order in the anonymous object passed to a query must match
    /// the order the corresponding <c>@Name</c> tokens appear in the SQL string.
    /// </para>
    /// </remarks>
    public class OleDbConnectionService : BaseDbConnectionService
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initialises a new instance targeting an Access database file.
        /// </summary>
        /// <param name="dbPath">
        /// Full path to the .accdb file.
        /// For legacy .mdb files, replace the provider in the connection string
        /// with <c>Microsoft.Jet.OLEDB.4.0</c>.
        /// </param>
        public OleDbConnectionService(string dbPath)
            => _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};";

        /// <inheritdoc/>
        public override T QuerySingle<T>(string sql, object parameters = null)
        {
            using var conn = new OleDbConnection(_connectionString);
            conn.Open();
            using var cmd = new OleDbCommand(sql, conn);
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
            using var conn = new OleDbConnection(_connectionString);
            conn.Open();
            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, parameters);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapReaderToObject<T>(reader));
            return list;
        }

        /// <inheritdoc/>
        public override int Execute(string sql, object parameters = null)
        {
            using var conn = new OleDbConnection(_connectionString);
            conn.Open();
            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Binds properties of <paramref name="parameters"/> to <paramref name="cmd"/>
        /// as positional OLE DB parameters.
        /// </summary>
        /// <remarks>
        /// OLE DB ignores parameter names and matches by position only.
        /// This method first replaces each <c>@PropertyName</c> token in the SQL
        /// with <c>?</c> (using <see cref="ReplaceFirstOccurrence"/> to preserve order),
        /// then adds the corresponding values in the same order.
        /// Properties are enumerated in declaration order, which is preserved by
        /// the C# compiler for anonymous objects — so <c>new { A = 1, B = 2 }</c>
        /// is safe as long as the SQL reads <c>@A</c> before <c>@B</c>.
        /// </remarks>
        /// <param name="cmd">The command whose <c>CommandText</c> will be mutated and whose parameters will be populated.</param>
        /// <param name="parameters">An anonymous object whose properties map to SQL parameters. Pass <see langword="null"/> for parameter-free queries.</param>
        private static void AddParameters(OleDbCommand cmd, object parameters)
        {
            if (parameters == null) return;

            var props = parameters.GetType().GetProperties();

            foreach (var prop in props)
                cmd.CommandText = ReplaceFirstOccurrence(cmd.CommandText, "@" + prop.Name, "?");

            foreach (var prop in props)
                cmd.Parameters.AddWithValue("?", prop.GetValue(parameters) ?? DBNull.Value);
        }

        /// <summary>
        /// Replaces the first occurrence of <paramref name="find"/> in <paramref name="source"/>
        /// with <paramref name="replace"/>, using a case-insensitive search.
        /// Returns <paramref name="source"/> unchanged if <paramref name="find"/> is not found.
        /// </summary>
        private static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int pos = source.IndexOf(find, StringComparison.OrdinalIgnoreCase);
            return pos < 0 ? source : source.Substring(0, pos) + replace + source.Substring(pos + find.Length);
        }
    }
}
