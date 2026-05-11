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

    public MapDomain ActiveMap => _player.trainerMapLocDomain.CurrentMap;
    public SquareMapState SquareMap => _squareMapState;

    public event Action<NpcObjectDomain>? TrainerSpotted;
    public event Action<NpcObjectDomain>? NpcInteracted;

    public MapManager(PlayerDomain player)
    {
        _player = player;
        LoadMap(player.trainerMapLocDomain.CurrentMap);
    }

    public void LoadMap(MapDomain map)
    {
        _player.trainerMapLocDomain.LastMapVisited = _player.trainerMapLocDomain.CurrentMap;
        _player.trainerMapLocDomain.CurrentMap = map;
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
            _player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x);
        
        _npcState.Tick(playerRow, playerCol);
    }

    public void TryInteractWithNpc()
    {
        var (squareRow, squareCol) = _squareMapState.TileToSquare(
            _player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x);

        _npcState.TryInteract(squareRow, squareCol, _player.trainerMapLocDomain.FacingDirection);
    }

    public void OnNpcDialogueFinished(NpcObjectDomain npc)
    {
        _npcState.OnNpcDialogueFinished(npc);
    }

    public InspectResult TryInspect()
    {
        var (squareRow, squareCol) = _squareMapState.TileToSquare(
            _player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x);

        return _squareMapState.TryInspect(squareRow, squareCol, _player.trainerMapLocDomain.FacingDirection);
    }

    public MoveResult TryMove(FacingDirection direction)
    {
        _player.trainerMapLocDomain.FacingDirection = direction;

        var (squareRow, squareCol) = _squareMapState.TileToSquare(
            _player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x);

        int toRow = squareRow, toCol = squareCol;
        switch (direction)
        {
            case FacingDirection.Up: toRow--; break;
            case FacingDirection.Down: toRow++; break;
            case FacingDirection.Left: toCol--; break;
            case FacingDirection.Right: toCol++; break;
        }

        // ── Out of bounds → connection check ──────────────────────────────────
        bool outOfBounds =
            toRow < 0 || toRow >= _squareMapState.SquareRows ||
            toCol < 0 || toCol >= _squareMapState.SquareCols;

        if (outOfBounds)
        {
            var connection = TryGetConnection(direction);
            if (connection != null && HandleConnection(connection, squareRow, squareCol, direction))
            {
                var (sr, sc) = _squareMapState.TileToSquare(
                    _player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x);
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
                _player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x);
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

            if (landing != null && _squareMapState.CanMoveTo(landRow, landCol, direction))
            {
                var (tileRow, tileCol) = _squareMapState.SquareToTile(landRow, landCol);
                _player.trainerMapLocDomain.playerLoc = (tileCol, tileRow);
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
        _player.trainerMapLocDomain.playerLoc = (ntileCol, ntileRow);

        return result;
    }

    // ── Viewport / collision / HM ─────────────────────────────────────────────
    public (int[,] background, int[,] foreground, int[,] vision, SpriteOverlay player) GetViewport()
        => _mapState.BuildViewPort(_player, _squareMapState);

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
        _player.trainerMapLocDomain.playerLoc = (tileCol, tileRow);
        _player.trainerMapLocDomain.FacingDirection = direction;
    }

    // ── Warp helpers ──────────────────────────────────────────────────────────
    private WrapDomain TryGetWarp(int squareRow, int squareCol)
        => ActiveMap.Wraps.FirstOrDefault(w =>
            w.WrapLoc.y == squareRow && w.WrapLoc.x == squareCol);

    private void HandleWarp(WrapDomain warp)
    {
        LoadMap(warp.TargetMap);
        var (tileRow, tileCol) = _squareMapState.SquareToTile(
            warp.SpawnLoc.row, warp.SpawnLoc.col);
        _player.trainerMapLocDomain.playerLoc = (tileCol, tileRow);
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

    private bool HandleConnection(ConnectedMapDomain connection, int squareRow, int squareCol, FacingDirection direction)
    {
        int tps = MapConstants.TilesPerSquare;
        int connSquareRows = connection.ConnectedMap.Height / tps;
        int connSquareCols = connection.ConnectedMap.Width / tps;
        int margin = connection.Margin;

        int newSquareRow, newSquareCol;
        switch (connection.ConnectionDirection)
        {
            case ConnectionDirection.North:
                newSquareRow = connSquareRows - 1;
                newSquareCol = squareCol - margin;
                break;
            case ConnectionDirection.South:
                newSquareRow = 0;
                newSquareCol = squareCol - margin;
                break;
            case ConnectionDirection.West:
                newSquareRow = squareRow - margin;
                newSquareCol = connSquareCols - 1;
                break;
            case ConnectionDirection.East:
                newSquareRow = squareRow - margin;
                newSquareCol = 0;
                break;
            default:
                newSquareRow = squareRow;
                newSquareCol = squareCol;
                break;
        }

        // Check walkable BEFORE loading the new map
        var tempSquareMap = new SquareMapState(connection.ConnectedMap);
        if (!tempSquareMap.CanMoveTo(newSquareRow, newSquareCol, direction))
            return false;

        LoadMap(connection.ConnectedMap);
        var (tileRow, tileCol) = _squareMapState.SquareToTile(newSquareRow, newSquareCol);
        _player.trainerMapLocDomain.playerLoc = (tileCol, tileRow);
        return true;
    }

    private (int row, int col) CurrentSquare()
        => _squareMapState.TileToSquare(_player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x);
}