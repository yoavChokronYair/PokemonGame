using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Map
{
    public class TileDomain
    {
        public int Tileid { get; set; }

        /// <summary>Tile-space column (X). Set by MapLoader from DB.</summary>
        public int X { get; set; }

        /// <summary>Tile-space row (Y). Set by MapLoader from DB.</summary>
        public int Y { get; set; }

        public CollisionType collisionType { get; set; }
        public TileType TileType { get; set; }
    }

    public class SquareDomain
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public int TileTopLeft { get; set; }
        public int TileTopRight { get; set; }
        public int TileBottomLeft { get; set; }
        public int TileBottomRight { get; set; }

        public CollisionType SquareType { get; set; }
        public TileType TileType { get; set; }
    }
}
