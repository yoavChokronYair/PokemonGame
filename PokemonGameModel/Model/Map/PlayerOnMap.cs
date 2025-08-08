using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Manager;

namespace PokemonGameModel.Model.Map
{
    public class PlayerOnMap
    {
        private MapData _currentMap;
        private int _currentLocation;
        public List<(TileTypeFirstLayer, TileTypeSecondLayer)> _currentMapTiles;
       

        public PlayerOnMap(MapData currentMap, int currentLocation)
        {
            _currentMap = currentMap;
            _currentLocation = currentLocation;
            _currentMapTiles = GameMap.Instance.mapTiles[_currentMap];
            SetPlayerTile(_currentLocation);
        }

        /// <summary>
        /// Checks if the player can move in the given direction offset.
        /// </summary>
        public bool CanMove(int offset)
        {
            int targetIndex = _currentLocation + offset;

            if (targetIndex == 0 || targetIndex >= _currentMapTiles.Count)
                return false; // Out of bounds

            return (int)_currentMapTiles[targetIndex].Item2 == 0; // 0 = None / OutOfBounds
        }

        /// <summary>
        /// Moves the player in the given direction offset.
        /// </summary>
        public void Move(int offset)
        {
            if (!CanMove(offset))
                return;

            RestoreOriginalTile(_currentLocation);
            _currentLocation += offset;
            SetPlayerTile(_currentLocation);
        }

        /// <summary>
        /// Move using X/Y deltas instead of index offsets.
        /// Automatically handles bounds and map switching.
        /// </summary>
        public void MoveByXY(int dx, int dy)
        {
            int width = _currentMap.Width;
            int height = _currentMap.Height;

            int playerX = _currentLocation % width;
            int playerY = _currentLocation / width;

            int newX = playerX + dx;
            int newY = playerY + dy;

            // Check if moving outside the current map -> switch maps
            if (newX < 0)
            {
                RestoreOriginalTile(_currentLocation); // remove player from old map
                TrySwitchMap(_currentMap.LeftMap, width - 1, newY);
                return;
            }
            if (newX >= width)
            {
                RestoreOriginalTile(_currentLocation);
                TrySwitchMap(_currentMap.RightMap, 0, newY);
                return;
            }
            if (newY < 0)
            {
                RestoreOriginalTile(_currentLocation);
                TrySwitchMap(_currentMap.UpMap, newX, height - 1);
                return;
            }
            if (newY >= height)
            {
                RestoreOriginalTile(_currentLocation);
                TrySwitchMap(_currentMap.DownMap, newX, 0);
                return;
            }


            // Regular move inside map
            int newIndex = newY * width + newX;
            if ((int)_currentMapTiles[newIndex].Item2 == 0)
            {
                RestoreOriginalTile(_currentLocation);
                _currentLocation = newIndex;
                SetPlayerTile(_currentLocation);
            }
            if ((int)_currentMapTiles[newIndex].Item2 == 1)
            {
                RestoreOriginalTile(_currentLocation);
                _currentLocation = newIndex;
                SetPlayerTile(_currentLocation);
                //ToDo: set event
            }
        }

        /// <summary>
        /// Handles switching to a linked map and placing the player at the given coordinates.
        /// </summary>
        private void TrySwitchMap(string? mapName, int newX, int newY)
        {
            if (string.IsNullOrEmpty(mapName))
                return; // No linked map

            MapData? newMap = GameDataManager.Instance.MapData.maps.FirstOrDefault(m => m.Name == mapName);
            if (newMap == null)
                return;

            _currentMap = newMap;
            _currentMapTiles = GameMap.Instance.mapTiles[_currentMap];
            _currentLocation = newY * _currentMap.Width + newX;
            SetPlayerTile(_currentLocation);
        }

        /// <summary>
        /// Places the player at the given index.
        /// </summary>
        private void SetPlayerTile(int index)
        {
            _currentMapTiles[index] = (_currentMapTiles[index].Item1, TileTypeSecondLayer.player);
        }

        /// <summary>
        /// Restores the original tile from the base map data.
        /// </summary>
        /// 
        private void RestoreOriginalTile(int index)
        {
            var originalTile = GameMap.Instance.baseMapTiles[_currentMap][index];
            _currentMapTiles[index] = originalTile;
        }

        public MapData CurrentMap => _currentMap;
        public int CurrentLocation => _currentLocation;
    }
}
