using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Config
{
    public static class MapConstants
    {
        public const int BlockSize = 4;
        public const int ViewRowSize = 10;
        public const int ViewColSize = 10;
    }
    public static class PlayerSprites
    {
        // Each direction has 4 tiles: [topLeft, topRight, bottomLeft, bottomRight]
        public static readonly Dictionary<FacingDirection, (int TL, int TR, int BL, int BR)> Tiles = new()
        {
            { FacingDirection.Down,  (TL: 10, TR: 11, BL: 12, BR: 13) },
            { FacingDirection.Up,    (TL: 14, TR: 15, BL: 16, BR: 17) },
            { FacingDirection.Left,  (TL: 18, TR: 19, BL: 20, BR: 21) },
            { FacingDirection.Right, (TL: 22, TR: 23, BL: 24, BR: 25) },
        };
    }
}
