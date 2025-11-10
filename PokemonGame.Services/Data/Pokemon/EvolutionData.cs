using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.Pokemon
{
    public sealed class EvolutionData
    {
        //TODO: change to enum the strings
        private ushort levelRequired;
        private SpeciesData babySpecies;
        private EvoMethodType method;
        private SpeciesData species;
        private FormsData form;

        public ushort LevelRequired { get => levelRequired; set => levelRequired = value; }
        public SpeciesData BabySpecies { get => babySpecies; set => babySpecies = value; }
        public EvoMethodType Method { get => method; set => method = value; }
        public SpeciesData Species { get => species; set => species = value; }
        public FormsData Form { get => form; set => form = value; }
    }
}
