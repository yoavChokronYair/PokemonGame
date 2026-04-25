using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Map
{
    public class TileDomain
    {
        public int Tileid { get; set; }
        public CollisionType collisionType { get; set; }
        public TileType TileType { get; set; }
    }
    public class SquareDomain
    {
        // Position in square-space (each square = 2×2 tiles)
        public int Row { get; set; }
        public int Col { get; set; }

        // The 4 tile IDs that make up this square (for lookup)
        public int TileTopLeft { get; set; }
        public int TileTopRight { get; set; }
        public int TileBottomLeft { get; set; }
        public int TileBottomRight { get; set; }

        // Collision/interaction is decided per-square, not per-tile
        public CollisionType SquareType { get; set; }
        public TileType TileType { get; set; }  // ← new

    }
}
