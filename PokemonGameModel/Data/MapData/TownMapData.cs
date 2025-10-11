using PokemonGame.Enums;
using PokemonGame.Model.Data.MapData;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PokemonGameModel.Model.Data.MapData
{
    public class TownMapData : WorldData
    {
        public string? Name { get; set; }
        public List<WorldRegion>? Regions { get; set; }
        public int[]? connections { get; set; }//first value:left,second value:up,third value:right,fourth value:down
        public int pathID { get; set; }
    }
    public class TownMapDataList 
    {
        public List<TownMapData>? maps;
    }
   
}

