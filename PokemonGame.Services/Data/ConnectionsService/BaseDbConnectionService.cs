using System.Data;
using System.Reflection;

namespace PokemonGame.Services.Data.ConnectionsService
{
    public abstract class BaseDbConnectionService : IDbConnectionService
    {
        public abstract T QuerySingle<T>(string sql, object parameters = null) where T : new();
        public abstract List<T> Query<T>(string sql) where T : new();
        public abstract List<T> Query<T>(string sql, object parameters) where T : new();
        public abstract int Execute(string sql, object parameters = null);

        protected static T MapReaderToObject<T>(IDataReader reader) where T : new()
        {
            var result = new T();

            var columnMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
                columnMap[reader.GetName(i)] = reader.GetName(i);

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!columnMap.TryGetValue(prop.Name, out var actualColumnName)) continue;

                var value = reader[actualColumnName];
                if (value == DBNull.Value) continue;

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                prop.SetValue(result, ConvertValue(value, targetType));
            }

            return result;
        }

        protected static object ConvertValue(object value, Type targetType)
        {
            if (targetType == typeof(string)) return value.ToString();
            if (targetType == typeof(bool)) return Convert.ToInt64(value) != 0;
            if (targetType == typeof(byte)) return (byte)Convert.ToInt64(value);
            if (targetType == typeof(sbyte)) return (sbyte)Convert.ToInt64(value);
            if (targetType == typeof(short)) return (short)Convert.ToInt64(value);
            if (targetType == typeof(ushort)) return (ushort)Convert.ToInt64(value);
            if (targetType == typeof(int)) return (int)Convert.ToInt64(value);
            if (targetType == typeof(uint)) return (uint)Convert.ToInt64(value);
            if (targetType == typeof(long)) return Convert.ToInt64(value);
            if (targetType == typeof(ulong)) return (ulong)Convert.ToInt64(value);
            if (targetType == typeof(float)) return (float)Convert.ToDouble(value);
            if (targetType == typeof(double)) return Convert.ToDouble(value);
            if (targetType == typeof(decimal)) return (decimal)Convert.ToDouble(value);
            if (targetType == typeof(DateTime)) return DateTime.Parse(value.ToString()!);
            if (targetType == typeof(Guid)) return Guid.Parse(value.ToString()!);
            return Convert.ChangeType(value, targetType);
        }
    }
}