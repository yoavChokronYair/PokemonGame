using System;
using System.Collections.Generic;
using System.Linq;
using PokemonGame.Enums;

namespace PokemonGame.Model.Data
{
    public class PokemonData
    {
            public int Number { get; set; }
            public string Name { get; set; }
            public PokemonType Type1 { get; set; }
            public PokemonType Type2 { get; set; }
            public string Ability1 { get; set; }
            public string Ability2 { get; set; }
            public string HiddenAbility { get; set; }
            public int HP { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int SpAtk { get; set; }
            public int SpDef { get; set; }
            public int Speed { get; set; }

    }
    public class PokemonDataList
    {
        public List<PokemonData> Starters { get; set; } = new List<PokemonData>();
        public List<PokemonData> Pokemons { get; set; } = new List<PokemonData>();

        public List<PokemonData> AllPokemons
        {
            get
            {
                return Starters.Concat(Pokemons).ToList();
            }
        }
    }
}
