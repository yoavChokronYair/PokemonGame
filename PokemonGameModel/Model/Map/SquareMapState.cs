using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Map
{
    public class MoveResult
    {
        public bool Success { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public CollisionType SquareType { get; set; }
    }

    public class SquareMapState
    {
        private SquareDomain[,] _squares;
        private MapDomain _activeMap;

        // Square-space dimensions
        public int SquareRows => _squares.GetLength(0);
        public int SquareCols => _squares.GetLength(1);

        public SquareMapState(MapDomain map)
        {
            _activeMap = map;
            _squares = BuildSquareGrid(map);
        }
        // -----------------------------------------------------------------------
        // Square access
        // -----------------------------------------------------------------------

        public SquareDomain GetSquare(int row, int col)
        {
            if ((uint)row >= (uint)SquareRows || (uint)col >= (uint)SquareCols)
                return null;
            return _squares[row, col];
        }

        // Convert tile-space position → square-space
        public (int row, int col) TileToSquare(int tileRow, int tileCol)
            => (tileRow / 2, tileCol / 2);

        // Convert square-space → top-left tile position
        public (int tileRow, int tileCol) SquareToTile(int squareRow, int squareCol)
            => (squareRow * 2, squareCol * 2);
        // -----------------------------------------------------------------------
        // Movement / collision
        // -----------------------------------------------------------------------
        public CollisionType GetCollision(int squareRow, int squareCol)
        {
            var square = GetSquare(squareRow, squareCol);   
            if (square == null) return CollisionType.Unwalkable;

            return square.SquareType; // or however your square stores it
        }
        public bool WalkableCheck(int squareRow, int squareCol) =>
            GetCollision(squareRow, squareCol) == CollisionType.None;

        public bool WildCheck(int squareRow, int squareCol) =>
            GetCollision(squareRow, squareCol) == CollisionType.WildGrass;

        public bool HmCheck(int squareRow, int squareCol) =>
            GetCollision(squareRow, squareCol) == CollisionType.HM;

        public bool JumpCheck(int squareRow, int squareCol, FacingDirection direction) =>
            GetCollision(squareRow, squareCol) switch
            {
                CollisionType.JumpLeft => direction == FacingDirection.Left,
                CollisionType.JumpRight => direction == FacingDirection.Right,
                CollisionType.JumpDown => direction == FacingDirection.Down,
                CollisionType.JumpUp => direction == FacingDirection.Up,
                _ => false
            };
        public bool CanMoveTo(int squareRow, int squareCol, FacingDirection direction)
        {
            var collision = GetCollision(squareRow, squareCol);

            return collision switch
            {
                CollisionType.None => true,
                CollisionType.WildGrass => true,
                CollisionType.HM => true, // handled separately / needs HM move
                CollisionType.JumpLeft => direction == FacingDirection.Left,
                CollisionType.JumpRight => direction == FacingDirection.Right,
                CollisionType.JumpDown => direction == FacingDirection.Down,
                CollisionType.JumpUp => direction == FacingDirection.Up,
                CollisionType.Unwalkable => false,
                _ => false
            };
        }

        public MoveResult TryMove(int fromRow, int fromCol, FacingDirection direction)
        {
            var (toRow, toCol) = direction switch
            {
                FacingDirection.Up => (fromRow - 1, fromCol),
                FacingDirection.Down => (fromRow + 1, fromCol),
                FacingDirection.Left => (fromRow, fromCol - 1),
                FacingDirection.Right => (fromRow, fromCol + 1),
                _ => (fromRow, fromCol)
            };

            if (!CanMoveTo(toRow, toCol, direction))
                return new MoveResult { Success = false, Row = fromRow, Col = fromCol };

            var landing = GetSquare(toRow, toCol);
            return new MoveResult
            {
                Success = true,
                Row = toRow,
                Col = toCol,
                SquareType = landing.SquareType,
            };
        }
        // -----------------------------------------------------------------------
        // Build
        // -----------------------------------------------------------------------

        private static SquareDomain[,] BuildSquareGrid(MapDomain map)
        {
            // tile grid is map.Height × map.Width
            // square grid is half that in each dimension
            int squareRows = map.Height / 2;
            int squareCols = map.Width / 2;

            var grid = new SquareDomain[squareRows, squareCols];
            var tiles = BuildTileArray(map.BackgroundBlocks, map);

            for (int sr = 0; sr < squareRows; sr++)
            {
                for (int sc = 0; sc < squareCols; sc++)
                {
                    int tileRow = sr * 2;
                    int tileCol = sc * 2;

                    int tl = tiles[tileRow, tileCol];
                    int tr = tiles[tileRow, tileCol + 1];
                    int bl = tiles[tileRow + 1, tileCol];
                    int br = tiles[tileRow + 1, tileCol + 1];

                    grid[sr, sc] = new SquareDomain
                    {
                        Row = sr,
                        Col = sc,
                        TileTopLeft = tl,
                        TileTopRight = tr,
                        TileBottomLeft = bl,
                        TileBottomRight = br,
                        SquareType = ResolveSquareType(tl, tr, bl, br),
                    };
                }
            }

            return grid;
        }

        /// Decides the square's type from its 4 tile IDs.
        /// Blocked wins over everything; otherwise top-left tile decides.
        private static CollisionType ResolveSquareType(int tl, int tr, int bl, int br)
        {
            // plug in your actual blocked/water/grass tile ID ranges here
            if (IsBlocked(tl) || IsBlocked(tr) || IsBlocked(bl) || IsBlocked(br))
                return CollisionType.Blocked;
            if (IsWater(tl)) return CollisionType.HM;
            if (IsGrass(tl)) return CollisionType.WildGrass;
            return CollisionType.None;
        }

        // ── tile-type helpers — replace with your actual tile ID logic ──
        private static bool IsBlocked(int id) => id == 0;
        private static bool IsWater(int id) => id >= 50 && id <= 59;
        private static bool IsGrass(int id) => id >= 40 && id <= 49;

        private static int[,] BuildTileArray(List<TileDomain> blocks, MapDomain map)
        {
            var tiles = new int[map.Height, map.Width];
            for (int b = 0; b < blocks.Count; b++)
            {
                var tile = blocks[b];
                if (tile is null) continue;
                tiles[b / map.Width, b % map.Width] = tile.Tileid;
            }
            return tiles;
        }
    }
}
