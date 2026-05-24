using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Dialogue;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Map
{
    // ── Result types ─────────────────────────────────────────────────────────

    public class InspectResult
    {
        public InspectResultType Type { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TargetRow { get; set; }

        public int TargetCol { get; set; }

        public DialogueSet? DialogueSet { get; set; }

        public string NpcName { get; set; } = string.Empty;

        public HMMoves RequiredHm { get; set; } = HMMoves.None;
    }

    public class MoveResult
    {
        public bool Success { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public CollisionType SquareType { get; set; }
        public bool WildEncounterTriggered { get; set; }
        public int SpottedByNpcId { get; set; }
        public FacingDirection Direction { get; set; } // ← add this
    }

    // ── SquareMapState ───────────────────────────────────────────────────────

    public class SquareMapState
    {
        private readonly MapDomain _map;
        private readonly SquareDomain[,] _squares;
        private readonly int[,] _visionLayer;

        public SquareMapState(MapDomain map)
        {
            _map = map;
            _squares = BuildSquareGrid(map);
            _visionLayer = new int[SquareRows, SquareCols];
            RebuildVisionLayer();
        }

        // ── Dimensions ───────────────────────────────────────────────c───────

        public int SquareRows => _squares.GetLength(0);
        public int SquareCols => _squares.GetLength(1);

        // ── Coordinate conversion ─────────────────────────────────────────────

        public (int row, int col) TileToSquare(int tileRow, int tileCol)
            => (tileRow / MapConstants.TilesPerSquare, tileCol / MapConstants.TilesPerSquare);

        public (int tileRow, int tileCol) SquareToTile(int squareRow, int squareCol)
            => (squareRow * MapConstants.TilesPerSquare, squareCol * MapConstants.TilesPerSquare);

        // ── Square access ────────────────────────────────────────────────────

        public SquareDomain? GetSquare(int row, int col)
            => InBounds(row, col) ? _squares[row, col] : null;

        // ── Collision ────────────────────────────────────────────────────────

        public CollisionType GetCollision(int row, int col)
        {
            var square = GetSquare(row, col);

            if (square == null)
                return CollisionType.Blocked;

            if (HasBlockingNpcAt(row, col))
                return CollisionType.Blocked;

            return square.SquareType;
        }

        public bool CanMoveTo(int row, int col, FacingDirection direction)
        {
            if (HasBlockingNpcAt(row, col))
                return false;

            return GetCollision(row, col) switch
            {
                CollisionType.None => true,
                CollisionType.WildGrass => true,
                CollisionType.HM => PlayerDomain.Instance.trainerMapLocDomain.IsSurfing,
                CollisionType.JumpLeft => direction == FacingDirection.Left,
                CollisionType.JumpRight => direction == FacingDirection.Right,
                CollisionType.JumpDown => direction == FacingDirection.Down,
                CollisionType.JumpUp => direction == FacingDirection.Up,
                _ => false,
            };
        }

        public bool WildCheck(int row, int col)
        {
            var square = GetSquare(row, col);
            return square?.SquareType == CollisionType.WildGrass
                && RNGHelper.TryWildEncounter(10);
        }
        public void ClearTile(int row, int col)
        {
            var square = GetSquare(row, col);
            if (square != null) square.SquareType = CollisionType.None;
        }

        // ── Movement ─────────────────────────────────────────────────────────

        public MoveResult TryMove(int fromRow, int fromCol, FacingDirection direction)
        {
            var (toRow, toCol) = Step(fromRow, fromCol, direction);

            if (!CanMoveTo(toRow, toCol, direction))
                return new MoveResult { Success = false, Row = fromRow, Col = fromCol, Direction = direction };

            RebuildVisionLayer();
            IsInNpcVision(toRow, toCol, out int spottedBy);

            // ── Bug #2 fix (Critical): Surfing double-step needs bounds guard ─
            //
            // OLD code called CanMoveTo(extraRow, extraCol) without first
            // checking InBounds — CanMoveTo calls GetSquare which returns null
            // out-of-bounds, but GetCollision then returns Blocked, so it
            // "worked" accidentally. However HasWalkingNpcAt / HasStationaryBlockerAt
            // also run and index _visionLayer without a bounds check, which
            // could throw. Guard explicitly with InBounds.
            bool surfing =
                PlayerDomain.Instance.trainerMapLocDomain.IsSurfing &&
                GetCollision(toRow, toCol) == CollisionType.HM;

            if (surfing)
            {
                var (extraRow, extraCol) = Step(toRow, toCol, direction);

                // ── Bug #2 fix: guard bounds before CanMoveTo ────────────────
                if (InBounds(extraRow, extraCol) && CanMoveTo(extraRow, extraCol, direction))
                {
                    toRow = extraRow;
                    toCol = extraCol;
                }
            }

            // ── Bug #3 fix (High): Waterfall while(true) loop — add iteration cap
            //
            // OLD code: unbounded while(true) — if the map data has a run of HM
            // tiles with no non-HM tile ahead, this loops forever.
            // FIX: cap at SquareRows (the longest possible straight run on the map).
            bool climbingWaterfall =
                PlayerDomain.Instance.trainerMapLocDomain.IsSurfing &&
                GetCollision(toRow, toCol) == CollisionType.HM;

            if (climbingWaterfall)
            {
                int maxSteps = SquareRows; // can never need more steps than map height
                for (int step = 0; step < maxSteps; step++)
                {
                    var (nextRow, nextCol) = Step(toRow, toCol, direction);

                    // ── Also guard bounds here (same class of bug as #2) ─────
                    if (!InBounds(nextRow, nextCol)) break;
                    if (GetCollision(nextRow, nextCol) != CollisionType.HM) break;
                    if (!CanMoveTo(nextRow, nextCol, direction)) break;

                    toRow = nextRow;
                    toCol = nextCol;
                }
            }

            return new MoveResult
            {
                Success = true,
                Row = toRow,
                Col = toCol,
                SquareType = GetSquare(toRow, toCol)!.SquareType,
                WildEncounterTriggered = WildCheck(toRow, toCol),
                SpottedByNpcId = spottedBy,
                Direction = direction, // ← add this
            };
        }

        // ── Inspect ──────────────────────────────────────────────────────────

        public InspectResult TryInspect(
     int fromRow,
     int fromCol,
     FacingDirection facing)
        {
            var (targetRow, targetCol) = Step(fromRow, fromCol, facing);

            var npc = GetNpcAt(targetRow, targetCol);

            if (npc != null)
            {
                var set = npc.NpcInfo.GetDialogue(TriggerType.Interact);

                if (set != null)
                {
                    return new InspectResult
                    {
                        Type = InspectResultType.NpcDialogue,
                        DialogueSet = set,
                        NpcName = npc.NpcInfo.Name ?? string.Empty
                    };
                }
            }

            var currentSquare = GetSquare(fromRow, fromCol);
            var targetSquare = GetSquare(targetRow, targetCol);

            if (targetSquare == null)
            {
                return new InspectResult
                {
                    Type = InspectResultType.Nothing,
                    Message = "There is nothing here."
                };
            }

            if (!TryResolveRequiredHm(
                    currentSquare,
                    targetSquare,
                    PlayerDomain.Instance.trainerMapLocDomain.IsSurfing,
                    out HMMoves requiredHm))
            {
                return new InspectResult
                {
                    Type = InspectResultType.Nothing,
                    Message = "There is nothing special here."
                };
            }

            bool hasHm =
                PlayerDomain.Instance.Team?.AnyPokemonKnows(requiredHm.ToString()) == true;

            if (!hasHm)
            {
                return new InspectResult
                {
                    Type = InspectResultType.NeedHm,
                    Message = $"You need {requiredHm} here.",
                    TargetRow = targetRow,
                    TargetCol = targetCol,
                    RequiredHm = requiredHm
                };
            }

            return new InspectResult
            {
                Type = InspectResultType.HmUsed,
                Message = $"Use {requiredHm}?",
                TargetRow = targetRow,
                TargetCol = targetCol,
                RequiredHm = requiredHm
            };
        }
        private static bool TryResolveRequiredHm(
            SquareDomain? currentSquare,
            SquareDomain targetSquare,
            bool isSurfing,
            out HMMoves requiredHm)
        {
            requiredHm = HMMoves.None;

            if (targetSquare.SquareType == CollisionType.CutTree)
            {
                requiredHm = HMMoves.Cut;
                return true;
            }

            if (targetSquare.SquareType == CollisionType.HM &&
                targetSquare.TileType == TileType.Water)
            {
                requiredHm = isSurfing
                    ? HMMoves.Waterfall
                    : HMMoves.Surf;

                return true;
            }

            if (targetSquare.SquareType == CollisionType.HM &&
                targetSquare.TileType == TileType.Cave)
            {
                requiredHm = HMMoves.Strength;
                return true;
            }

            return false;
        }
        // ── NPC queries ──────────────────────────────────────────────────────

        public NpcObjectDomain? GetNpcAt(int squareRow, int squareCol)
            => _map.Npc.FirstOrDefault(n => NpcSquare(n) == (squareRow, squareCol));

        // ── Vision ───────────────────────────────────────────────────────────

        public int[,] VisionLayer => _visionLayer;

        public bool IsInNpcVision(int row, int col, out int npcId)
        {
            npcId = InBounds(row, col) ? _visionLayer[row, col] : 0;
            return npcId != 0;
        }

        public void RebuildVisionLayer()
        {
            Array.Clear(_visionLayer, 0, _visionLayer.Length);
            foreach (var npc in _map.Npc)
            {
                if (npc.VisionRange <= 0) continue;
                var (r, c) = NpcSquare(npc);
                switch (npc.VisionType)
                {
                    case VisionType.Normal: PaintLineVision(npc, r, c); break;
                    case VisionType.Circular: PaintCircularVision(npc, r, c); break;
                }
            }
        }

        // ── Private — grid construction ───────────────────────────────────────

        /// <summary>
        /// Builds the square grid purely from CollisionObjects.
        /// Tile layers are visual-only and play no role here.
        /// Each square is TilesPerSquare × TilesPerSquare tiles.
        /// A square's CollisionType is the highest-priority collision
        /// found in any of its constituent tiles, with Blocked winning over all.
        /// Squares with no collision object default to None (walkable).
        /// </summary>
        private static SquareDomain[,] BuildSquareGrid(MapDomain map)
        {
            int tps = MapConstants.TilesPerSquare;
            int rows = map.Height / tps;
            int cols = map.Width / tps;

            var grid = new SquareDomain[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    grid[row, col] = new SquareDomain
                    {
                        Row = row,
                        Col = col,
                        SquareType = CollisionType.None,
                        TileType = TileType.Ground
                    };
                }
            }

            ApplyVisualTiles(grid, rows, cols, map.BackgroundBlocks);
            ApplyVisualTiles(grid, rows, cols, map.Blocks);
            ApplyCollisionObjects(grid, rows, cols, map.CollisionObjects);

            return grid;
        }
        private static void ApplyVisualTiles(
    SquareDomain[,] grid,
    int rows,
    int cols,
    IEnumerable<TileDomain> tiles)
        {
            int tps = MapConstants.TilesPerSquare;

            foreach (var tile in tiles)
            {
                int row = tile.Y / tps;
                int col = tile.X / tps;

                if ((uint)row >= (uint)rows ||
                    (uint)col >= (uint)cols)
                {
                    continue;
                }

                var square = grid[row, col];

                if (tile.TileType != TileType.Ground)
                    square.TileType = tile.TileType;

                if (tile.collisionType != CollisionType.None)
                {
                    PaintCollision(square, tile.collisionType);
                    continue;
                }

                if (tile.TileType == TileType.Water &&
                    square.SquareType == CollisionType.None)
                {
                    square.SquareType = CollisionType.HM;
                }

                if (tile.TileType == TileType.Objects &&
                    square.SquareType == CollisionType.None)
                {
                    square.SquareType = CollisionType.Blocked;
                }

                if (tile.TileType == TileType.Grass &&
                    square.SquareType == CollisionType.None)
                {
                    square.SquareType = CollisionType.WildGrass;
                }
            }
        }

        private static void ApplyCollisionObjects(
            SquareDomain[,] grid,
            int rows,
            int cols,
            IEnumerable<CollisionObjectDomain> objects)
        {
            int tps = MapConstants.TilesPerSquare;

            foreach (var obj in objects)
            {
                int startRow = obj.Y / tps;
                int startCol = obj.X / tps;
                int endRow = (obj.Y + obj.Height - 1) / tps;
                int endCol = (obj.X + obj.Width - 1) / tps;

                for (int row = startRow; row <= endRow; row++)
                {
                    for (int col = startCol; col <= endCol; col++)
                    {
                        if ((uint)row >= (uint)rows ||
                            (uint)col >= (uint)cols)
                        {
                            continue;
                        }

                        PaintCollision(grid[row, col], obj.CollisionType);
                    }
                }
            }
        }

        private static void PaintCollision(
            SquareDomain square,
            CollisionType collision)
        {
            if (square.SquareType == CollisionType.Blocked)
                return;

            if (collision == CollisionType.Blocked ||
                square.SquareType == CollisionType.None)
            {
                square.SquareType = collision;
                square.TileType = CollisionToTileType(collision);
            }
        }
        /// <summary>
        /// Expands every CollisionObject rectangle into a per-tile lookup array.
        /// </summary>



        // ── Private — TileType from CollisionType ─────────────────────────────

        private static TileType CollisionToTileType(CollisionType ct) => ct switch
        {
            CollisionType.HM => TileType.Water,
            CollisionType.CutTree => TileType.Objects,
            CollisionType.WildGrass => TileType.Grass,
            CollisionType.Blocked => TileType.Objects,
            _ => TileType.Ground
        };

        // ── Private — NPC collision helpers ──────────────────────────────────

        private bool HasBlockingNpcAt(int row, int col)
        {
            return _map.Npc.Any(n =>
            {
                if (n.CollisionType != CollisionType.Blocked)
                    return false;

                var (npcRow, npcCol) =
                    TileToSquare(n.Location.y, n.Location.x);

                return npcRow == row && npcCol == col;
            });
        }

        // ── Private — vision ─────────────────────────────────────────────────

        private void PaintLineVision(
             NpcObjectDomain npc,
             int npcRow,
             int npcCol)
        {
            var (dRow, dCol) = Delta(npc.Direction);

            if (dRow == 0 && dCol == 0)
                return;

            for (int step = 1; step <= npc.VisionRange; step++)
            {
                int row = npcRow + dRow * step;
                int col = npcCol + dCol * step;

                if (!InBounds(row, col))
                    break;

                if (IsVisionBlocking(row, col))
                    break;

                _visionLayer[row, col] = npc.NpcInfo.Id;
            }
        }
        private bool IsVisionBlocking(int row, int col)
        {
            CollisionType collision = GetCollision(row, col);

            return collision != CollisionType.None &&
                   collision != CollisionType.WildGrass;
        }
        private void PaintCircularVision(
     NpcObjectDomain npc,
     int npcRow,
     int npcCol)
        {
            int radius = npc.VisionRange;
            int radiusSquared = radius * radius;

            for (int dRow = -radius; dRow <= radius; dRow++)
            {
                for (int dCol = -radius; dCol <= radius; dCol++)
                {
                    if (dRow == 0 && dCol == 0)
                        continue;

                    if (dRow * dRow + dCol * dCol > radiusSquared)
                        continue;

                    int row = npcRow + dRow;
                    int col = npcCol + dCol;

                    if (!InBounds(row, col))
                        continue;

                    if (IsVisionBlocking(row, col))
                        continue;

                    if (HasLineOfSight(npcRow, npcCol, row, col))
                        _visionLayer[row, col] = npc.NpcInfo.Id;
                }
            }
        }
        private bool HasLineOfSight(
     int fromRow,
     int fromCol,
     int toRow,
     int toCol)
        {
            int x0 = fromCol;
            int y0 = fromRow;
            int x1 = toCol;
            int y1 = toRow;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            int x = x0;
            int y = y0;

            while (true)
            {
                if (x == x1 && y == y1)
                    return true;

                int e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }

                if (x == x1 && y == y1)
                    return true;

                if (!InBounds(y, x))
                    return false;

                if (IsVisionBlocking(y, x))
                    return false;
            }
        }

        // ── Private — shared helpers ──────────────────────────────────────────

        private (int row, int col) NpcSquare(NpcObjectDomain npc)
            => TileToSquare(npc.Location.y, npc.Location.x);

        private bool InBounds(int row, int col)
            => (uint)row < (uint)SquareRows && (uint)col < (uint)SquareCols;

        private static (int row, int col) Step(int row, int col, FacingDirection dir)
        {
            var (dr, dc) = Delta(dir);
            return (row + dr, col + dc);
        }

        private static (int dRow, int dCol) Delta(FacingDirection dir) => dir switch
        {
            FacingDirection.Up => (-1, 0),
            FacingDirection.Down => (1, 0),
            FacingDirection.Left => (0, -1),
            FacingDirection.Right => (0, 1),
            _ => (0, 0),
        };
    }
}