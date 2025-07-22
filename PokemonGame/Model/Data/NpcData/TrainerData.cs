using PokemonGame.Model.PokemonCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Model.Data.NpcData
{
    public class TrainerData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Route { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }

        public string Title { get; set; }
        public string Sprite { get; set; }
        public string BattleTheme { get; set; }
        public string MapLocation { get; set; }
        public Dialogue Dialogue { get; set; }
        public List<TrainerPokemonJson> Team { get; set; }
        public Reward Rewards { get; set; }
        public bool RematchAvailable { get; set; }
    }
    public class TrainerPokemonJson
    {
        public int PokedexID { get; set; }
        public string PokemonName { get; set; }
        public int Level { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public bool IsShiny { get; set; }
        public bool IsMale { get; set; }
        public string Nature { get; set; }
        public string Ability { get; set; }
        public string[] Types { get; set; }
        public StatValues IVs { get; set; }
        public StatValues EVs { get; set; }
        public List<string> Moves { get; set; }
        public string StatusCondition { get; set; }
        public string SpriteFileName { get; set; }
        public string ImageFileName { get; set; }
    }

    public class TrainerDataList
    {
        public List<TrainerData> trainers { get; set; }
    }
}
