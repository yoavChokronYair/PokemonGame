using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonGame.Model.Data;
using PokemonGame.Model.Data.Items;
using PokemonGame.Model.Data.NpcData;
using System;
using System.Collections.Generic;
using System.IO;

namespace PokemonGame.Model.Manager
{
   
    public class GameDataManager
    {
        public MapDataList MapData { get; private set; } // Public property to access MapData
        public PokemonDataList PokemonData { get; private set; } // Public property to access PokemonData
        public RouteDataList RouteData { get; private set; } // Public property to access RouteData
        public MoveDataList MoveData { get; private set; } // Public property to access MoveData
        public CaughtPokemonDataList CaughtPokemonData { get; set; }//public property to access CaughtPokemonData
        public PlayerDataList PlayerData { get; set; }
        public RivalDataList RivalData {  get; set; }
        public PokeBallDataList PokeballData { get; set; }
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
        public void SaveAllData()
        {
            SaveJson(PokemonData, "CaughtPokemons.json");
        }
        public void LoadAllData()
        {
            MapData = LoadJson<MapDataList>("Maps/Maps.json");
            PokemonData = LoadJson<PokemonDataList>("Pokemons.json");
            RouteData = LoadJson<RouteDataList>("Maps/Routes.json");
            MoveData = LoadJson<MoveDataList>("Moves.json");
            CaughtPokemonData = LoadJson<CaughtPokemonDataList>("Player/CaughtPokemons.json");
            PlayerData = LoadJson<PlayerDataList>("Player/Players.json");
            RivalData = LoadJson<RivalDataList>("Npc/Rivals.json");
            PokeballData = LoadJson<PokeBallDataList>("Items/Pokeballs.json");
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
        private void SaveJson<T>(T data, string filePath)
        {
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string fullFilePath = Path.Combine(projectRoot, "Resources", filePath);

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(data, settings);
            File.WriteAllText(fullFilePath, json);
        }

    }


}
