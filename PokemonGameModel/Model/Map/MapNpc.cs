using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Npc;

namespace PokemonGame.Model.Model.Map
{
    public sealed class NpcRuntimeState
    {
        public int StepsWalked { get; set; }
        public int AnimationTick { get; set; }
        public bool IsMoving { get; set; }
        public FacingDirection Direction { get; set; }
    }
    public class MapNpc
    {
        private MapDomain _map;
        private SquareMapState _squareMap;
        private Action<NpcObjectDomain>? _spottedHandler;
        private Action<NpcObjectDomain>? _interactHandler;

        private readonly HashSet<int> _engagedNpcs = new();
        private readonly HashSet<int> _alreadySpotted = new();
        private readonly Dictionary<int, NpcRuntimeState> _runtime = new();

        public MapNpc(MapDomain map, SquareMapState squareMap)
        {
            _map = map;
            _squareMap = squareMap;
            InitRuntime();
        }

        public void OnMapChanged(MapDomain map, SquareMapState squareMap)
        {
            _map = map;
            _squareMap = squareMap;

            _engagedNpcs.Clear();
            _alreadySpotted.Clear();

            InitRuntime();
        }

        public void SetSpottedHandler(Action<NpcObjectDomain> handler)
            => _spottedHandler = handler;

        public void SetInteractHandler(Action<NpcObjectDomain> handler)
            => _interactHandler = handler;

        private void InitRuntime()
        {
            _runtime.Clear();

            foreach (var npc in _map.Npc)
            {
                _runtime[npc.NpcInfo.Id] = new NpcRuntimeState
                {
                    Direction = npc.Direction,
                    StepsWalked = 0,
                    AnimationTick = 0,
                    IsMoving = false
                };
            }
        }

        public void OnNpcDialogueFinished(NpcObjectDomain npc)
        {
            if (npc.IsDisappearing)
            {
                _map.Npc.Remove(npc);
                _runtime.Remove(npc.NpcInfo.Id);
            }
        }

        public void Tick(int playerRow, int playerCol)
        {
            foreach (var npc in _map.Npc.ToList())
            {
                if (_engagedNpcs.Contains(npc.NpcInfo.Id))
                    continue;

                var state = GetState(npc);
                state.IsMoving = false;

                switch (npc.MovementType)
                {
                    case MovementType.Walking:
                        if (npc.StepsPerLeg > 0)
                            TickWalking(npc, state, playerRow, playerCol);
                        break;

                    case MovementType.Stationary:
                        if (npc.DirectionA != FacingDirection.None &&
                            npc.DirectionB != FacingDirection.None &&
                            npc.StepsPerLeg > 0)
                            TickTurning(npc, state);
                        break;
                }
            }

            _squareMap.RebuildVisionLayer();

            if (_squareMap.IsInNpcVision(playerRow, playerCol, out int spottedById))
            {
                if (!_alreadySpotted.Contains(spottedById))
                {
                    _alreadySpotted.Add(spottedById);
                    _engagedNpcs.Add(spottedById);

                    var spotter = _map.Npc.FirstOrDefault(n => n.NpcInfo.Id == spottedById);
                    if (spotter != null)
                        _spottedHandler?.Invoke(spotter);
                }
            }
        }

        public void TryInteract(int playerRow, int playerCol, FacingDirection facing)
        {
            var (targetRow, targetCol) = facing switch
            {
                FacingDirection.Up => (playerRow - 1, playerCol),
                FacingDirection.Down => (playerRow + 1, playerCol),
                FacingDirection.Left => (playerRow, playerCol - 1),
                FacingDirection.Right => (playerRow, playerCol + 1),
                _ => (playerRow, playerCol)
            };

            var npc = _map.Npc.FirstOrDefault(n =>
            {
                var (r, c) = _squareMap.TileToSquare(n.Location.y, n.Location.x);
                return r == targetRow && c == targetCol;
            });

            if (npc == null)
                return;

            if (npc.VisionRange > 0)
                return;

            if (_engagedNpcs.Contains(npc.NpcInfo.Id))
                return;

            var state = GetState(npc);
            state.Direction = OppositeFacing(facing);
            npc.Direction = state.Direction;

            _engagedNpcs.Add(npc.NpcInfo.Id);
            _interactHandler?.Invoke(npc);
        }

        private void TickWalking(
            NpcObjectDomain npc,
            NpcRuntimeState state,
            int playerRow,
            int playerCol)
        {
            var (curRow, curCol) = _squareMap.TileToSquare(npc.Location.y, npc.Location.x);

            if (!TryGetFreeStep(
                    npc,
                    state,
                    curRow,
                    curCol,
                    playerRow,
                    playerCol,
                    out int nextRow,
                    out int nextCol))
            {
                return;
            }

            var (tileRow, tileCol) = _squareMap.SquareToTile(nextRow, nextCol);

            npc.Location = (tileCol, tileRow);

            state.IsMoving = true;
            state.AnimationTick++;

            state.StepsWalked++;

            if (state.StepsWalked >= npc.StepsPerLeg)
            {
                FlipDirection(npc, state);
                state.StepsWalked = 0;
            }

            npc.Direction = state.Direction;
        }

        private bool TryGetFreeStep(
            NpcObjectDomain npc,
            NpcRuntimeState state,
            int curRow,
            int curCol,
            int playerRow,
            int playerCol,
            out int nextRow,
            out int nextCol)
        {
            (nextRow, nextCol) = StepInDirection(curRow, curCol, state.Direction);

            if (IsSquareFree(nextRow, nextCol, playerRow, playerCol, npc))
                return true;

            FlipDirection(npc, state);
            state.StepsWalked = 0;

            (nextRow, nextCol) = StepInDirection(curRow, curCol, state.Direction);

            return IsSquareFree(nextRow, nextCol, playerRow, playerCol, npc);
        }

        private bool IsSquareFree(
            int squareRow,
            int squareCol,
            int playerRow,
            int playerCol,
            NpcObjectDomain currentNpc)
        {
            if ((uint)squareRow >= (uint)_squareMap.SquareRows ||
                (uint)squareCol >= (uint)_squareMap.SquareCols)
                return false;

            if (squareRow == playerRow && squareCol == playerCol)
                return false;

            var collision = _squareMap.GetCollision(squareRow, squareCol);

            if (collision != CollisionType.None &&
                collision != CollisionType.WildGrass)
                return false;

            return !_map.Npc.Any(other =>
            {
                if (ReferenceEquals(other, currentNpc))
                    return false;

                var (r, c) = _squareMap.TileToSquare(other.Location.y, other.Location.x);
                return r == squareRow && c == squareCol;
            });
        }

        private void TickTurning(NpcObjectDomain npc, NpcRuntimeState state)
        {
            state.StepsWalked++;

            if (state.StepsWalked >= npc.StepsPerLeg)
            {
                FlipDirection(npc, state);
                state.StepsWalked = 0;
            }

            npc.Direction = state.Direction;
        }

        private NpcRuntimeState GetState(NpcObjectDomain npc)
        {
            if (_runtime.TryGetValue(npc.NpcInfo.Id, out var state))
                return state;

            state = new NpcRuntimeState
            {
                Direction = npc.Direction,
                StepsWalked = 0,
                AnimationTick = 0,
                IsMoving = false
            };

            _runtime[npc.NpcInfo.Id] = state;
            return state;
        }

        private static void FlipDirection(NpcObjectDomain npc, NpcRuntimeState state)
        {
            if (state.Direction == npc.DirectionA)
                state.Direction = npc.DirectionB ?? state.Direction;
            else
                state.Direction = npc.DirectionA ?? state.Direction;
        }

        private static (int row, int col) StepInDirection(
            int row,
            int col,
            FacingDirection dir)
        {
            return dir switch
            {
                FacingDirection.Up => (row - 1, col),
                FacingDirection.Down => (row + 1, col),
                FacingDirection.Left => (row, col - 1),
                FacingDirection.Right => (row, col + 1),
                _ => (row, col)
            };
        }

        private static FacingDirection OppositeFacing(FacingDirection dir)
        {
            return dir switch
            {
                FacingDirection.Up => FacingDirection.Down,
                FacingDirection.Down => FacingDirection.Up,
                FacingDirection.Left => FacingDirection.Right,
                FacingDirection.Right => FacingDirection.Left,
                _ => dir
            };
        }
    }
}