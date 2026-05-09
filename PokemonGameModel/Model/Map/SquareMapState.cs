using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Dialogue;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.DesignPatterns;

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
    }

    public class MoveResult
    {
        public bool Success { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public CollisionType SquareType { get; set; }
        public bool WildEncounterTriggered { get; set; }
        public int SpottedByNpcId { get; set; }
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

        // ── Dimensions ───────────────────────────────────────────────────────

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
            if (square == null) return CollisionType.Blocked;
            if (HasStationaryBlockerAt(row, col)) return CollisionType.Blocked;
            return square.SquareType;
        }

        public bool CanMoveTo(int row, int col, FacingDirection direction)
        {
            if (HasStationaryBlockerAt(row, col) || HasWalkingNpcAt(row, col))
                return false;

            return GetCollision(row, col) switch
            {
                CollisionType.None => true,
                CollisionType.WildGrass => true,
                CollisionType.HM => PlayerDomain.Instance.IsSurfing,
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
                return new MoveResult { Success = false, Row = fromRow, Col = fromCol };

            RebuildVisionLayer();
            IsInNpcVision(toRow, toCol, out int spottedBy);
            bool surfing =
                PlayerDomain.Instance.IsSurfing &&
                GetCollision(toRow, toCol) == CollisionType.HM;
            if (surfing)
            {
                var (extraRow, extraCol) = Step(toRow, toCol, direction);

                if (CanMoveTo(extraRow, extraCol, direction))
                {
                    toRow = extraRow;
                    toCol = extraCol;
                }
            }
            bool climbingWaterfall =
                PlayerDomain.Instance.IsSurfing &&
                GetCollision(toRow, toCol) == CollisionType.HM;
            if (climbingWaterfall)
            {
                while (true)
                {
                    var (nextRow, nextCol) = Step(toRow, toCol, direction);

                    if (GetCollision(nextRow, nextCol) != CollisionType.HM)
                        break;

                    if (!CanMoveTo(nextRow, nextCol, direction))
                        break;

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
            };
        }

        // ── Inspect ──────────────────────────────────────────────────────────

        public InspectResult TryInspect(int fromRow, int fromCol, FacingDirection facing)
        {
            var (targetRow, targetCol) = Step(fromRow, fromCol, facing);

            var npc = GetNpcAt(targetRow, targetCol);
            if (npc != null)
            {
                var set = npc.NpcInfo.GetDialogue(TriggerType.Interact);
                if (set != null)
                    return new InspectResult
                    {
                        Type = InspectResultType.NpcDialogue,
                        DialogueSet = set,
                        NpcName = npc.NpcInfo.Name ?? string.Empty,
                    };
            }

            var square = GetSquare(targetRow, targetCol);

            if (square?.SquareType == CollisionType.HM)
            {
                bool hasSurf =
                    PlayerDomain.Instance.Team.AnyPokemonKnows(HMMoves.Surf.ToString());

                if (!hasSurf)
                {
                    return new InspectResult
                    {
                        Type = InspectResultType.NeedHm,
                        Message = "You need Surf to cross the water.",
                    };
                }

                PlayerDomain.Instance.IsSurfing = true;

                return new InspectResult
                {
                    Type = InspectResultType.HmUsed,
                    Message = "Used Surf!",
                    TargetRow = targetRow,
                    TargetCol = targetCol,
                };
            }

            return new InspectResult { Type = InspectResultType.Nothing };
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
                if (npc.visionRange <= 0) continue;
                var (r, c) = NpcSquare(npc);
                switch (npc.VisionType)
                {
                    case VisionType.Normal: PaintLineVision(npc, r, c); break;
                    case VisionType.circular: PaintCircularVision(npc, r, c); break;
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

            // Build a flat tile-space collision lookup first (tile coords → type)
            var tileCollision = BuildTileCollisionGrid(map, map.Height, map.Width);

            var grid = new SquareDomain[rows, cols];

            for (int sr = 0; sr < rows; sr++)
            {
                for (int sc = 0; sc < cols; sc++)
                {
                    // Collect collision types for every tile in this square
                    var squareType = CollisionType.None;

                    for (int tr = 0; tr < tps && squareType != CollisionType.Blocked; tr++)
                    {
                        for (int tc = 0; tc < tps && squareType != CollisionType.Blocked; tc++)
                        {
                            int tileRow = sr * tps + tr;
                            int tileCol = sc * tps + tc;
                            var t = tileCollision[tileRow, tileCol];
                            if (t != CollisionType.None)
                                squareType = t; // last non-None wins; Blocked short-circuits
                        }
                    }

                    grid[sr, sc] = new SquareDomain
                    {
                        Row = sr,
                        Col = sc,
                        SquareType = squareType,
                        TileType = CollisionToTileType(squareType),
                    };
                }
            }

            return grid;
        }

        /// <summary>
        /// Expands every CollisionObject rectangle into a per-tile lookup array.
        /// </summary>
        private static CollisionType[,] BuildTileCollisionGrid(MapDomain map, int tileRows, int tileCols)
        {
            var grid = new CollisionType[tileRows, tileCols]; // default = None (0)

            foreach (var obj in map.CollisionObjects)
            {
                for (int dy = 0; dy < obj.Height; dy++)
                {
                    for (int dx = 0; dx < obj.Width; dx++)
                    {
                        int r = obj.Y + dy;
                        int c = obj.X + dx;
                        if ((uint)r < (uint)tileRows && (uint)c < (uint)tileCols)
                            grid[r, c] = obj.CollisionType;
                    }
                }
            }

            return grid;
        }

        // ── Private — HM resolution ───────────────────────────────────────────

        private static HMMoves HmForSquare(
            SquareDomain? square,
            bool isSurfing)
        {
            if (square == null)
                return HMMoves.None;

            if (square.SquareType != CollisionType.HM)
                return HMMoves.None;

            // already surfing + trying to go upward
            // means waterfall
            if (isSurfing)
                return HMMoves.Waterfall;

            // otherwise regular water entry
            return HMMoves.Surf;
        }

        // ── Private — TileType from CollisionType ─────────────────────────────

        private static TileType CollisionToTileType(CollisionType ct) => ct switch
        {
            CollisionType.HM => TileType.Water,
            _ => TileType.Ground,
        };

        // ── Private — NPC collision helpers ──────────────────────────────────

        private bool HasStationaryBlockerAt(int row, int col)
            => _map.Npc.Any((Func<NpcObjectDomain, bool>)(n =>
                n.MovementType != MovementType.Walking &&
                n.CollisionType == CollisionType.Blocked &&
                NpcSquare(n) == (row, col)));

        private bool HasWalkingNpcAt(int row, int col)
            => _map.Npc.Any((Func<NpcObjectDomain, bool>)(n =>
                n.MovementType == MovementType.Walking &&
                n.CollisionType == CollisionType.Blocked &&
                NpcSquare(n) == (row, col)));

        // ── Private — vision ─────────────────────────────────────────────────

        private void PaintLineVision(NpcObjectDomain npc, int npcRow, int npcCol)
        {
            var (dRow, dCol) = Delta(npc.direction);
            if (dRow == 0 && dCol == 0) return;

            for (int step = 1; step <= npc.visionRange; step++)
            {
                int r = npcRow + dRow * step;
                int c = npcCol + dCol * step;
                if (!InBounds(r, c)) break;
                _visionLayer[r, c] = npc.NpcInfo.Id;
                var col = GetCollision(r, c);
                if (col != CollisionType.None && col != CollisionType.WildGrass) break;
            }
        }

        private void PaintCircularVision(NpcObjectDomain npc, int npcRow, int npcCol)
        {
            for (int dr = -npc.visionRange; dr <= npc.visionRange; dr++)
                for (int dc = -npc.visionRange; dc <= npc.visionRange; dc++)
                {
                    if (Math.Max(Math.Abs(dr), Math.Abs(dc)) > npc.visionRange) continue;
                    int r = npcRow + dr;
                    int c = npcCol + dc;
                    if (InBounds(r, c) && HasLineOfSight(npcRow, npcCol, r, c))
                        _visionLayer[r, c] = npc.NpcInfo.Id;
                }
        }

        private bool HasLineOfSight(int fromRow, int fromCol, int toRow, int toCol)
        {
            int dr = toRow - fromRow;
            int dc = toCol - fromCol;
            int steps = Math.Max(Math.Abs(dr), Math.Abs(dc));
            if (steps == 0) return true;
            for (int i = 1; i < steps; i++)
            {
                int r = fromRow + (int)Math.Round((double)dr * i / steps);
                int c = fromCol + (int)Math.Round((double)dc * i / steps);
                var col = GetCollision(r, c);
                if (col != CollisionType.None && col != CollisionType.WildGrass) return false;
            }
            return true;
        }

        // ── Private — shared helpers ──────────────────────────────────────────

        private (int row, int col) NpcSquare(NpcObjectDomain npc)
            => TileToSquare(npc.Location.x, npc.Location.y);

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