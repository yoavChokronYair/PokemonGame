using System.Collections.Generic;

namespace PokemonGame.Services
{
    public interface ISQLiteConnectionService
    {
        /// <summary>
        /// Executes a SQL query that returns a single row and maps it to an object of type <typeparamref name="T"/>.
        /// </summary>
        T QuerySingle<T>(string sql, object? parameters = null);

        /// <summary>
        /// Executes a SQL query that returns multiple rows and maps them to a list of type <typeparamref name="T"/>.
        /// </summary>
        List<T> Query<T>(string sql, object? parameters = null);

        /// <summary>
        /// Executes a non-query SQL command (INSERT, UPDATE, DELETE).
        /// </summary>
        int Execute(string sql, object? parameters = null);
    }
}
