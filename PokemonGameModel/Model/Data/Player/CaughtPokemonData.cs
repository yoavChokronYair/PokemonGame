using PokemonGame.Enums;
using PokemonGame.Model.PokemonCreation;
using System;
using System.Collections.Generic;

namespace PokemonGame.Model.Data.Player
{
    public class CaughtPokemonData
    {
        public string SpriteFileName { get; set; }  // e.g. "pikachu.png"
        public string ImageFileName { get; set; }   // e.g. "pikachu_large.png"
        public int PokemonID { get; set; }
        public int pokedexID { get; set; }
        public string pokemonName { get; set; }
        public string Nickname { get; set; }
        public int Level { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public bool IsShiny { get; set; }
        public bool IsMale { get; set; }
        public NatureType Nature { get; set; }
        public AbilityType Ability { get; set; }
        public PokemonType[] Types = new PokemonType[2];
        public StatValues IVs { get; set; }
        public StatValues EVs { get; set; }
        public List<MoveData> Moves { get; set; }
        public int Experience { get; set; }
        public StatusType StatusCondition { get; set; }
        public DateTime CaughtDate { get; set; }
        public GrowthRateType GrowthRate { get; set; } // Placeholder for enum use
        public int Friendship { get; set; } // Base 0, can be used for evolution mechanics
    }
    public class CaughtPokemonDataList
    {
        public List<CaughtPokemonData> CaughtPokemons { get; set; }
    }
}
