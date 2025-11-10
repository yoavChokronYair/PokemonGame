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
        private SpeciesType babySpecies;
        private EvoMethodType method;
        private SpeciesType species;
        private FormType form;

        public ushort LevelRequired { get => levelRequired; set => levelRequired = value; }
        public SpeciesType BabySpecies { get => babySpecies; set => babySpecies = value; }
        public EvoMethodType Method { get => method; set => method = value; }
        public SpeciesType Species { get => species; set => species = value; }
        public FormType Form { get => form; set => form = value; }
    }
}
