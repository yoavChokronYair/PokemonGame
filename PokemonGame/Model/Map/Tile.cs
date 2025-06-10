using PokemonGame.Enums;

namespace PokemonGame.Model.Map
{
    public struct Tile
    {
        public TileType Type { get; set; }
        public Tile(TileType type)
        {
            Type = type;
        }
    }
}
