using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.Pokemon
{
    public sealed class PokemonFormData
    {
        private int pokemonID;
        private string formName;
        private byte formID;
        public int PokemonID { get => pokemonID; set => pokemonID = value; }
        public string FormName { get => formName; set => formName = value; }
        public byte FormID { get => formID; set => formID = value; }
    }
}
