
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonGameModel.Model.Data;
using PokemonGameModel.Model.Data.Items;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Data.NpcData;
using PokemonGameModel.Model.Data.Player;


namespace PokemonGameModel.Model.Manager
{
    public class GameDataManager
    {
        public MapDataList MapData { get; private set; } // Public property to access MapData
        public PokemonDataList PokemonData { get; private set; } // Public property to access PokemonData
        public MoveDataList MoveData { get; private set; } // Public property to access MoveData
        public CaughtPokemonDataList CaughtPokemonData { get; set; }//public property to access CaughtPokemonData
        public PlayerDataList PlayerData { get; set; }
        public Dictionary<RivalData,bool> RivalData {  get; set; }//is defeted or not 
        public PokeBallDataList PokeballData { get; set; }
        public TrainerDataList TrainerData { get; set; }
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
            MoveData = LoadJson<MoveDataList>("Moves.json");
            CaughtPokemonData = LoadJson<CaughtPokemonDataList>("Player/CaughtPokemons.json");
            PlayerData = LoadJson<PlayerDataList>("Player/Players.json");
            var rivalList = LoadJson<RivalDataList>("Npc/Rivals.json");
            RivalData = rivalList.Rival.ToDictionary(rival => rival, rival => true);
            PokeballData = LoadJson<PokeBallDataList>("Items/Pokeballs.json");
            TrainerData = LoadJson<TrainerDataList>("Npc/Trainers.json");
        }

        private T LoadJson<T>(string filePath)
        {
            // This assumes "Resources" folder is in the output directory (e.g. bin/Debug/netstandard2.0/Resources)
            string fullFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", filePath);
            
            string json = File.ReadAllText(fullFilePath);

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };

            T list = JsonConvert.DeserializeObject<T>(json, settings);

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
