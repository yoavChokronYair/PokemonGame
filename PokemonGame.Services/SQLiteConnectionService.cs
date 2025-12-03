using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PokemonGame.Services
{ 
    public class SQLiteConnectionService: ISQLiteConnectionService
    {
        private readonly string _connectionString;

        public SQLiteConnectionService(string dbPath)
        {
            _connectionString = $"data source={dbPath}";
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

            if (!reader.Read())
                return default!;

            var result = new T();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);

                var prop = typeof(T).GetProperty(
                    columnName,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase
                );

                if (prop == null || reader.IsDBNull(i))
                    continue;

                var value = reader.GetValue(i);

                // Get the actual type (handles nullable types)
                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;           
                var safeValue = Convert.ChangeType(value, targetType);
                prop.SetValue(result, safeValue);
                
            }

            return result;
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
        public int Execute(string sql, object parameters = null)
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

            return cmd.ExecuteNonQuery(); // returns number of rows affected
        }

    }

}
