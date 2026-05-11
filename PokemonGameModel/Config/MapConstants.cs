using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Config
{
    public static class MapConstants
    {
        public const int TileSize = 8;       // one tile = 8x8 pixels
        public const int SquareSize = 16;    // one square = 16x16 pixels = 2x2 tiles
        public const int TilesPerSquare = SquareSize / TileSize; // = 2

        public const int ViewRowSize = 35;   // viewport height in tiles
        public const int ViewColSize = 25;   // viewport width in tiles
    }

    public static class PlayerSprites
    {
        // 3 frames per direction: [0] left leg, [1] standing, [2] right leg
        public static readonly Dictionary<FacingDirection, string[]> Frames = new()
        {
            [FacingDirection.Down] = new[] { "sprite_0.png", "sprite_1.png", "sprite_2.png" },
            [FacingDirection.Up] = new[] { "sprite_3.png", "sprite_4.png", "sprite_5.png" },
            [FacingDirection.Left] = new[] { "sprite_6.png", "sprite_7.png", "sprite_8.png" },
            [FacingDirection.Right] = new[] { "sprite_9.png", "sprite_10.png", "sprite_11.png" },
        };

        private static readonly int[] WalkCycle = { 0, 1, 2, 1 };

        public static string GetFrame(FacingDirection dir, int tick, bool isMoving)
        {
            if (!Frames.TryGetValue(dir, out var frames))
                frames = Frames[FacingDirection.Down];

            int frame = isMoving ? WalkCycle[tick % WalkCycle.Length] : 1;
            return frames[frame];
        }
    }
}