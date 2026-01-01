using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public sealed class EggMoveData
    {
        public int EggMoveType { get; set ; }//should be enum in model 
        public int PokemonID { get; set; }
    }
}
