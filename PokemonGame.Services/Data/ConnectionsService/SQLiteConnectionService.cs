using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PokemonGame.Services.Data.ConnectionsService
{
    public class SQLiteConnectionService : ISQLiteConnectionService
    {
        private readonly string _connectionString;

        public SQLiteConnectionService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        // --- Single row query ---
        public T QuerySingle<T>(string sql, object parameters = null) where T : new()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = new SqliteCommand(sql, conn);
            AddParameters(cmd, parameters);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return default!;

            return MapReaderToObject<T>(reader);
        }

        // --- Multiple row query (without parameters) ---
        public List<T> Query<T>(string sql) where T : new()
        {
            return Query<T>(sql, null);
        }

        // --- Multiple row query (with parameters) ---
        public List<T> Query<T>(string sql, object parameters) where T : new()
        {
            var list = new List<T>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = new SqliteCommand(sql, conn);
            AddParameters(cmd, parameters);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapReaderToObject<T>(reader));
            }

            return list;
        }

        // --- Execute non-query (INSERT, UPDATE, DELETE) ---
        public int Execute(string sql, object parameters = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = new SqliteCommand(sql, conn);
            AddParameters(cmd, parameters);

            return cmd.ExecuteNonQuery();
        }

        // --- Helper: add parameters to command ---
        private static void AddParameters(SqliteCommand cmd, object parameters)
        {
            if (parameters == null)
                return;

            foreach (var prop in parameters.GetType().GetProperties())
            {
                cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(parameters) ?? DBNull.Value);
            }
        }

        // --- Helper: map a SqliteDataReader row to an object ---
        private static T MapReaderToObject<T>(SqliteDataReader reader) where T : new()
        {
            var result = new T();

            var columnNames = new HashSet<string>(
                Enumerable.Range(0, reader.FieldCount).Select(reader.GetName),
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!columnNames.Contains(prop.Name))
                    continue;

                var value = reader[prop.Name];
                if (value == DBNull.Value)
                    continue;

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                prop.SetValue(result, ConvertValue(value, targetType));
            }

            return result;
        }

        // --- Helper: safely convert SQLite value to target CLR type ---
        // SQLite driver returns: long for all integers, double for reals,
        // string for text, byte[] for blobs.
        // Convert.ChangeType fails for byte/sbyte/ushort/uint/float, so we
        // handle every numeric type explicitly via an intermediate long/double.
        private static object ConvertValue(object value, Type targetType)
        {
            if (targetType == typeof(string))
                return value.ToString();

            if (targetType == typeof(bool))
                return Convert.ToInt64(value) != 0;

            if (targetType == typeof(byte))
                return (byte)Convert.ToInt64(value);

            if (targetType == typeof(sbyte))
                return (sbyte)Convert.ToInt64(value);

            if (targetType == typeof(short))
                return (short)Convert.ToInt64(value);

            if (targetType == typeof(ushort))
                return (ushort)Convert.ToInt64(value);

            if (targetType == typeof(int))
                return (int)Convert.ToInt64(value);

            if (targetType == typeof(uint))
                return (uint)Convert.ToInt64(value);

            if (targetType == typeof(long))
                return Convert.ToInt64(value);

            if (targetType == typeof(ulong))
                return (ulong)Convert.ToInt64(value);

            if (targetType == typeof(float))
                return (float)Convert.ToDouble(value);

            if (targetType == typeof(double))
                return Convert.ToDouble(value);

            if (targetType == typeof(decimal))
                return (decimal)Convert.ToDouble(value);

            if (targetType == typeof(DateTime))
                return DateTime.Parse(value.ToString()!);

            if (targetType == typeof(Guid))
                return Guid.Parse(value.ToString()!);

            // Fallback for any other type
            return Convert.ChangeType(value, targetType);
        }
    }
}