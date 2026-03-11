using PokemonGame.Model.Domain.Pokemon;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Domain.Battle
{
    public class BattleDomain
    {
        public PokemonData attacker { get; set; }
        public PokemonData defender { get; set; }
        public void init(PokemonData pokemonData, PokemonData pokemonData1)
        {
            this.attacker = pokemonData;
            this.defender = pokemonData1;
        }
        public void switchAttackerDefender()
        {
            PokemonData defender = this.defender;
            this.defender = this.attacker;
            this.attacker = defender;
        }
    }
    
}
