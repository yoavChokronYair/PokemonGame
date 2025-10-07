using PokemonGame.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGameModel.Model.Data.MapData
{
    public class RouteMapData
    {
        public int ID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<MapRegion>? Regions { get; set; }
        public List<Encounter>? Encounters { get; set; }
        public int[]? TownConnections { get; set; }//first value:first town,second value:second town 
        public int pathID { get; set; }
    }
    public class RouteMapDataList
    {
        public List<RouteMapData>? maps;
    }
    public class Encounter
    {
        public string? Name { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public double Rarity {  get; set; }
        public string? Environment { get; set; }
    }
    public class RouteRegion
    {
        public TileType TileType { get; set; }
        public int ID { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
