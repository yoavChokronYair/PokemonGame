using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data.MapData;

namespace PokemonGameModel.Model.Map
{
    public class PlayerOnMap
    {
        private int worldX;
        private int worldY;

        // Store the previous second layer of the tile player is on
        private TileTypeSecondLayer previousTileLayer = TileTypeSecondLayer.None;

        public PlayerOnMap(int startX, int startY)
        {
            worldX = startX;
            worldY = startY;
            SetPlayerTile();
        }

        /// <summary>
        /// Move the player by (dx, dy) in the world.
        /// </summary>
        public void Move(int dx, int dy, Direction playerDirection)
        {
            int newX = worldX + dx;
            int newY = worldY + dy;

            // Boundary check
            if (newX < 0 || newX >= GameMap.Instance.WorldWidth ||
                newY < 0 || newY >= GameMap.Instance.WorldHeight)
                return;

            int index = newY * GameMap.Instance.WorldWidth + newX;
            var targetTile = GameMap.Instance.WorldTiles[index];

            bool canMove = false;

            // Check tile rules
            if (targetTile.Item2 == TileTypeSecondLayer.None)
            {
                canMove = true;
            }
            else if (targetTile.Item2 == TileTypeSecondLayer.hill && playerDirection != Direction.Up)
            {
                canMove = true;
            }

            if (!canMove)
                return;

            // Restore the previous tile layer at current position
            RestoreOriginalTile();

            // Move player
            worldX = newX;
            worldY = newY;

            // Save the target tile's current second layer before overwriting
            previousTileLayer = GameMap.Instance.WorldTiles[index].Item2;

            // Place player on new tile
            SetPlayerTile();
        }

        /// <summary>
        /// Places the player at the current world coordinates.
        /// </summary>
        private void SetPlayerTile()
        {
            int index = worldY * GameMap.Instance.WorldWidth + worldX;
            var original = GameMap.Instance.WorldTiles[index];
            GameMap.Instance.WorldTiles[index] = (original.Item1, TileTypeSecondLayer.player);
        }

        /// <summary>
        /// Restores the tile to what it was before the player occupied it.
        /// </summary>
        private void RestoreOriginalTile()
        {
            int index = worldY * GameMap.Instance.WorldWidth + worldX;
            var original = GameMap.Instance.WorldTiles[index];
            GameMap.Instance.WorldTiles[index] = (original.Item1, previousTileLayer);
        }

        public int WorldX => worldX;
        public int WorldY => worldY;
    }
}
