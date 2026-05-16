using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Npc;

namespace PokemonGame.Model.Model.Map
{
    public class NpcState
    {
        private MapDomain _map;
        private SquareMapState _squareMap;

        private Action<NpcObjectDomain>? _spottedHandler;

        private readonly HashSet<int> _engagedNpcs = new();
        private readonly HashSet<int> _alreadySpotted = new();

        private readonly Dictionary<int, NpcRuntimeState> _runtime = new();

        public NpcState(MapDomain map, SquareMapState squareMap)
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

        // ---------------------------------------------------------------
        // Main tick
        // ---------------------------------------------------------------
        public void Tick(int playerRow, int playerCol)
        {
            foreach (var npc in _map.Npc)
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
                            npc.DirectionB != FacingDirection.None)
                        {
                            TickTurning(npc, state);
                        }

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
            else
            {
                _alreadySpotted.Clear();
            }
        }

        // ---------------------------------------------------------------
        // Walking NPC
        // ---------------------------------------------------------------
        private void TickWalking(
            NpcObjectDomain npc,
            NpcRuntimeState state,
            int playerRow,
            int playerCol)
        {
            var (curRow, curCol) =
                _squareMap.TileToSquare(npc.Location.y, npc.Location.x);

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

            var (tileRow, tileCol) =
                _squareMap.SquareToTile(nextRow, nextCol);

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
            (nextRow, nextCol) =
                StepInDirection(curRow, curCol, state.Direction);

            if (IsSquareFree(nextRow, nextCol, playerRow, playerCol, npc))
                return true;

            FlipDirection(npc, state);

            state.StepsWalked = 0;

            (nextRow, nextCol) =
                StepInDirection(curRow, curCol, state.Direction);

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
            {
                return false;
            }

            if (squareRow == playerRow &&
                squareCol == playerCol)
            {
                return false;
            }

            var collision =
                _squareMap.GetCollision(squareRow, squareCol);

            if (collision != CollisionType.None &&
                collision != CollisionType.WildGrass)
            {
                return false;
            }

            return !_map.Npc.Any(other =>
            {
                if (ReferenceEquals(other, currentNpc))
                    return false;

                var (r, c) =
                    _squareMap.TileToSquare(
                        other.Location.y,
                        other.Location.x);

                return r == squareRow &&
                       c == squareCol;
            });
        }

        // ---------------------------------------------------------------
        // Turning NPC
        // ---------------------------------------------------------------
        private void TickTurning(
            NpcObjectDomain npc,
            NpcRuntimeState state)
        {
            state.StepsWalked++;

            if (state.StepsWalked >= npc.StepsPerLeg)
            {
                FlipDirection(npc, state);

                state.StepsWalked = 0;
            }

            npc.Direction = state.Direction;
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------
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

        private static void FlipDirection(
             NpcObjectDomain npc,
             NpcRuntimeState state)
        {
            if (npc.DirectionA == null ||
                npc.DirectionB == null)
            {
                return;
            }

            state.Direction =
                state.Direction == npc.DirectionA.Value
                    ? npc.DirectionB.Value
                    : npc.DirectionA.Value;
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
    }
}