using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonGame.Model.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace PokemonGame.Core
{
    namespace Scripts.Core
    {
        public class GameDataManager
        {
            public MapDataList MapData { get; private set; } // Public property to access MapData
            public PokemonDataList PokemonData { get; private set; } // Public property to access PokemonData
            public RouteDataList RouteData { get; private set; } // Public property to access RouteData
            public MoveDataList MoveData { get; private set; } // Public property to access MoveData
            private GameDataManager() { } // Private constructor

            private static GameDataManager instance;
            public static GameDataManager Instance
            {
                get
                {
                    if (instance == null)
                        instance = new GameDataManager();
                    return instance;
                }
            }
            public void LoadAllData()
            {
                MapData = LoadJson<MapDataList>("Maps.json");
                PokemonData = LoadJson<PokemonDataList>("Pokemons.json");
                RouteData = LoadJson<RouteDataList>("Routes.json");
                MoveData = LoadJson<MoveDataList>("Moves.json");
            }

            private T LoadJson<T>(string filePath)
            {
                T list;
                string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string FullFIlePath = Path.Combine(projectRoot, "Resources", filePath);

                string json = File.ReadAllText(FullFIlePath);
                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new StringEnumConverter() }, // Converts JSON string keys to Enums
                    NullValueHandling = NullValueHandling.Ignore
                };
                list = JsonConvert.DeserializeObject<T>(json, settings);

                return list;
            }

        }
    }

}
