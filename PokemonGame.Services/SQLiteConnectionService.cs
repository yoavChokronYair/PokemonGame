using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services
{ 
    public class SQLiteConnectionService
    {
        private readonly string _connectionString;

        public SQLiteConnectionService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        public T QuerySingle<T>(string sql, object parameters = null) where T : new()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);

            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(parameters));
                }
            }

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var result = new T();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(prop.Name)))
                        prop.SetValue(result, reader[prop.Name]);
                }
                return result;
            }

            return default!;
        }

        public List<T> Query<T>(string sql) where T : new()
        {
            var list = new List<T>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var result = new T();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(prop.Name)))
                        prop.SetValue(result, reader[prop.Name]);
                }
                list.Add(result);
            }

            return list;
        }
    }

}
