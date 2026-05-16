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

       public InspectResult TryInspect(int fromRow, int fromCol, FacingDirection facing)
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
                        NpcName = npc.NpcInfo.Name ?? string.Empty,
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

            if (targetSquare.SquareType == CollisionType.HM)
            {
                HMMoves requiredHm = ResolveRequiredHm(
                    currentSquare,
                    targetSquare,
                    PlayerDomain.Instance.trainerMapLocDomain.IsSurfing);

                bool hasHm =
                    PlayerDomain.Instance.Team.AnyPokemonKnows(requiredHm.ToString());

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

                if (requiredHm == HMMoves.Surf)
                {
                    PlayerDomain.Instance.trainerMapLocDomain.IsSurfing = true;
                }

                return new InspectResult
                {
                    Type = InspectResultType.HmUsed,
                    Message = $"Used {requiredHm}!",
                    TargetRow = targetRow,
                    TargetCol = targetCol,
                    RequiredHm = requiredHm
                };
            }

            return new InspectResult
            {
                Type = InspectResultType.Nothing,
                Message = "There is nothing special here."
            };
        }
        private static HMMoves ResolveRequiredHm(
        SquareDomain? currentSquare,
        SquareDomain targetSquare,
        bool isSurfing)
        {
            if (targetSquare.TileType == TileType.Water)
            {
                return isSurfing
                    ? HMMoves.Waterfall
                    : HMMoves.Surf;
            }

            if (targetSquare.TileType == TileType.Cave ||
                currentSquare?.TileType == TileType.Cave)
            {
                return HMMoves.Strength;
            }

            return HMMoves.Cut;
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

            // initialise all squares to None
            for (int sr = 0; sr < rows; sr++)
                for (int sc = 0; sc < cols; sc++)
                    grid[sr, sc] = new SquareDomain
                    {
                        Row = sr,
                        Col = sc,
                        SquareType = CollisionType.None,
                        TileType = TileType.Ground,
                    };

            // paint collision objects — DB stores tile coords, convert to square coords
            foreach (var obj in map.CollisionObjects)
            {
                int startRow = obj.Y / tps;
                int startCol = obj.X / tps;
                int endRow = (obj.Y + obj.Height - 1) / tps;
                int endCol = (obj.X + obj.Width - 1) / tps;

                for (int sr = startRow; sr <= endRow; sr++)
                {
                    for (int sc = startCol; sc <= endCol; sc++)
                    {
                        if ((uint)sr >= (uint)rows || (uint)sc >= (uint)cols)
                            continue;

                        // Blocked always wins
                        if (grid[sr, sc].SquareType == CollisionType.Blocked)
                            continue;

                        if (obj.CollisionType == CollisionType.Blocked ||
                            grid[sr, sc].SquareType == CollisionType.None)
                        {
                            grid[sr, sc].SquareType = obj.CollisionType;
                            grid[sr, sc].TileType = CollisionToTileType(obj.CollisionType);
                        }
                    }
                }
            }

            return grid;
        }

        /// <summary>
        /// Expands every CollisionObject rectangle into a per-tile lookup array.
        /// </summary>
      
   

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
            var (dRow, dCol) = Delta(npc.Direction);
            if (dRow == 0 && dCol == 0) return;

            for (int step = 1; step <= npc.VisionRange; step++)
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
            for (int dr = -npc.VisionRange; dr <= npc.VisionRange; dr++)
                for (int dc = -npc.VisionRange; dc <= npc.VisionRange; dc++)
                {
                    if (Math.Max(Math.Abs(dr), Math.Abs(dc)) > npc.VisionRange) continue;
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