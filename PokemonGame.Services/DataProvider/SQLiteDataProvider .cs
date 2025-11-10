using PokemonGame.Services;
using PokemonGame.Services.Data;
using PokemonGame.Services.Data.Move;
using PokemonGame.Services.Data.Pokemon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Services.DataProvider
{
    public sealed class SQLiteDataProvider : GameDataProvider
    {
        private readonly ISQLiteConnectionService db;

        public SQLiteDataProvider(ISQLiteConnectionService dbService)
        {
            db = dbService;

            // --- Register all tables + columns here ---
            RegisterTable<int, PokemonData>("Pokemon", "PokemonID");
            RegisterTable<int, PokemonFormData>("PokemonForm", "PokemonID");
            RegisterTable<int, BaseStatsdata>("BaseStats", "PokemonID");
            RegisterTable<int, EvolutionData>("Evolution", "PokemonID");
            RegisterTable<int, EggMoveData>("EggMove", "PokemonID");
            RegisterTable<int, LevelUpMoveData>("LevelUpMove", "PokemonID");
            RegisterTable<string, MoveData>("Move", "MoveName");
            RegisterTable<int, AbilityData>("Ability", "AbilityID");
            RegisterTable<string, AbilityData>("Ability", "AbilityName");
            RegisterTable<string, AbilityData>("Ability", "AbilityDescription");
        }

        /// <summary>
        /// Registers a table with a key column. Builds both single-item and GetAll loaders.
        /// </summary>
        private void RegisterTable<TKey, TValue>(string table, string keyColumn)
        {
            // Register single-record loader
            base.Register<TKey, TValue>(keyColumn, key =>
                db.QuerySingle<TValue>(
                    $"SELECT * FROM {table} WHERE {keyColumn} = @value",
                    new { value = key }
                )
            );

            // Register "GetAll" loader (only once per type)
            base.RegisterAllLoader<TValue>(() =>
                db.Query<TValue>($"SELECT * FROM {table}").ToList()
            );
        }

    }
}
