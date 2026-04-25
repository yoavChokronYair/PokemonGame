using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Map;

namespace PokemonGame.Model.Model.Managers
{
    public class MapManager
    {
        // ---------------------------------------------------------------
        // State
        // ---------------------------------------------------------------
        private MapState _mapState;
        private SquareMapState _squareMapState;
        private readonly PlayerDomain _player;

        public MapDomain ActiveMap => _player.CurrentMap;

        public MapManager(PlayerDomain player)
        {
            _player = player;
            LoadMap(player.CurrentMap);
        }

        // ---------------------------------------------------------------
        // Map loading
        // ---------------------------------------------------------------
        public void LoadMap(MapDomain map)
        {
            _player.LastMapVisited = _player.CurrentMap;
            _player.CurrentMap = map;
            _mapState = new MapState(map);
            _squareMapState = new SquareMapState(map);
        }

        // ---------------------------------------------------------------
        // Movement
        // ---------------------------------------------------------------
        public MoveResult TryMove(FacingDirection direction)
        {
            _player.facingDirection = direction;

            var (squareRow, squareCol) = _squareMapState.TileToSquare(
                _player.playerLoc.x,
                _player.playerLoc.y
            );

            // Calculate destination in square-space
            int toRow = squareRow, toCol = squareCol;
            switch (direction)
            {
                case FacingDirection.Up: toRow--; break;
                case FacingDirection.Down: toRow++; break;
                case FacingDirection.Left: toCol--; break;
                case FacingDirection.Right: toCol++; break;
            }

            // ── Out of bounds → connection check first ───────────────────
            bool outOfBounds =
                toRow < 0 ||
                toRow >= _squareMapState.SquareRows ||
                toCol < 0 ||
                toCol >= _squareMapState.SquareCols;

            if (outOfBounds)
            {
                var connection = TryGetConnection(direction);
                if (connection != null)
                {
                    HandleConnection(connection, squareRow, squareCol);
                    var (sr, sc) = _squareMapState.TileToSquare(
                        _player.playerLoc.x, _player.playerLoc.y);
                    return new MoveResult
                    {
                        Success = true,
                        Row = sr,
                        Col = sc,
                        SquareType = CollisionType.None
                    };
                }

                // No connection — hard edge
                return new MoveResult { Success = false, Row = squareRow, Col = squareCol };
            }

            // ── Warp check ───────────────────────────────────────────────
            var warp = TryGetWarp(toRow, toCol);
            if (warp != null)
            {
                HandleWarp(warp);
                var (sr, sc) = _squareMapState.TileToSquare(
                    _player.playerLoc.x, _player.playerLoc.y);
                return new MoveResult
                {
                    Success = true,
                    Row = sr,
                    Col = sc,
                    SquareType = CollisionType.None
                };
            }

            // ── Normal collision + move ──────────────────────────────────
            var result = _squareMapState.TryMove(squareRow, squareCol, direction);

            if (!result.Success)
                return result;

            // Commit position
            var (tileRow, tileCol) = _squareMapState.SquareToTile(result.Row, result.Col);
            _player.playerLoc = (tileRow, tileCol);

            return result;
        }

        // ---------------------------------------------------------------
        // Viewport
        // ---------------------------------------------------------------
        public (int[,] background, int[,] foreground) GetViewport()
            => _mapState.BuildViewPort(_player);

        // ---------------------------------------------------------------
        // Collision queries
        // ---------------------------------------------------------------
        public CollisionType GetCollisionAt(int squareRow, int squareCol)
            => _squareMapState.GetCollision(squareRow, squareCol);

        public bool IsWildTile()
        {
            var (sr, sc) = CurrentSquare();
            return _squareMapState.WildCheck(sr, sc);
        }

        public bool IsWaterTile()
        {
            var (sr, sc) = CurrentSquare();
            return _squareMapState.HmCheck(sr, sc);
        }

        // ---------------------------------------------------------------
        // Warp helpers
        // ---------------------------------------------------------------
        private WrapDomain TryGetWarp(int squareRow, int squareCol)
        {
            return ActiveMap.Wraps.FirstOrDefault(w =>
                w.WrapLoc.x == squareRow &&
                w.WrapLoc.y == squareCol);
        }

        private void HandleWarp(WrapDomain warp)
        {
            LoadMap(warp.TargetMap);
            _player.playerLoc = _squareMapState.SquareToTile(
                warp.SpawnLoc.row, warp.SpawnLoc.col);
        }

        // ---------------------------------------------------------------
        // Connection helpers
        // ---------------------------------------------------------------

        // Takes the direction the player is moving, not the destination coords,
        // so we never need out-of-bounds square lookups.
        private ConnectedMapDomain TryGetConnection(FacingDirection direction)
        {
            var connDir = direction switch
            {
                FacingDirection.Up => ConnectionDirection.North,
                FacingDirection.Down => ConnectionDirection.South,
                FacingDirection.Left => ConnectionDirection.West,
                FacingDirection.Right => ConnectionDirection.East,
                _ => (ConnectionDirection?)null
            };

            if (connDir == null) return null;

            return ActiveMap.ConnectedMaps
                .FirstOrDefault(c => c.ConnectionDirection == connDir.Value);
        }

        private void HandleConnection(ConnectedMapDomain connection,
                                      int squareRow, int squareCol)
        {
            int newSquareRow = squareRow;
            int newSquareCol = squareCol;

            // Translate the player's square position into the neighbour map's
            // coordinate space, landing them on the opposite edge.
            switch (connection.ConnectionDirection)
            {
                case ConnectionDirection.North:
                    // Coming from south → land on the last row of the north map
                    newSquareRow = (connection.ConnectedMap.Height / 2) - 1;
                    newSquareCol = squareCol - connection.Margin / 2;
                    break;

                case ConnectionDirection.South:
                    // Coming from north → land on row 0 of the south map
                    newSquareRow = 0;
                    newSquareCol = squareCol - connection.Margin / 2;
                    break;

                case ConnectionDirection.West:
                    // Coming from east → land on the last col of the west map
                    newSquareRow = squareRow - connection.Margin / 2;
                    newSquareCol = (connection.ConnectedMap.Width / 2) - 1;
                    break;

                case ConnectionDirection.East:
                    // Coming from west → land on col 0 of the east map
                    newSquareRow = squareRow - connection.Margin / 2;
                    newSquareCol = 0;
                    break;
            }

            LoadMap(connection.ConnectedMap);

            var (tileRow, tileCol) = _squareMapState.SquareToTile(newSquareRow, newSquareCol);
            _player.playerLoc = (tileRow, tileCol);
        }

        // ---------------------------------------------------------------
        // Utility
        // ---------------------------------------------------------------
        private (int row, int col) CurrentSquare()
            => _squareMapState.TileToSquare(_player.playerLoc.x, _player.playerLoc.y);
    }
}