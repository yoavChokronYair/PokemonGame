using System.Collections.Generic;

namespace PokemonGame.Model.Data
{
    public class MapData
    {
        public string name;
        public int width;
        public int height;
        public string tiles;
    }
    public class MapDataList
    {
        public List<MapData> maps;
    }
}
