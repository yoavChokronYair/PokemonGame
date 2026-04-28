using PokemonGame.Core.Model.Helper.MathHelper;
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
        // ── Fields ───────────────────────────────────────────────────────────

        private readonly MapDomain _map;
        private readonly SquareDomain[,] _squares;
        private readonly int[,] _visionLayer;

        // ── Construction ─────────────────────────────────────────────────────

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
            => (tileRow / 2, tileCol / 2);

        public (int tileRow, int tileCol) SquareToTile(int squareRow, int squareCol)
            => (squareRow * 2, squareCol * 2);

        // ── Square access ────────────────────────────────────────────────────

        public SquareDomain? GetSquare(int row, int col)
            => InBounds(row, col) ? _squares[row, col] : null;

        // ── Collision ────────────────────────────────────────────────────────

        public CollisionType GetCollision(int row, int col)
        {
            var square = GetSquare(row, col);
            if (square == null) return CollisionType.Unwalkable;
            if (HasStationaryBlockerAt(row, col)) return CollisionType.Unwalkable;
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

        public bool HmCheck(int row, int col)
        {
            if (GetCollision(row, col) != CollisionType.HM) return false;
            var required = HmForTileType(GetSquare(row, col)?.TileType ?? TileType.Normal);
            return required != HMMoves.None
                && PlayerDomain.Instance.Team.AnyPokemonKnows(required.ToString());
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

            // NPC dialogue
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

            // HM tile
            var square = GetSquare(targetRow, targetCol);
            if (square?.SquareType == CollisionType.HM)
            {
                var required = HmForTileType(square.TileType);

                if (required == HMMoves.None)
                    return new InspectResult { Type = InspectResultType.Nothing };

                if (!PlayerDomain.Instance.Team.AnyPokemonKnows(required.ToString()))
                    return new InspectResult
                    {
                        Type = InspectResultType.NeedHm,
                        Message = $"You need {required} to get past this.",
                    };

                ClearTile(targetRow, targetCol);
                return new InspectResult
                {
                    Type = InspectResultType.HmUsed,
                    Message = $"Used {required}!",
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

        // ── Private — NPC collision helpers ──────────────────────────────────

        private bool HasStationaryBlockerAt(int row, int col)
            => _map.Npc.Any(n =>
                n.MovementType != MovementType.Walking &&
                n.CollisionType == CollisionType.Unwalkable &&
                NpcSquare(n) == (row, col));

        private bool HasWalkingNpcAt(int row, int col)
            => _map.Npc.Any(n =>
                n.MovementType == MovementType.Walking &&
                n.CollisionType == CollisionType.Unwalkable &&
                NpcSquare(n) == (row, col));

        // ── Private — grid construction ───────────────────────────────────────

        private static SquareDomain[,] BuildSquareGrid(MapDomain map)
        {
            int rows = map.Height / 2;
            int cols = map.Width / 2;
            var grid = new SquareDomain[rows, cols];
            var tiles = BuildTileArray(map.BackgroundBlocks, map);

            for (int sr = 0; sr < rows; sr++)
                for (int sc = 0; sc < cols; sc++)
                {
                    int tr = sr * 2, tc = sc * 2;
                    int tl = tiles[tr, tc], t = tiles[tr, tc + 1];
                    int bl = tiles[tr + 1, tc], br = tiles[tr + 1, tc + 1];

                    grid[sr, sc] = new SquareDomain
                    {
                        Row = sr,
                        Col = sc,
                        TileTopLeft = tl,
                        TileTopRight = t,
                        TileBottomLeft = bl,
                        TileBottomRight = br,
                        SquareType = ResolveSquareType(tl, t, bl, br),
                        TileType = ResolveTileType(tl),
                    };
                }

            return grid;
        }

        private static int[,] BuildTileArray(List<TileDomain> blocks, MapDomain map)
        {
            var tiles = new int[map.Height, map.Width];
            for (int b = 0; b < blocks.Count; b++)
            {
                if (blocks[b] is { } tile)
                    tiles[b / map.Width, b % map.Width] = tile.Tileid;
            }
            return tiles;
        }

        // ── Private — tile classification ─────────────────────────────────────

        private static CollisionType ResolveSquareType(int tl, int tr, int bl, int br)
        {
            if (IsBlocked(tl) || IsBlocked(tr) || IsBlocked(bl) || IsBlocked(br)) return CollisionType.Blocked;
            if (IsJumpDown(tl)) return CollisionType.JumpDown;
            if (IsJumpUp(tl)) return CollisionType.JumpUp;
            if (IsJumpLeft(tl)) return CollisionType.JumpLeft;
            if (IsJumpRight(tl)) return CollisionType.JumpRight;
            if (IsWarp(tl)) return CollisionType.None;
            if (IsWater(tl)) return CollisionType.HM;
            if (IsGrass(tl)) return CollisionType.WildGrass;
            return CollisionType.None;
        }

        private static TileType ResolveTileType(int tl) => tl switch
        {
            _ when IsWater(tl) => TileType.Water,
            _ when IsGrass(tl) => TileType.TallGrass,
            _ when IsBranch(tl) => TileType.Branch,
            _ when IsRock(tl) => TileType.Rock,
            _ when IsStrength(tl) => TileType.StrengthAble,
            _ => TileType.Normal,
        };

        private static bool IsBlocked(int id) => id == 0;
        private static bool IsWarp(int id) => id == 60;
        private static bool IsJumpDown(int id) => id == 70;
        private static bool IsJumpUp(int id) => id == 71;
        private static bool IsJumpLeft(int id) => id == 72;
        private static bool IsJumpRight(int id) => id == 73;
        private static bool IsWater(int id) => id is >= 50 and <= 59;
        private static bool IsGrass(int id) => id is >= 40 and <= 49;
        private static bool IsBranch(int id) => id is >= 80 and <= 89;
        private static bool IsRock(int id) => id is >= 90 and <= 99;
        private static bool IsStrength(int id) => id is >= 100 and <= 109;

        private static HMMoves HmForTileType(TileType type) => type switch
        {
            TileType.Water => HMMoves.Surf,
            TileType.Branch => HMMoves.Cut,
            TileType.Rock => HMMoves.RockSmash,
            TileType.StrengthAble => HMMoves.Strength,
            _ => HMMoves.None,
        };

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