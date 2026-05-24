using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Map
{
    public sealed class NpcRuntimeState
    {
        public int StepsWalked { get; set; }

        public int AnimationTick { get; set; }

        public bool IsMoving { get; set; }

        public FacingDirection Direction { get; set; }

        public FacingDirection OriginalDirection { get; set; }

        public bool HasTemporaryInteractionDirection { get; set; }

        public bool IsWalkingToPlayer { get; set; }

        public int TargetPlayerRow { get; set; }

        public int TargetPlayerCol { get; set; }
    }

    public class MapNpc
    {
        private MapDomain _map;
        private SquareMapState _squareMap;
        private readonly PlayerDomain _player;

        private Action<NpcObjectDomain>? _spottedHandler;
        private Action<NpcObjectDomain>? _interactHandler;

        private readonly HashSet<int> _engagedNpcs = new();
        private readonly HashSet<int> _alreadySpotted = new();
        private readonly Dictionary<int, NpcRuntimeState> _runtime = new();

        private readonly Random _random = new();

        public MapNpc(
            MapDomain map,
            SquareMapState squareMap,
            PlayerDomain player)
        {
            _map = map;
            _squareMap = squareMap;
            _player = player;

            InitRuntime();
        }

        public void OnMapChanged(
            MapDomain map,
            SquareMapState squareMap)
        {
            _map = map;
            _squareMap = squareMap;

            _engagedNpcs.Clear();
            _alreadySpotted.Clear();

            InitRuntime();
            MarkDefeatedTrainersAsAlreadySpotted();
        }

        public void SetSpottedHandler(Action<NpcObjectDomain> handler)
        {
            _spottedHandler = handler;
        }

        public void SetInteractHandler(Action<NpcObjectDomain> handler)
        {
            _interactHandler = handler;
        }

        private void InitRuntime()
        {
            _runtime.Clear();

            foreach (var npc in _map.Npc)
            {
                _runtime[npc.NpcInfo.Id] = new NpcRuntimeState
                {
                    Direction = npc.Direction,
                    OriginalDirection = npc.Direction,
                    StepsWalked = 0,
                    AnimationTick = 0,
                    IsMoving = false,
                    HasTemporaryInteractionDirection = false,
                    IsWalkingToPlayer = false
                };
            }

            MarkDefeatedTrainersAsAlreadySpotted();
        }

        private void MarkDefeatedTrainersAsAlreadySpotted()
        {
            foreach (var npc in _map.Npc)
            {
                int npcId = npc.NpcInfo.Id;

                if (_player.HasDefeatedTrainer(npcId))
                    _alreadySpotted.Add(npcId);
            }
        }

        public void OnNpcDialogueFinished(NpcObjectDomain npc)
        {
            int npcId = npc.NpcInfo.Id;

            if (npc.IsDisappearing)
            {
                _map.Npc.Remove(npc);
                _runtime.Remove(npcId);
                _engagedNpcs.Remove(npcId);
                _alreadySpotted.Remove(npcId);
                return;
            }

            if (_runtime.TryGetValue(npcId, out var state))
            {
                state.IsWalkingToPlayer = false;
                state.HasTemporaryInteractionDirection = false;
                state.Direction = state.OriginalDirection;
                npc.Direction = state.OriginalDirection;
            }

            _engagedNpcs.Remove(npcId);
        }

        public void Tick(
            int playerRow,
            int playerCol)
        {
            foreach (var npc in _map.Npc.ToList())
            {
                int npcId = npc.NpcInfo.Id;

                if (_player.HasDefeatedTrainer(npcId))
                {
                    _alreadySpotted.Add(npcId);
                    _engagedNpcs.Remove(npcId);
                    continue;
                }

                var state = GetState(npc);
                state.IsMoving = false;

                if (state.IsWalkingToPlayer)
                {
                    TickTrainerWalkToPlayer(npc, state, playerRow, playerCol);
                    continue;
                }

                if (_engagedNpcs.Contains(npcId))
                    continue;

                switch (npc.MovementType)
                {
                    case MovementType.Walking:
                        TickWalking(npc, state, playerRow, playerCol);
                        break;

                    case MovementType.Wander:
                        TickWander(npc, state, playerRow, playerCol);
                        break;


                    case MovementType.Random:
                        TickRandomTurning(npc, state);
                        break;

                    case MovementType.Stationary:
                        TickStationary(npc, state);
                        break;
                }
            }

            _squareMap.RebuildVisionLayer();

            if (_squareMap.IsInNpcVision(playerRow, playerCol, out int spottedById))
            {
                TryStartTrainerSpotted(spottedById, playerRow, playerCol);
            }
        }

        private void TryStartTrainerSpotted(
            int spottedById,
            int playerRow,
            int playerCol)
        {
            if (_alreadySpotted.Contains(spottedById))
                return;

            if (_player.HasDefeatedTrainer(spottedById))
            {
                _alreadySpotted.Add(spottedById);
                return;
            }

            var spotter =
                _map.Npc.FirstOrDefault(n => n.NpcInfo.Id == spottedById);

            if (spotter == null)
                return;

            var state = GetState(spotter);

            FaceNpcTowardPlayer(spotter, state, playerRow, playerCol);

            _alreadySpotted.Add(spottedById);
            _engagedNpcs.Add(spottedById);

            state.IsWalkingToPlayer = true;
            state.TargetPlayerRow = playerRow;
            state.TargetPlayerCol = playerCol;
        }

        public void TryInteract(
            int playerRow,
            int playerCol,
            FacingDirection facing)
        {
            var (targetRow, targetCol) =
                StepInDirection(playerRow, playerCol, facing);

            var npc = _map.Npc.FirstOrDefault(n =>
            {
                var (row, col) =
                    _squareMap.TileToSquare(n.Location.y, n.Location.x);

                return row == targetRow && col == targetCol;
            });

            if (npc == null)
                return;

            int npcId = npc.NpcInfo.Id;

            if (_engagedNpcs.Contains(npcId))
                return;

            var state = GetState(npc);

            state.OriginalDirection = npc.Direction;
            state.HasTemporaryInteractionDirection = true;
            state.Direction = OppositeFacing(facing);

            npc.Direction = state.Direction;

            _engagedNpcs.Add(npcId);
            _interactHandler?.Invoke(npc);
        }

        private void TickTrainerWalkToPlayer(
            NpcObjectDomain npc,
            NpcRuntimeState state,
            int playerRow,
            int playerCol)
        {
            var (npcRow, npcCol) =
                _squareMap.TileToSquare(npc.Location.y, npc.Location.x);

            int distance =
                Math.Abs(npcRow - playerRow) +
                Math.Abs(npcCol - playerCol);

            if (distance <= 1)
            {
                FaceNpcTowardPlayer(npc, state, playerRow, playerCol);
                state.IsWalkingToPlayer = false;
                _spottedHandler?.Invoke(npc);
                return;
            }

            FacingDirection stepDirection =
                PickDirectionTowardTarget(
                    npcRow,
                    npcCol,
                    playerRow,
                    playerCol);

            var (nextRow, nextCol) =
                StepInDirection(npcRow, npcCol, stepDirection);

            if (!IsSquareFree(nextRow, nextCol, playerRow, playerCol, npc))
            {
                state.IsWalkingToPlayer = false;
                _spottedHandler?.Invoke(npc);
                return;
            }

            MoveNpcToSquare(npc, state, nextRow, nextCol, stepDirection);
        }

        private void TickWalking(
            NpcObjectDomain npc,
            NpcRuntimeState state,
            int playerRow,
            int playerCol)
        {
            if (npc.StepsPerLeg <= 0)
                return;

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

            MoveNpcToSquare(
                npc,
                state,
                nextRow,
                nextCol,
                state.Direction);

            state.StepsWalked++;

            if (state.StepsWalked >= npc.StepsPerLeg)
            {
                FlipDirection(npc, state);
                state.StepsWalked = 0;
            }

            npc.Direction = state.Direction;
        }

        private void TickWander(
            NpcObjectDomain npc,
            NpcRuntimeState state,
            int playerRow,
            int playerCol)
        {
            int moveChance = 20;

            if (_random.Next(0, 100) >= moveChance)
                return;

            var directions = new[]
            {
                FacingDirection.Up,
                FacingDirection.Down,
                FacingDirection.Left,
                FacingDirection.Right
            }
            .OrderBy(_ => _random.Next())
            .ToList();

            var (curRow, curCol) =
                _squareMap.TileToSquare(npc.Location.y, npc.Location.x);

            foreach (var direction in directions)
            {
                var (nextRow, nextCol) =
                    StepInDirection(curRow, curCol, direction);

                if (!IsSquareFree(nextRow, nextCol, playerRow, playerCol, npc))
                    continue;

                MoveNpcToSquare(
                    npc,
                    state,
                    nextRow,
                    nextCol,
                    direction);

                return;
            }
        }

        private void TickRandomTurning(
            NpcObjectDomain npc,
            NpcRuntimeState state)
        {
            int turnChance = 10;

            if (_random.Next(0, 100) >= turnChance)
                return;

            var directions = new[]
            {
                FacingDirection.Up,
                FacingDirection.Down,
                FacingDirection.Left,
                FacingDirection.Right
            };

            state.Direction = directions[_random.Next(directions.Length)];
            npc.Direction = state.Direction;
        }

        private void TickStationary(
            NpcObjectDomain npc,
            NpcRuntimeState state)
        {
            if (npc.DirectionA != FacingDirection.None &&
                npc.DirectionB != FacingDirection.None &&
                npc.StepsPerLeg > 0)
            {
                TickTurning(npc, state);
            }
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

            if (squareRow == playerRow && squareCol == playerCol)
                return false;

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

                var (row, col) =
                    _squareMap.TileToSquare(
                        other.Location.y,
                        other.Location.x);

                return row == squareRow && col == squareCol;
            });
        }

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

        private void MoveNpcToSquare(
            NpcObjectDomain npc,
            NpcRuntimeState state,
            int row,
            int col,
            FacingDirection direction)
        {
            var (tileRow, tileCol) =
                _squareMap.SquareToTile(row, col);

            npc.Location = (tileCol, tileRow);

            state.Direction = direction;
            state.IsMoving = true;
            state.AnimationTick++;

            npc.Direction = direction;
        }

        private void FaceNpcTowardPlayer(
            NpcObjectDomain npc,
            NpcRuntimeState state,
            int playerRow,
            int playerCol)
        {
            var (npcRow, npcCol) =
                _squareMap.TileToSquare(npc.Location.y, npc.Location.x);

            FacingDirection direction =
                PickDirectionTowardTarget(
                    npcRow,
                    npcCol,
                    playerRow,
                    playerCol);

            state.Direction = direction;
            npc.Direction = direction;
        }

        private static FacingDirection PickDirectionTowardTarget(
            int fromRow,
            int fromCol,
            int targetRow,
            int targetCol)
        {
            int rowDistance = targetRow - fromRow;
            int colDistance = targetCol - fromCol;

            if (Math.Abs(rowDistance) >= Math.Abs(colDistance))
            {
                if (rowDistance < 0)
                    return FacingDirection.Up;

                if (rowDistance > 0)
                    return FacingDirection.Down;
            }

            if (colDistance < 0)
                return FacingDirection.Left;

            if (colDistance > 0)
                return FacingDirection.Right;

            return FacingDirection.None;
        }

        private NpcRuntimeState GetState(NpcObjectDomain npc)
        {
            int npcId = npc.NpcInfo.Id;

            if (_runtime.TryGetValue(npcId, out var state))
                return state;

            state = new NpcRuntimeState
            {
                Direction = npc.Direction,
                OriginalDirection = npc.Direction,
                StepsWalked = 0,
                AnimationTick = 0,
                IsMoving = false,
                HasTemporaryInteractionDirection = false,
                IsWalkingToPlayer = false
            };

            _runtime[npcId] = state;
            return state;
        }

        private static void FlipDirection(
            NpcObjectDomain npc,
            NpcRuntimeState state)
        {
            if (state.Direction == npc.DirectionA)
                state.Direction = npc.DirectionB ?? state.Direction;
            else
                state.Direction = npc.DirectionA ?? state.Direction;
        }

        private static (int row, int col) StepInDirection(
            int row,
            int col,
            FacingDirection direction)
        {
            return direction switch
            {
                FacingDirection.Up => (row - 1, col),
                FacingDirection.Down => (row + 1, col),
                FacingDirection.Left => (row, col - 1),
                FacingDirection.Right => (row, col + 1),
                _ => (row, col)
            };
        }

        private static FacingDirection OppositeFacing(
            FacingDirection direction)
        {
            return direction switch
            {
                FacingDirection.Up => FacingDirection.Down,
                FacingDirection.Down => FacingDirection.Up,
                FacingDirection.Left => FacingDirection.Right,
                FacingDirection.Right => FacingDirection.Left,
                _ => direction
            };
        }
    }
}