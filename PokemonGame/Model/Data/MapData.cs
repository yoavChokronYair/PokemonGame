using PokemonGame.Model.Map;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Model.Data
{
    public class MapData
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<MapRegion> Regions { get; set; }
        public string LeftMap {  get; set; }
        public string RightMap {  get; set; }
        public string UpMap {  get; set; }
        public string DownMap { get; set; }
    }
    public class MapDataList
    {
        public List<MapData> maps;
    }
    public class MapRegion
    {
        public string Name { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

}
