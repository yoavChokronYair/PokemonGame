using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Dialogue;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Map
{
    public class InspectResult
    {
        public InspectResultType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TargetRow { get; set; }
        public int TargetCol { get; set; }
        public DialogueSet? DialogueSet { get; set; }
        public string NpcName { get; set; } = string.Empty;  // ← add
    }

    public class MoveResult
    {
        public bool Success { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public CollisionType SquareType { get; set; }
        public bool WildEncounterTriggered { get; set; }
        public int SpottedByNpcId { get; set; }   // 0 = nobody sees the player
    }

    public class SquareMapState
    {
        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------

        private readonly MapDomain _activeMap;
        private readonly SquareDomain[,] _squares;
        private readonly int[,] _visionLayer;

        // -----------------------------------------------------------------------
        // Construction
        // -----------------------------------------------------------------------

        public SquareMapState(MapDomain map)
        {
            _activeMap = map;
            _squares = BuildSquareGrid(map);
            _visionLayer = new int[SquareRows, SquareCols];
            RebuildVisionLayer();
        }

        // -----------------------------------------------------------------------
        // Public dimensions
        // -----------------------------------------------------------------------

        public int SquareRows => _squares.GetLength(0);
        public int SquareCols => _squares.GetLength(1);

        // -----------------------------------------------------------------------
        // Coordinate helpers
        // -----------------------------------------------------------------------
            
        /// Tile-space → square-space (each square = 2×2 tiles).
        public (int row, int col) TileToSquare(int tileRow, int tileCol)
            => (tileRow / 2, tileCol / 2);

        /// Square-space → top-left tile position.
        public (int tileRow, int tileCol) SquareToTile(int squareRow, int squareCol)
            => (squareRow * 2, squareCol * 2);

        // -----------------------------------------------------------------------
        // Square access
        // -----------------------------------------------------------------------

        public SquareDomain? GetSquare(int row, int col)
        {
            if ((uint)row >= (uint)SquareRows || (uint)col >= (uint)SquareCols)
                return null;
            return _squares[row, col];
        }

        // -----------------------------------------------------------------------
        // Collision
        // -----------------------------------------------------------------------

        public CollisionType GetCollision(int squareRow, int squareCol)
        {   
            var square = GetSquare(squareRow, squareCol);
            if (square == null) return CollisionType.Unwalkable;

            // Stationary unwalkable NPCs (items, signs) block their tile.
            // Walking NPCs are checked separately in IsSquareFreeForNpc.
            if (HasStationaryBlockerAt(squareRow, squareCol))
                return CollisionType.Unwalkable;

            return square.SquareType;
        }

        public bool CanMoveTo(int squareRow, int squareCol, FacingDirection direction)
        {
            // Block if any unwalkable NPC is standing here
            if (HasStationaryBlockerAt(squareRow, squareCol) || HasWalkingNpcAt(squareRow, squareCol))
                return false;

            return GetCollision(squareRow, squareCol) switch
            {
                CollisionType.None      => true,
                CollisionType.WildGrass => true,
                CollisionType.JumpLeft  => direction == FacingDirection.Left,
                CollisionType.JumpRight => direction == FacingDirection.Right,
                CollisionType.JumpDown  => direction == FacingDirection.Down,
                CollisionType.JumpUp    => direction == FacingDirection.Up,
                _                       => false,
            };
        }

        private bool HasWalkingNpcAt(int squareRow, int squareCol)
        {
            return _activeMap.Npc.Any(npc =>
            {
                if (npc.MovementType  != MovementType.Walking)    return false;
                if (npc.CollisionType != CollisionType.Unwalkable) return false;
                var (r, c) = TileToSquare(npc.Location.x, npc.Location.y);
                return r == squareRow && c == squareCol;
            });
        }

        public bool JumpCheck(int squareRow, int squareCol, FacingDirection direction)
        {
            return GetCollision(squareRow, squareCol) switch
            {
                CollisionType.JumpLeft => direction == FacingDirection.Left,
                CollisionType.JumpRight => direction == FacingDirection.Right,
                CollisionType.JumpDown => direction == FacingDirection.Down,
                CollisionType.JumpUp => direction == FacingDirection.Up,
                _ => false,
            };
        }

        public bool HmCheck(int squareRow, int squareCol)
        {
            if (GetCollision(squareRow, squareCol) != CollisionType.HM) return false;

            var square = GetSquare(squareRow, squareCol);
            if (square == null) return false;

            var required = HmForTileType(square.TileType);
            return required != HMMoves.None
                && PlayerDomain.Instance.Team.AnyPokemonKnows(required.ToString());
        }

        public bool WildCheck(int squareRow, int squareCol)
        {
            var square = GetSquare(squareRow, squareCol);
            if (square == null || square.SquareType != CollisionType.WildGrass) return false;
            return RNGHelper.TryWildEncounter(10);
        }

        // -----------------------------------------------------------------------
        // Player movement
        // -----------------------------------------------------------------------

        public MoveResult TryMove(int fromRow, int fromCol, FacingDirection direction)
        {
            var (toRow, toCol) = StepInDirection(fromRow, fromCol, direction);

            if (!CanMoveTo(toRow, toCol, direction))
                return new MoveResult { Success = false, Row = fromRow, Col = fromCol };

            // Vision is rebuilt by TickNpcs each frame; rebuild here too so
            // SpottedByNpcId is always valid even when called outside a tick.
            RebuildVisionLayer();
            IsInNpcVision(toRow, toCol, out int spottedBy);

            var landing = GetSquare(toRow, toCol)!;
            return new MoveResult
            {
                Success = true,
                Row = toRow,
                Col = toCol,
                SquareType = landing.SquareType,
                WildEncounterTriggered = WildCheck(toRow, toCol),
                SpottedByNpcId = spottedBy,
            };
        }

        // -----------------------------------------------------------------------
        // Inspect (items + HM tiles)
        // -----------------------------------------------------------------------

        public InspectResult TryInspect(int fromRow, int fromCol, FacingDirection facing)
        {
            var (targetRow, targetCol) = StepInDirection(fromRow, fromCol, facing);

            // ── NPC dialogue ────────────────────────────────────────────────────
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
            // ── HM tile ─────────────────────────────────────────────────────────
            var square = GetSquare(targetRow, targetCol);
            if (square != null && square.SquareType == CollisionType.HM)
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

        // Add this helper alongside GetPickupAt:
        public NpcObjectDomain? GetNpcAt(int squareRow, int squareCol)
        {
            return _activeMap.Npc.FirstOrDefault(npc =>
            {
                var (r, c) = TileToSquare(npc.Location.x, npc.Location.y);
                return r == squareRow && c == squareCol;
            });
        }

        public void ClearTile(int squareRow, int squareCol)
        {
            var square = GetSquare(squareRow, squareCol);
            if (square != null) square.SquareType = CollisionType.None;
        }

        // -----------------------------------------------------------------------
        // NPC queries
        // -----------------------------------------------------------------------

        /// Pickup item at a square (for inspect — separate from collision concern).
        public NpcObjectDomain? GetPickupAt(int squareRow, int squareCol)
        {
            return _activeMap.Npc.FirstOrDefault(npc =>
            {
                var (r, c) = TileToSquare(npc.Location.x, npc.Location.y);
                return r == squareRow && c == squareCol;
            });
        }

        // -----------------------------------------------------------------------
        // Vision layer
        // -----------------------------------------------------------------------

        public int[,] VisionLayer => _visionLayer;

        public bool IsInNpcVision(int squareRow, int squareCol, out int npcId)
        {
            npcId = ((uint)squareRow < (uint)SquareRows && (uint)squareCol < (uint)SquareCols)
                ? _visionLayer[squareRow, squareCol]
                : 0;
            return npcId != 0;
        }

        public void RebuildVisionLayer()
        {
            Array.Clear(_visionLayer, 0, _visionLayer.Length);

            foreach (var npc in _activeMap.Npc)
            {
                if (npc.visionRange <= 0) continue;
                var (npcRow, npcCol) = TileToSquare(npc.Location.x, npc.Location.y);

                switch (npc.VisionType)
                {
                    case VisionType.Normal:
                        PaintLineVision(npc, npcRow, npcCol);
                        break;
                    case VisionType.circular:
                        PaintCircularVision(npc, npcRow, npcCol);
                        break;
                }
            }
        }

        // -----------------------------------------------------------------------
        // Private — vision painting
        // -----------------------------------------------------------------------

        /// Single-tile-wide ray in the NPC's facing direction; stops at solid tiles.
        private void PaintLineVision(NpcObjectDomain npc, int npcRow, int npcCol)
        {
            var (dRow, dCol) = DirectionDelta(npc.direction);
            if (dRow == 0 && dCol == 0) return;

            for (int step = 1; step <= npc.visionRange; step++)
            {
                int r = npcRow + dRow * step;
                int c = npcCol + dCol * step;

                if ((uint)r >= (uint)SquareRows || (uint)c >= (uint)SquareCols) break;

                var collision = GetCollision(r, c);

                // Paint this tile first, then check if it blocks further sight
                _visionLayer[r, c] = npc.NpcInfo.Id;

                if (collision != CollisionType.None && collision != CollisionType.WildGrass)
                    break;  // solid tile — paint it but don't see through it
            }
        }

        /// Chebyshev (square) radius — swap Math.Max for Manhattan if you prefer a diamond.
        private void PaintCircularVision(NpcObjectDomain npc, int npcRow, int npcCol)
        {
            for (int dr = -npc.visionRange; dr <= npc.visionRange; dr++)
                for (int dc = -npc.visionRange; dc <= npc.visionRange; dc++)
                {
                    if (Math.Max(Math.Abs(dr), Math.Abs(dc)) > npc.visionRange) continue;

                    int r = npcRow + dr;
                    int c = npcCol + dc;

                    if ((uint)r >= (uint)SquareRows || (uint)c >= (uint)SquareCols) continue;

                    if (HasLineOfSight(npcRow, npcCol, r, c))
                        _visionLayer[r, c] = npc.NpcInfo.Id;
                }
        }

        private bool HasLineOfSight(int fromRow, int fromCol, int toRow, int toCol)
        {
            int dr = toRow - fromRow;
            int dc = toCol - fromCol;
            int steps = Math.Max(Math.Abs(dr), Math.Abs(dc));
            if (steps == 0) return true;

            for (int i = 1; i < steps; i++)   // < steps: don't check the destination itselfed
            {
                int r = fromRow + (int)Math.Round((double)dr * i / steps);
                int c = fromCol + (int)Math.Round((double)dc * i / steps);
                
                var collision = GetCollision(r, c);
                if (collision != CollisionType.None && collision != CollisionType.WildGrass)
                    return false;
            }
            return true;
        }

        // -----------------------------------------------------------------------
        // Private — collision helpers
        // -----------------------------------------------------------------------

        private bool HasStationaryBlockerAt(int squareRow, int squareCol)
        {
            return _activeMap.Npc.Any(npc =>
            {
                if (npc.MovementType == MovementType.Walking) return false;
                if (npc.CollisionType != CollisionType.Unwalkable) return false;
                var (r, c) = TileToSquare(npc.Location.x, npc.Location.y);
                return r == squareRow && c == squareCol;
            });
        }

        // -----------------------------------------------------------------------
        // Private — static helpers
        // -----------------------------------------------------------------------

        private static (int row, int col) StepInDirection(int row, int col, FacingDirection dir)
        {
            var (dr, dc) = DirectionDelta(dir);
            return (row + dr, col + dc);
        }

        private static (int dRow, int dCol) DirectionDelta(FacingDirection dir) => dir switch
        {
            FacingDirection.Up => (-1, 0),
            FacingDirection.Down => (1, 0),
            FacingDirection.Left => (0, -1),
            FacingDirection.Right => (0, 1),
            _ => (0, 0),
        };

        private static HMMoves HmForTileType(TileType tileType) => tileType switch
        {
            TileType.Water => HMMoves.Surf,
            TileType.Branch => HMMoves.Cut,
            TileType.Rock => HMMoves.RockSmash,
            TileType.StrengthAble => HMMoves.Strength,
            _ => HMMoves.None,
        };

        // -----------------------------------------------------------------------
        // Private — grid construction
        // -----------------------------------------------------------------------

        private static SquareDomain[,] BuildSquareGrid(MapDomain map)
        {
            int squareRows = map.Height / 2;
            int squareCols = map.Width / 2;

            var grid = new SquareDomain[squareRows, squareCols];
            var tiles = BuildTileArray(map.BackgroundBlocks, map);

            for (int sr = 0; sr < squareRows; sr++)
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
                var tile = blocks[b];
                if (tile is null) continue;
                tiles[b / map.Width, b % map.Width] = tile.Tileid;
            }
            return tiles;
        }

        // -----------------------------------------------------------------------
        // Private — tile classification
        // -----------------------------------------------------------------------

        private static CollisionType ResolveSquareType(int tl, int tr, int bl, int br)
        {
            if (IsBlocked(tl) || IsBlocked(tr) || IsBlocked(bl) || IsBlocked(br))
                return CollisionType.Blocked;
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
        private static bool IsWater(int id) => id >= 50 && id <= 59;
        private static bool IsGrass(int id) => id >= 40 && id <= 49;
        private static bool IsBranch(int id) => id >= 80 && id <= 89;
        private static bool IsRock(int id) => id >= 90 && id <= 99;
        private static bool IsStrength(int id) => id >= 100 && id <= 109;
    }
}