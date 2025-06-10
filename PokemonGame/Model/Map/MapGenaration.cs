using PokemonGame.Enums;
using PokemonGame.Model.Data;

namespace PokemonGame.Model.Map
{
    public class MapGenaration
    {
        public TileType[,] mapTiles { get; set; }

        public MapGenaration(MapData mapData)
        {
            
            this.mapTiles = new TileType[mapData.height, mapData.width];
            for (int i = 0; i < mapData.height; i++)
            {
                for (int j = 0; j < mapData.width; j++)
                {
                    int index = i * mapData.width + j;
                    if(!(index % 2 == 0))
                    {
                        index++;
                    }

                    char tileChar = mapData.tiles[index];
                    mapTiles[i, j] = (TileType)tileChar;
                }
            }
        }

    }
}
