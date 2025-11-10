using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace PokemonGame.Services
{
    public class SQLiteConnectionService : ISQLiteConnectionService
    {
        private readonly string _connectionString;

        public SQLiteConnectionService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        public T QuerySingle<T>(string sql, object? parameters = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);

            AddParameters(cmd, parameters);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapReaderToObject<T>(reader);
            }

            return default!;
        }

        public List<T> Query<T>(string sql, object? parameters = null)
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

        public int Execute(string sql, object? parameters = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);

            AddParameters(cmd, parameters);

            return cmd.ExecuteNonQuery();
        }

        #region --- Helper Methods ---

        private void AddParameters(SqliteCommand cmd, object? parameters)
        {
            if (parameters == null)
                return;

            foreach (var prop in parameters.GetType().GetProperties())
            {
                var value = prop.GetValue(parameters) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@" + prop.Name, value);
            }
        }

        private T MapReaderToObject<T>(SqliteDataReader reader)
        {
            // Use Activator.CreateInstance instead of new()
            var obj = Activator.CreateInstance<T>()!;

            foreach (var prop in typeof(T).GetProperties())
            {
                if (!reader.IsDBNull(reader.GetOrdinal(prop.Name)))
                    prop.SetValue(obj, reader[prop.Name]);
            }

            return obj;
        }

        #endregion
    }
}
