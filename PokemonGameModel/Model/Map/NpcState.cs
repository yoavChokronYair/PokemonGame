using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.Map
{
    public class NpcState
    {
        private MapDomain _map;
        private SquareMapState _squareMap;
        private Action<NpcObjectDomain>? _spottedHandler;
        private readonly HashSet<int> _engagedNpcs = new();
        private readonly HashSet<int> _alreadySpotted = new();

        public NpcState(MapDomain map, SquareMapState squareMap)
        {
            _map = map;
            _squareMap = squareMap;
        }

        public void OnMapChanged(MapDomain map, SquareMapState squareMap)
        {
            _map = map;
            _squareMap = squareMap;
            _engagedNpcs.Clear();   // ← add
            _alreadySpotted.Clear();
        }
        public void SetSpottedHandler(Action<NpcObjectDomain> handler)
            => _spottedHandler = handler;
        // ---------------------------------------------------------------
        // Main tick — called once per timer interval
        // ---------------------------------------------------------------
        public void Tick(int playerRow, int playerCol)
        {
            foreach (var npc in _map.Npc)
            {
                if (_engagedNpcs.Contains(npc.NpcInfo.Id)) continue;  // ← change from _alreadySpotted
                switch (npc.MovementType)
                {
                    case MovementType.Walking:
                        if (npc.StepsPerLeg > 0)
                            TickWalking(npc, playerRow, playerCol);
                        break;

                    case MovementType.Stationery:
                        if (npc.DirectionA != FacingDirection.None &&
                            npc.DirectionB != FacingDirection.None)
                            TickTurning(npc);
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
        private void TickWalking(NpcObjectDomain npc, int playerRow, int playerCol)
        {
            var (curRow, curCol) = _squareMap.TileToSquare(npc.Location.x, npc.Location.y);

            if (!TryGetFreeStep(npc, curRow, curCol, playerRow, playerCol,
                                out int nextRow, out int nextCol))
                return;

            var (tileRow, tileCol) = _squareMap.SquareToTile(nextRow, nextCol);
            npc.Location = (tileRow, tileCol);

            npc.StepsWalked++;
            if (npc.StepsWalked >= npc.StepsPerLeg)
            {
                FlipDirection(npc);
                npc.StepsWalked = 0;
            }
        }

        private bool TryGetFreeStep(NpcObjectDomain npc, int curRow, int curCol,
                                    int playerRow, int playerCol,
                                    out int nextRow, out int nextCol)
        {
            (nextRow, nextCol) = StepInDirection(curRow, curCol, npc.direction);

            if (IsSquareFree(nextRow, nextCol, playerRow, playerCol))
                return true;

            FlipDirection(npc);
            npc.StepsWalked = 0;
            (nextRow, nextCol) = StepInDirection(curRow, curCol, npc.direction);

            return IsSquareFree(nextRow, nextCol, playerRow, playerCol);
        }

        private bool IsSquareFree(int squareRow, int squareCol, int playerRow, int playerCol)
        {
            if ((uint)squareRow >= (uint)_squareMap.SquareRows ||
                (uint)squareCol >= (uint)_squareMap.SquareCols)
                return false;

            if (squareRow == playerRow && squareCol == playerCol)
                return false;

            var collision = _squareMap.GetCollision(squareRow, squareCol);
            if (collision != CollisionType.None && collision != CollisionType.WildGrass)
                return false;

            return !_map.Npc.Any(other =>
            {
                var (r, c) = _squareMap.TileToSquare(other.Location.x, other.Location.y);
                return r == squareRow && c == squareCol;
            });
        }

        // ---------------------------------------------------------------
        // Turning NPC (stationary, just rotates)
        // ---------------------------------------------------------------
        private void TickTurning(NpcObjectDomain npc)
        {
            npc.StepsWalked++;
            if (npc.StepsWalked >= npc.StepsPerLeg)
            {
                FlipDirection(npc);
                npc.StepsWalked = 0;
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------
        private static void FlipDirection(NpcObjectDomain npc)
            => npc.direction = npc.direction == npc.DirectionA
                ? npc.DirectionB
                : npc.DirectionA;

        private static (int row, int col) StepInDirection(int row, int col, FacingDirection dir)
        {
            var (dr, dc) = dir switch
            {
                FacingDirection.Up => (-1, 0),
                FacingDirection.Down => (1, 0),
                FacingDirection.Left => (0, -1),
                FacingDirection.Right => (0, 1),
                _ => (0, 0),
            };
            return (row + dr, col + dc);
        }
    }
}
