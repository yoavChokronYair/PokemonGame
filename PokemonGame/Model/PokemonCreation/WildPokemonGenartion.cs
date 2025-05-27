using PokemonGame.Interface;
using PokemonGame.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Model.PokemonCreation
{
    public class WildPokemonGenartion:IPokemon
    {
        public string Species { get; private set; }
        public string Nickname { get; set; }
        public int level { get; set; }
        public int CurrentHP { get; set; }
        public IStatValues IVs { get; private set; }
        public IStatValues EVs { get; private set; }
        public List<IMove> Moves { get; private set; }
        public WildPokemonGenartion(Encounter species)
        {
            Random random = new Random();
            this.Species = species.Pokemon;
            this.Nickname = species.Pokemon;
            this.level = random.Next(species.MinLevel, species.MaxLevel+1); // Random level between the min and max level of an encounter
            this.CurrentHP = species.
        }
    }
}
