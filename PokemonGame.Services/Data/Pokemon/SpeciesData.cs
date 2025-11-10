using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.Pokemon
{
    public sealed class SpeciesData
    {
        private string speciesName;
        private int speciesID;
        public string SpeciesName { get => speciesName; set => speciesName = value; }
        public int SpeciesID { get => speciesID; set => speciesID = value; }
    }
}
