using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Config
{
    public static class MapConstants
    {
        public const int TileSize = 8;       // one tile = 8x8 pixels
        public const int SquareSize = 16;    // one square = 16x16 pixels = 2x2 tiles
        public const int TilesPerSquare = SquareSize / TileSize; // = 2

        public const int ViewRowSize = 10;   // viewport height in tiles
        public const int ViewColSize = 10;   // viewport width in tiles
    }

    public static class PlayerSprites
    {
        public static readonly Dictionary<FacingDirection, (int TL, int TR, int BL, int BR)> Tiles = new()
        {
            { FacingDirection.Down,  (TL: 10, TR: 11, BL: 12, BR: 13) },
            { FacingDirection.Up,    (TL: 14, TR: 15, BL: 16, BR: 17) },
            { FacingDirection.Left,  (TL: 18, TR: 19, BL: 20, BR: 21) },
            { FacingDirection.Right, (TL: 22, TR: 23, BL: 24, BR: 25) },
        };
    }
}