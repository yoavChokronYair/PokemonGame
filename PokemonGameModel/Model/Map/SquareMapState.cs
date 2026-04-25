using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Map
{
    //TODO: change the percent of wild encounter and add more factors to it,and id handle it in a more appropriate place
    public class InspectResult
    {
        public InspectResultType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TargetRow { get; set; }
        public int TargetCol { get; set; }
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

    public class SquareMapState
    {
        private SquareDomain[,] _squares;
        private MapDomain _activeMap;
        private int[,] _visionLayer;


        // Square-space dimensions
        public int SquareRows => _squares.GetLength(0);
        public int SquareCols => _squares.GetLength(1);

        public SquareMapState(MapDomain map)
        {
            _activeMap = map;
            _squares = BuildSquareGrid(map);
            _visionLayer = new int[SquareRows, SquareCols];
            RebuildVisionLayer();
        }
        // ── Vision layer ────────────────────────────────────────────────────────────

        public int[,] VisionLayer => _visionLayer;

        public void RebuildVisionLayer()
        {
            // Clear
            Array.Clear(_visionLayer, 0, _visionLayer.Length);

            foreach (var npc in _activeMap.Npc)
            {
                if (npc.visionRange <= 0) continue;

                var (npcRow, npcCol) = TileToSquare(npc.Location.x, npc.Location.y);

                switch (npc.VisionType)
                {
                    case VisionType.Normal:
                        PaintConeVision(npc, npcRow, npcCol);
                        break;
                    case VisionType.circular:
                        PaintCircularVision(npc, npcRow, npcCol);
                        break;
                }
            }
        }

        /// <summary>
        /// Paints a straight-line cone in the NPC's facing direction,
        /// stopping at the first unwalkable/blocked square (mirrors real Pokémon trainer sight).
        /// </summary>
        private void PaintConeVision(NpcObjectDomain npc, int npcRow, int npcCol)
        {
            var (dRow, dCol) = npc.direction switch
            {
                FacingDirection.Up => (-1, 0),
                FacingDirection.Down => (1, 0),
                FacingDirection.Left => (0, -1),
                FacingDirection.Right => (0, 1),
                _ => (0, 0)
            };

            if (dRow == 0 && dCol == 0) return;

            for (int step = 1; step <= npc.visionRange; step++)
            {
                int r = npcRow + dRow * step;
                int c = npcCol + dCol * step;

                if ((uint)r >= (uint)SquareRows || (uint)c >= (uint)SquareCols) break;

                var collision = GetCollision(r, c);

                // Vision is blocked by solid tiles — paint then stop
                if (collision == CollisionType.Unwalkable ||
                    collision == CollisionType.Blocked ||
                    collision == CollisionType.HM)
                {
                    break;   // wall blocks sight entirely
                }

                _visionLayer[r, c] = npc.NpcInfo.Id;   // ← NpcDomain needs an int Id field (see note)
            }
        }

        /// <summary>
        /// Paints a filled circle (Manhattan or Chebyshev — pick whichever feels right).
        /// Uses Chebyshev distance so diagonals count equally.
        /// </summary>
        private void PaintCircularVision(NpcObjectDomain npc, int npcRow, int npcCol)
        {
            for (int dr = -npc.visionRange; dr <= npc.visionRange; dr++)
            {
                for (int dc = -npc.visionRange; dc <= npc.visionRange; dc++)
                {
                    // Chebyshev: max(|dr|,|dc|) ≤ range
                    if (Math.Max(Math.Abs(dr), Math.Abs(dc)) > npc.visionRange) continue;

                    int r = npcRow + dr;
                    int c = npcCol + dc;

                    if ((uint)r >= (uint)SquareRows || (uint)c >= (uint)SquareCols) continue;

                    _visionLayer[r, c] = npc.NpcInfo.Id;
                }
            }
        }

        // ── Helper: check if a square is in an NPC's vision ─────────────────────────

        public bool IsInNpcVision(int squareRow, int squareCol, out int npcId)
        {
            npcId = _visionLayer[squareRow, squareCol];
            return npcId != 0;
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

            // Hidden item on this square overrides tile collision
            var item = GetHiddenItemAt(squareRow, squareCol);
            if (item != null && item.CollisionType == CollisionType.Unwalkable)
                return CollisionType.Unwalkable;

            return square.SquareType;
        }

        // Returns the hidden item at a square if it exists and is still visible
        public NpcObjectDomain? GetHiddenItemAt(int squareRow, int squareCol)
        {
            return _activeMap.Npc.FirstOrDefault(h =>
            {
                var (itemSquareRow, itemSquareCol) = TileToSquare(h.Location.x, h.Location.y);
                return itemSquareRow == squareRow &&
                       itemSquareCol == squareCol
                       ;
            });
        }

        // Called when player presses inspect — picks up item in the faced direction
        public InspectResult TryInspect(int fromSquareRow, int fromSquareCol, FacingDirection facing)
        {
            var (targetRow, targetCol) = facing switch
            {
                FacingDirection.Up => (fromSquareRow - 1, fromSquareCol),
                FacingDirection.Down => (fromSquareRow + 1, fromSquareCol),
                FacingDirection.Left => (fromSquareRow, fromSquareCol - 1),
                FacingDirection.Right => (fromSquareRow, fromSquareCol + 1),
                _ => (fromSquareRow, fromSquareCol)
            };

            // ── Hidden item ──────────────────────────────────────────────────────
            var item = GetHiddenItemAt(targetRow, targetCol);
            if (item != null)
            {
                item.IsDisappearing = true;
                return new InspectResult
                {
                    Type = InspectResultType.ItemPickup,
                    Message = $"Found {item.NpcInfo.Name}!",
                    TargetRow = targetRow,
                    TargetCol = targetCol,
                };
            }

            // ── HM tile ──────────────────────────────────────────────────────────
            var square = GetSquare(targetRow, targetCol);
            if (square != null && square.SquareType == CollisionType.HM)
            {
                HMMoves required = ResolveHmMove(targetRow, targetCol);

                if (required == HMMoves.None)
                    return new InspectResult { Type = InspectResultType.Nothing };

                if (!PlayerDomain.Instance.Team.AnyPokemonKnows(required.ToString()))
                    return new InspectResult
                    {
                        Type = InspectResultType.NeedHm,
                        Message = $"You need {required} to get past this.",
                    };

                // Has the move — clear the tile
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
        public void ClearTile(int squareRow, int squareCol)
        {
            var square = GetSquare(squareRow, squareCol);
            if (square != null)
                square.SquareType = CollisionType.None;
        }   
        public bool WildCheck(int squareRow, int squareCol)
        {
            var square = GetSquare(squareRow, squareCol);
            if (square == null || square.SquareType != CollisionType.WildGrass)
                return false;
            return RNGHelper.TryWildEncounter(10); // example 10% encounter rate; replace with your actual logic
        }


        public bool HmCheck(int squareRow, int squareCol)
        {
            if (GetCollision(squareRow, squareCol) != CollisionType.HM) return false;

            var square = GetSquare(squareRow, squareCol);
            if (square == null) return false;

            var required = RequiredHmForTile(square.TileType);
            if (required == HMMoves.None) return false;

            return PlayerDomain.Instance.Team.AnyPokemonKnows(required.ToString());
        }

        private HMMoves ResolveHmMove(int squareRow, int squareCol)
        {
            var square = GetSquare(squareRow, squareCol);
            if (square == null) return HMMoves.None;

            return RequiredHmForTile(square.TileType);
        }
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
                CollisionType.HM => false,   // always blocked — handled via prompt path
                CollisionType.JumpLeft => direction == FacingDirection.Left,
                CollisionType.JumpRight => direction == FacingDirection.Right,
                CollisionType.JumpDown => direction == FacingDirection.Down,
                CollisionType.JumpUp => direction == FacingDirection.Up,
                CollisionType.Unwalkable => false,
                CollisionType.Blocked => false,
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

            RebuildVisionLayer();   // ← rebuild every move

            var landing = GetSquare(toRow, toCol);
            return new MoveResult
            {
                Success = true,
                Row = toRow,
                Col = toCol,
                SquareType = landing.SquareType,
                WildEncounterTriggered = WildCheck(toRow, toCol)
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
                        TileType = ResolveTileType(tl),   // ← new
                    };
                }
            }

            return grid;
        }
        private static HMMoves RequiredHmForTile(TileType tileType) => tileType switch
        {
            TileType.Water => HMMoves.Surf,
            TileType.Branch => HMMoves.Cut,
            TileType.Rock => HMMoves.RockSmash,
            TileType.StrengthAble => HMMoves.Strength,
            _ => HMMoves.None
        };

        /// Decides the square's type from its 4 tile IDs.
        /// Blocked wins over everything; otherwise top-left tile decides.
        /// 
        private static bool IsWarp(int id) => id == 60;
        // in ResolveSquareType
        private static bool IsJumpDown(int id) => id == 70;
        private static bool IsJumpUp(int id) => id == 71;
        private static bool IsJumpLeft(int id) => id == 72;
        private static bool IsJumpRight(int id) => id == 73;

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
            _ => TileType.Normal
        };

        // Add tile ID ranges to match your tileset
        private static bool IsBranch(int id) => id >= 80 && id <= 89;
        private static bool IsRock(int id) => id >= 90 && id <= 99;
        private static bool IsStrength(int id) => id >= 100 && id <= 109;

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
