using PokemonGame.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGameModel.Model.Data.MapData
{
    public class TownMapData
    {
        public string? Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<MapRegion>? Regions { get; set; }
        public string[]? connections { get; set; }//first value:left,second value:up,third value:right,fourth value:down
    }
    public class TownMapDataList 
    {
        public List<TownMapData>? maps;
    }
    public class MapRegion
    {
        public TileTypeFirstLayer Title { get; set; }
        public string? ID { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}

