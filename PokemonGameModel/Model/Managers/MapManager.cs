using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Map;

public class MapManager
{
    // ── Coordinate convention (see MapDomain for full spec) ──────────────────
    //   Tile-space  : x = tileCol,   y = tileRow
    //   Square-space: row = squareRow, col = squareCol
    //   Margin      : square units
    // ────────────────────────────────────────────────────────────────────────

    private MapState _mapState;
    private SquareMapState _squareMapState;
    private MapNpc _npcState;
    private readonly PlayerDomain _player;

    public MapDomain ActiveMap => _player.CurrentMap;
    public SquareMapState SquareMap => _squareMapState;

    public event Action<NpcObjectDomain>? TrainerSpotted;
    public event Action<NpcObjectDomain>? NpcInteracted;

    public MapManager(PlayerDomain player)
    {
        _player = player;
        LoadMap(player.CurrentMap);
    }

    public void LoadMap(MapDomain map)
    {
        _player.LastMapVisited = _player.CurrentMap;
        _player.CurrentMap = map;
        _mapState = new MapState(map);
        _squareMapState = new SquareMapState(map);

        if (_npcState == null)
        {
            _npcState = new MapNpc(map, _squareMapState);
            _npcState.SetSpottedHandler(npc => TrainerSpotted?.Invoke(npc));
            _npcState.SetInteractHandler(npc => NpcInteracted?.Invoke(npc));
        }
        else
        {
            _npcState.OnMapChanged(map, _squareMapState);
        }
    }

    public void TickNpcs()
    {
        var (playerRow, playerCol) = _squareMapState.TileToSquare(
            _player.playerLoc.y, _player.playerLoc.x);   // y=tileRow, x=tileCol

        _npcState.Tick(playerRow, playerCol);
    }

    public void TryInteractWithNpc()
    {
        var (squareRow, squareCol) = _squareMapState.TileToSquare(
            _player.playerLoc.y, _player.playerLoc.x);   // y=tileRow, x=tileCol

        _npcState.TryInteract(squareRow, squareCol, _player.FacingDirection);
    }

    public void OnNpcDialogueFinished(NpcObjectDomain npc)
    {
        _npcState.OnNpcDialogueFinished(npc);
    }

    public InspectResult TryInspect()
    {
        var (squareRow, squareCol) = _squareMapState.TileToSquare(
            _player.playerLoc.y, _player.playerLoc.x);   // y=tileRow, x=tileCol

        return _squareMapState.TryInspect(squareRow, squareCol, _player.FacingDirection);
    }

    public MoveResult TryMove(FacingDirection direction)
    {
        _player.FacingDirection = direction;

        // playerLoc: x=tileCol, y=tileRow
        var (squareRow, squareCol) = _squareMapState.TileToSquare(
            _player.playerLoc.y, _player.playerLoc.x);

        int toRow = squareRow, toCol = squareCol;
        switch (direction)
        {
            case FacingDirection.Up: toRow--; break;
            case FacingDirection.Down: toRow++; break;
            case FacingDirection.Left: toCol--; break;
            case FacingDirection.Right: toCol++; break;
        }

        // ── Out of bounds → connection check ─────────────────────────────────
        bool outOfBounds =
            toRow < 0 || toRow >= _squareMapState.SquareRows ||
            toCol < 0 || toCol >= _squareMapState.SquareCols;

        if (outOfBounds)
        {
            var connection = TryGetConnection(direction);
            if (connection != null)
            {
                HandleConnection(connection, squareRow, squareCol);
                var (sr, sc) = _squareMapState.TileToSquare(
                    _player.playerLoc.y, _player.playerLoc.x);
                return new MoveResult { Success = true, Row = sr, Col = sc, SquareType = CollisionType.None };
            }
            return new MoveResult { Success = false, Row = squareRow, Col = squareCol };
        }

        // ── Warp check ────────────────────────────────────────────────────────
        var warp = TryGetWarp(toRow, toCol);
        if (warp != null)
        {
            HandleWarp(warp);
            var (sr, sc) = _squareMapState.TileToSquare(
                _player.playerLoc.y, _player.playerLoc.x);
            return new MoveResult { Success = true, Row = sr, Col = sc, SquareType = CollisionType.None };
        }

        // ── Normal collision + move ───────────────────────────────────────────
        var result = _squareMapState.TryMove(squareRow, squareCol, direction);
        if (!result.Success) return result;

        // ── Jump ─────────────────────────────────────────────────────────────
        var landedCollision = _squareMapState.GetCollision(result.Row, result.Col);
        if (landedCollision is CollisionType.JumpDown or CollisionType.JumpUp
                            or CollisionType.JumpLeft or CollisionType.JumpRight)
        {
            int landRow = result.Row, landCol = result.Col;
            switch (direction)
            {
                case FacingDirection.Up: landRow--; break;
                case FacingDirection.Down: landRow++; break;
                case FacingDirection.Left: landCol--; break;
                case FacingDirection.Right: landCol++; break;
            }

            var landing = _squareMapState.GetSquare(landRow, landCol);

            // ── Bug #1 fix ───────────────────────────────────────────────────
            // Only commit the jump if the landing square exists AND is walkable.
            // Old code committed to result.Row/Col (the ledge) when blocked,
            // putting the player inside the wall.
            if (landing != null && _squareMapState.CanMoveTo(landRow, landCol, direction))
            {
                var (tileRow, tileCol) = _squareMapState.SquareToTile(landRow, landCol);
                _player.playerLoc = (tileCol, tileRow);   // x=tileCol, y=tileRow
                return new MoveResult
                {
                    Success = true,
                    Row = landRow,
                    Col = landCol,
                    SquareType = landing.SquareType,
                    WildEncounterTriggered = _squareMapState.WildCheck(landRow, landCol)
                };
            }

            return new MoveResult { Success = false, Row = result.Row, Col = result.Col };
        }

        // ── Commit normal move ────────────────────────────────────────────────
        var (ntileRow, ntileCol) = _squareMapState.SquareToTile(result.Row, result.Col);
        _player.playerLoc = (ntileCol, ntileRow);   // x=tileCol, y=tileRow

        return result;
    }

<<<<<<< HEAD
    public (int[,] background, int[,] foreground, int[,] vision) GetViewport()
        => _mapState.BuildViewPort(_player, _squareMapState);
=======
    // ---------------------------------------------------------------
    // Viewport / collision / HM
    // ---------------------------------------------------------------
    public (int[,] background, int[,] foreground, int[,] vision, SpriteOverlay player) GetViewport()
            => _mapState.BuildViewPort(_player, _squareMapState);
>>>>>>> ffedec0895c70be5f6563ca012858e64cb30befe

    public CollisionType GetCollisionAt(int squareRow, int squareCol)
        => _squareMapState.GetCollision(squareRow, squareCol);

    public bool IsWildTile()
    {
        var (sr, sc) = CurrentSquare();
        return _squareMapState.WildCheck(sr, sc);
    }

    public void ConfirmHmUse(int squareRow, int squareCol, FacingDirection direction)
    {
        _squareMapState.ClearTile(squareRow, squareCol);
        var (tileRow, tileCol) = _squareMapState.SquareToTile(squareRow, squareCol);
        _player.playerLoc = (tileCol, tileRow);   // x=tileCol, y=tileRow
        _player.FacingDirection = direction;
    }

    // ── Warp helpers ──────────────────────────────────────────────────────────

    // ── Bug #1 fix: WrapLoc.x = squareCol, WrapLoc.y = squareRow ────────────
    // Previously compared WrapLoc.x to squareRow (transposed).
    private WrapDomain TryGetWarp(int squareRow, int squareCol)
        => ActiveMap.Wraps.FirstOrDefault(w =>
            w.WrapLoc.y == squareRow && w.WrapLoc.x == squareCol);

    // ── Bug #2 fix: SpawnLoc is already in square coords; SquareToTile ────────
    // converts it. Previously there was risk of double-conversion if callers
    // passed tile coords into SpawnLoc. Contract is now enforced in WrapDomain.
    private void HandleWarp(WrapDomain warp)
    {
        LoadMap(warp.TargetMap);
        // SpawnLoc is square-space → SquareToTile gives us tile coords
        var (tileRow, tileCol) = _squareMapState.SquareToTile(
            warp.SpawnLoc.row, warp.SpawnLoc.col);
        _player.playerLoc = (tileCol, tileRow);   // x=tileCol, y=tileRow
    }

    // ── Connection helpers ────────────────────────────────────────────────────

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

    // ── Bug #3 fix: Connection margin math wrong — off-by-2× and wrong sign ──
    //
    // Old code divided map.Height / 2 (tile units) to get a square index, and
    // divided Margin by 2 (but Margin is already in square units). Corrected:
    //   • Use SquareRows / SquareCols (not Height/Width) for the map dimensions
    //     in square space — fixes Bug #4 (Height/Width were tile units).
    //   • Margin is in square units; subtract directly without dividing by 2
    //     for the column/row offset. The /2 divide was wrong — the margin is an
    //     absolute alignment offset, not a half-offset to center. Removed.
    //
    // ── Bug #4 fix: ConnectedMap Height/Width used instead of square count ────
    // HandleConnection used `connection.ConnectedMap.Height / 2` to find the
    // "last square row of the neighbor". Corrected to use SquareCols/SquareRows
    // via a temporary SquareMapState, or simply derive from map dimensions:
    //   squareRows = map.Height / TilesPerSquare
    //   squareCols = map.Width  / TilesPerSquare
    private void HandleConnection(ConnectedMapDomain connection, int squareRow, int squareCol)
    {
        int tps = MapConstants.TilesPerSquare;
        int connSquareRows = connection.ConnectedMap.Height / tps;
        int connSquareCols = connection.ConnectedMap.Width / tps;
        int margin = connection.Margin; // already in square units

        int newSquareRow, newSquareCol;
        switch (connection.ConnectionDirection)
        {
            case ConnectionDirection.North:
                // Entering from the south edge of the connected (northern) map
                newSquareRow = connSquareRows - 1;
                newSquareCol = squareCol - margin;
                break;
            case ConnectionDirection.South:
                // Entering from the north edge of the connected (southern) map
                newSquareRow = 0;
                newSquareCol = squareCol - margin;
                break;
            case ConnectionDirection.West:
                // Entering from the east edge of the connected (western) map
                newSquareRow = squareRow - margin;
                newSquareCol = connSquareCols - 1;
                break;
            case ConnectionDirection.East:
                // Entering from the west edge of the connected (eastern) map
                newSquareRow = squareRow - margin;
                newSquareCol = 0;
                break;
            default:
                newSquareRow = squareRow;
                newSquareCol = squareCol;
                break;
        }

        LoadMap(connection.ConnectedMap);
        var (tileRow, tileCol) = _squareMapState.SquareToTile(newSquareRow, newSquareCol);
        _player.playerLoc = (tileCol, tileRow);   // x=tileCol, y=tileRow
    }

    private (int row, int col) CurrentSquare()
        => _squareMapState.TileToSquare(_player.playerLoc.y, _player.playerLoc.x);
}