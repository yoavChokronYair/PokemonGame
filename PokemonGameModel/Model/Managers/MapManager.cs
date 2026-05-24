using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Map;
namespace PokemonGame.Model.Model.Managers
{
    public partial class MapManager
    {
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
            _player.trainerMapLocDomain.LastMapVisited =
                _player.trainerMapLocDomain.CurrentMap;

            _player.trainerMapLocDomain.CurrentMap = map;

            _mapState = new MapState(map);
            _squareMapState = new SquareMapState(map);
            ApplyPersistentMapChanges();
            if (_npcState == null)
            {
                _npcState = new MapNpc(map, _squareMapState,_player);

                _npcState.SetSpottedHandler(
                    npc => TrainerSpotted?.Invoke(npc));

                _npcState.SetInteractHandler(
                    npc => NpcInteracted?.Invoke(npc));
            }
            else
            {
                _npcState.OnMapChanged(map, _squareMapState);
            }
        }
        public bool TryStopSurfing()
{
    if (!_player.trainerMapLocDomain.IsSurfing)
        return false;

    var (squareRow, squareCol) = CurrentSquare();

    if (_squareMapState.GetCollision(squareRow, squareCol) == CollisionType.HM)
        return false;

    _player.trainerMapLocDomain.IsSurfing = false;
    return true;
}
        private void ApplyPersistentMapChanges()
        {
            for (int row = 0; row < _squareMapState.SquareRows; row++)
            {
                for (int col = 0; col < _squareMapState.SquareCols; col++)
                {
                    string cutFlag =
                        BuildCutFlag(ActiveMap.Name, row, col);

                    if (_player.ProgressFlags.StoryFlags.Contains(cutFlag.GetHashCode()))
                    {
                        _squareMapState.ClearTile(row, col);
                    }
                }
            }
        }
        public void TickNpcs()
        {
            var (playerRow, playerCol) =
                _squareMapState.TileToSquare(
                    _player.trainerMapLocDomain.playerLoc.y,
                    _player.trainerMapLocDomain.playerLoc.x);

            _npcState.Tick(playerRow, playerCol);
        }

        public void TryInteractWithNpc()
        {
            var (squareRow, squareCol) =
                _squareMapState.TileToSquare(
                    _player.trainerMapLocDomain.playerLoc.y,
                    _player.trainerMapLocDomain.playerLoc.x);

            _npcState.TryInteract(
                squareRow,
                squareCol,
                _player.trainerMapLocDomain.FacingDirection);
        }

        public void OnNpcDialogueFinished(NpcObjectDomain npc)
        {
            _npcState.OnNpcDialogueFinished(npc);
        }

        public InspectResult TryInspect()
        {
            var (squareRow, squareCol) =
                _squareMapState.TileToSquare(
                    _player.trainerMapLocDomain.playerLoc.y,
                    _player.trainerMapLocDomain.playerLoc.x);

            return _squareMapState.TryInspect(
                squareRow,
                squareCol,
                _player.trainerMapLocDomain.FacingDirection);
        }

        public MoveResult TryMove(FacingDirection direction)
        {
            _player.trainerMapLocDomain.FacingDirection = direction;

            var (squareRow, squareCol) =
                _squareMapState.TileToSquare(
                    _player.trainerMapLocDomain.playerLoc.y,
                    _player.trainerMapLocDomain.playerLoc.x);

            int toRow = squareRow;
            int toCol = squareCol;

            switch (direction)
            {
                case FacingDirection.Up:
                    toRow--;
                    break;

                case FacingDirection.Down:
                    toRow++;
                    break;

                case FacingDirection.Left:
                    toCol--;
                    break;

                case FacingDirection.Right:
                    toCol++;
                    break;
            }

            bool outOfBounds =
                toRow < 0 ||
                toRow >= _squareMapState.SquareRows ||
                toCol < 0 ||
                toCol >= _squareMapState.SquareCols;

            if (outOfBounds)
            {
                var connection = TryGetConnection(direction);

                if (connection != null &&
                    HandleConnection(connection, squareRow, squareCol, direction))
                {
                    var (sr, sc) =
                        _squareMapState.TileToSquare(
                            _player.trainerMapLocDomain.playerLoc.y,
                            _player.trainerMapLocDomain.playerLoc.x);

                    return new MoveResult
                    {
                        Success = true,
                        Row = sr,
                        Col = sc,
                        SquareType = CollisionType.None
                    };
                }

                return new MoveResult
                {
                    Success = false,
                    Row = squareRow,
                    Col = squareCol
                };
            }

            var warp = TryGetWarp(toRow, toCol);

            if (warp != null)
            {
                HandleWarp(warp);

                var (sr, sc) =
                    _squareMapState.TileToSquare(
                        _player.trainerMapLocDomain.playerLoc.y,
                        _player.trainerMapLocDomain.playerLoc.x);

                return new MoveResult
                {
                    Success = true,
                    Row = sr,
                    Col = sc,
                    SquareType = CollisionType.None
                };
            }

            var result =
                _squareMapState.TryMove(squareRow, squareCol, direction);

            if (!result.Success)
                return result;

            var landedCollision =
                _squareMapState.GetCollision(result.Row, result.Col);

            if (landedCollision is CollisionType.JumpDown
                or CollisionType.JumpUp
                or CollisionType.JumpLeft
                or CollisionType.JumpRight)
            {
                int landRow = result.Row;
                int landCol = result.Col;

                switch (direction)
                {
                    case FacingDirection.Up:
                        landRow--;
                        break;

                    case FacingDirection.Down:
                        landRow++;
                        break;

                    case FacingDirection.Left:
                        landCol--;
                        break;

                    case FacingDirection.Right:
                        landCol++;
                        break;
                }

                var landing =
                    _squareMapState.GetSquare(landRow, landCol);

                if (landing != null &&
                    _squareMapState.CanMoveTo(landRow, landCol, direction))
                {
                    var (tileRow, tileCol) =
                        _squareMapState.SquareToTile(landRow, landCol);

                    _player.trainerMapLocDomain.playerLoc =
                        (tileCol, tileRow);

                    return new MoveResult
                    {
                        Success = true,
                        Row = landRow,
                        Col = landCol,
                        SquareType = landing.SquareType,
                        WildEncounterTriggered =
                            _squareMapState.WildCheck(landRow, landCol)
                    };
                }

                return new MoveResult
                {
                    Success = false,
                    Row = result.Row,
                    Col = result.Col
                };
            }

            var (ntileRow, ntileCol) =
                _squareMapState.SquareToTile(result.Row, result.Col);

            _player.trainerMapLocDomain.playerLoc =
                (ntileCol, ntileRow);
            TryStopSurfing();
            return result;
        }

        public (int[,] background,
                int[,] foreground,
                int[,] vision,
                List<SpriteOverlay> npcs,
                SpriteOverlay player) GetViewport()
        {
            return _mapState.BuildViewPort(_player, _squareMapState);
        }

        public void ConfirmCutUse(
            int squareRow,
            int squareCol,
            FacingDirection direction)
        {
            _squareMapState.ClearTile(squareRow, squareCol);

            string flag =
                BuildCutFlag(ActiveMap.Name, squareRow, squareCol);

            _player.ProgressFlags.StoryFlags.Add(flag.GetHashCode());

            _player.trainerMapLocDomain.FacingDirection = direction;
        }
        private static string BuildCutFlag(
            string mapName,
            int squareRow,
            int squareCol)
        {
            return $"CUT:{mapName}:{squareRow}:{squareCol}";
        }
        public void ConfirmSurfUse(
            int squareRow,
            int squareCol,
            FacingDirection direction)
        {
            var (tileRow, tileCol) =
                _squareMapState.SquareToTile(squareRow, squareCol);

            _player.trainerMapLocDomain.playerLoc =
                (tileCol, tileRow);

            _player.trainerMapLocDomain.FacingDirection =
                direction;

            _player.trainerMapLocDomain.IsSurfing =
                true;
        }

        private WrapDomain? TryGetWarp(int squareRow, int squareCol)
        {
            return ActiveMap.Wraps.FirstOrDefault(w =>
                w.WrapLoc.y == squareRow &&
                w.WrapLoc.x == squareCol);
        }

        private void HandleWarp(WrapDomain warp)
        {
            LoadMap(warp.TargetMap);

            var (tileRow, tileCol) =
                _squareMapState.SquareToTile(
                    warp.SpawnLoc.row,
                    warp.SpawnLoc.col);

            _player.trainerMapLocDomain.playerLoc =
                (tileCol, tileRow);
        }

        private ConnectedMapDomain? TryGetConnection(FacingDirection direction)
        {
            var connDir = direction switch
            {
                FacingDirection.Up => ConnectionDirection.North,
                FacingDirection.Down => ConnectionDirection.South,
                FacingDirection.Left => ConnectionDirection.West,
                FacingDirection.Right => ConnectionDirection.East,
                _ => (ConnectionDirection?)null
            };

            if (connDir == null)
                return null;

            return ActiveMap.ConnectedMaps
                .FirstOrDefault(c => c.ConnectionDirection == connDir.Value);
        }

        private bool HandleConnection(
            ConnectedMapDomain connection,
            int squareRow,
            int squareCol,
            FacingDirection direction)
        {
            int tps = MapConstants.TilesPerSquare;

            int connSquareRows =
                connection.ConnectedMap.Height / tps;

            int connSquareCols =
                connection.ConnectedMap.Width / tps;

            int margin = connection.Margin;

            int newSquareRow;
            int newSquareCol;

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

            var tempSquareMap =
                new SquareMapState(connection.ConnectedMap);

            if (!tempSquareMap.CanMoveTo(newSquareRow, newSquareCol, direction))
                return false;

            LoadMap(connection.ConnectedMap);

            var (tileRow, tileCol) =
                _squareMapState.SquareToTile(newSquareRow, newSquareCol);

            _player.trainerMapLocDomain.playerLoc =
                (tileCol, tileRow);

            return true;
        }

        private (int row, int col) CurrentSquare()
        {
            return _squareMapState.TileToSquare(
                _player.trainerMapLocDomain.playerLoc.y,
                _player.trainerMapLocDomain.playerLoc.x);
        }
    }

    public partial class MapManager
    {
        public WildPokemonDomain? GetWildEncounter()
        {
            var encounters = ActiveMap.Encounters;

            if (encounters == null ||
                encounters.Count == 0)
            {
                return null;
            }

            EncounterDomain? encounter =
                RNGHelper.PickWildEncounter(encounters);

            if (encounter == null)
                return null;

            return new WildPokemonDomain(encounter);
        }
    }
}
   