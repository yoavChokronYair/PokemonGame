using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public sealed class PokemonFormData
    {
        public int PokemonID { get; set; }
        public string FormName { get; set; }
        public byte FormID { get; set; }
        
    }
}
