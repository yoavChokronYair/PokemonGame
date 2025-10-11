using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.PokemonCreation;
using System;
using System.Collections.Generic;

namespace PokemonGame.Model.Data.Player
{
    public class CaughtPokemonData : PokemonData
    {
        public int ID { get; set; }
        public string? Nickname { get; set; }
        public int Level { get; set; }
        public int CurrentHP { get; set; }
        public bool IsShiny { get; set; }
        public bool IsMale { get; set; }
        public NatureType Nature { get; set; }
        public int Experience { get; set; }
        public StatusType StatusCondition { get; set; }
        public DateTime CaughtDate { get; set; }
        public  List<MoveData> ExsitingMoves { get; set; }
        public int Friendship { get; set; } // Base 0, can be used for evolution mechanics
    }
    public class CaughtPokemonDataList
    {
        public List<CaughtPokemonData> CaughtPokemons { get; set; }
    }
}
