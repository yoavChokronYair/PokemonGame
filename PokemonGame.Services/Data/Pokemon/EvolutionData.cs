using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.Pokemon
{
    public sealed class EvolutionData
    {
        
        private int pokemonID;
        private ushort levelRequired;
        private string babySpeciesName;
        private EvoMethodType method;
        private string speciesName;
        private PokemonFormData form;

        public int PokemonID { get => pokemonID; set => pokemonID = value; }
        public ushort LevelRequired { get => levelRequired; set => levelRequired = value; }
        public string BabySpeciesName { get => babySpeciesName; set => babySpeciesName = value; }
        public EvoMethodType Method { get => method; set => method = value; }
        public string SpeciesName { get => speciesName; set => speciesName = value; }
        public PokemonFormData Form { get => form; set => form = value; }
    }
}
