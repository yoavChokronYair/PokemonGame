using System.Data;
using System.Reflection;

namespace PokemonGame.Services.Data.ConnectionsService
{
    /// <summary>
    /// Abstract base class for all database connection services.
    /// Provides shared row-mapping and type-conversion logic so concrete
    /// implementations only need to handle connection and parameter mechanics.
    /// </summary>
    /// <remarks>
    /// Implements the Strategy pattern base: subclasses are interchangeable
    /// behind <see cref="IDbConnectionService"/>. Uses Reflection internally
    /// so that <see cref="MapReaderToObject{T}"/> works for any model class
    /// without any per-type configuration.
    /// </remarks>
    public abstract class BaseDbConnectionService : IDbConnectionService
    {
        public abstract string ConnectionString { get; }
        public abstract int ExecuteAndGetLastId(string sql, object parameters = null);

        /// <inheritdoc/>
        public abstract T QuerySingle<T>(string sql, object parameters = null) where T : new();
        public abstract T QueryScalar<T>(string sql, object parameters = null);
        public abstract List<T> QueryScalarList<T>(string sql, object parameters = null);

        /// <inheritdoc/>
        public abstract List<T> Query<T>(string sql) where T : new();

        /// <inheritdoc/>
        public abstract List<T> Query<T>(string sql, object parameters) where T : new();

        /// <inheritdoc/>
        public abstract int Execute(string sql, object parameters = null);

        /// <summary>
        /// Maps the current row of an <see cref="IDataReader"/> to a new instance of <typeparamref name="T"/>.
        /// Property names are matched to column names case-insensitively via Reflection.
        /// Properties with no matching column, or whose column value is <see cref="DBNull"/>, are left at their default.
        /// </summary>
        /// <typeparam name="T">
        /// The target type. Must have a public parameterless constructor.
        /// Property names must match database column names (or SQL aliases).
        /// </typeparam>
        /// <param name="reader">
        /// An open reader positioned on the row to map.
        /// The reader is not advanced or closed by this method.
        /// </param>
        /// <returns>A new <typeparamref name="T"/> instance populated from the current row.</returns>
        protected static T MapReaderToObject<T>(IDataReader reader) where T : new()
        {
            var result = new T();

            var columnMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnMap[reader.GetName(i)] = reader.GetName(i);
            }

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!columnMap.TryGetValue(prop.Name, out var actualColumnName))
                {
                    continue;
                }

                var value = reader[actualColumnName];
                if (value == DBNull.Value)
                {
                    continue;
                }

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                prop.SetValue(result, ConvertValue(value, targetType));
            }

            return result;
        }

        /// <summary>
        /// Converts a raw value returned by a database driver to the requested CLR type.
        /// </summary>
        /// <remarks>
        /// Database drivers do not return every numeric type directly.
        /// SQLite returns <see langword="long"/> for all integers and
        /// <see langword="double"/> for all real numbers. OleDb returns
        /// <see langword="int"/> for Access Integer fields and
        /// <see langword="double"/> for Currency/Number fields.
        /// <see cref="Convert.ChangeType"/> fails for unsigned types and
        /// <see langword="float"/>, so every numeric type is handled explicitly
        /// via an intermediate <see langword="long"/> or <see langword="double"/> cast.
        /// </remarks>
        /// <param name="value">The raw value from the reader. Must not be <see cref="DBNull"/>.</param>
        /// <param name="targetType">The unwrapped target CLR type (nullable types must already be unwrapped).</param>
        /// <returns>The value converted to <paramref name="targetType"/>.</returns>
        protected static object ConvertValue(object value, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return value.ToString();
            }

            if (targetType == typeof(bool))
            {
                return Convert.ToInt64(value) != 0;
            }

            if (targetType == typeof(byte))
            {
                return (byte)Convert.ToInt64(value);
            }

            if (targetType == typeof(sbyte))
            {
                return (sbyte)Convert.ToInt64(value);
            }

            if (targetType == typeof(short))
            {
                return (short)Convert.ToInt64(value);
            }

            if (targetType == typeof(ushort))
            {
                return (ushort)Convert.ToInt64(value);
            }

            if (targetType == typeof(int))
            {
                return (int)Convert.ToInt64(value);
            }

            if (targetType == typeof(uint))
            {
                return (uint)Convert.ToInt64(value);
            }

            if (targetType == typeof(long))
            {
                return Convert.ToInt64(value);
            }

            if (targetType == typeof(ulong))
            {
                return (ulong)Convert.ToInt64(value);
            }

            if (targetType == typeof(float))
            {
                return (float)Convert.ToDouble(value);
            }

            if (targetType == typeof(double))
            {
                return Convert.ToDouble(value);
            }

            if (targetType == typeof(decimal))
            {
                return (decimal)Convert.ToDouble(value);
            }

            if (targetType == typeof(DateTime))
            {
                return DateTime.Parse(value.ToString()!);
            }

            if (targetType == typeof(Guid))
            {
                return Guid.Parse(value.ToString()!);
            }

            return Convert.ChangeType(value, targetType);
        }
    }
}