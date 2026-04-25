using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Map
{
    public class MapState
    {
        private MapDomain _activeMap;
        private int[,] _backgroundTiles;
        private int[,] _foregroundTiles;
        private readonly Dictionary<MapDomain, (int[,] bg, int[,] fg)> _mapCache = new();

        public MapState(MapDomain startMap)
        {
            _activeMap = startMap;
            (_backgroundTiles, _foregroundTiles) = GetCachedTiles(startMap);
            PreloadNeighbors(startMap);
        }

        public void ChangeMap(MapDomain newMap)
        {
            _activeMap = newMap;
            (_backgroundTiles, _foregroundTiles) = GetCachedTiles(newMap);
            PreloadNeighbors(newMap);
        }

        // ---------------------------------------------------------------
        // Viewport
        // ---------------------------------------------------------------

        public (int[,] background, int[,] foreground, int[,] vision) BuildViewPort(
            PlayerDomain player, SquareMapState squareMap)
        {
            var bg = BuildLayerViewport(player.playerLoc, isForeground: false);
            var fg = BuildLayerViewport(player.playerLoc, isForeground: true);
            var vision = BuildVisionViewport(player.playerLoc, squareMap);

            StampPlayer(fg, player.FacingDirection);
            return (bg, fg, vision);
        }

        private int[,] BuildVisionViewport(
            (int playerRow, int playerCol) playerPos, SquareMapState squareMap)
        {
            // Vision is in square-space; viewport is in tile-space.
            // Each square = 2×2 tiles, so the vision viewport is half the tile viewport.
            int visionRows = MapConstants.ViewRowSize / 2;
            int visionCols = MapConstants.ViewColSize / 2;
            var view = new int[visionRows, visionCols];

            // Player tile position → square position
            var (playerSquareRow, playerSquareCol) = squareMap.TileToSquare(
                playerPos.playerRow, playerPos.playerCol);

            int halfRows = visionRows / 2;
            int halfCols = visionCols / 2;

            for (int r = 0; r < visionRows; r++)
            {
                for (int c = 0; c < visionCols; c++)
                {
                    int srcRow = playerSquareRow - halfRows + r;
                    int srcCol = playerSquareCol - halfCols + c;

                    if ((uint)srcRow < (uint)squareMap.SquareRows &&
                        (uint)srcCol < (uint)squareMap.SquareCols)
                    {
                        view[r, c] = squareMap.VisionLayer[srcRow, srcCol];
                    }
                }
            }

            return view;
        }
        private static void StampPlayer(int[,] fg, FacingDirection direction)
        {
            // No sprite for None — skip stamping entirely
            if (direction == FacingDirection.None) return;
            if (!PlayerSprites.Tiles.TryGetValue(direction, out var sprite)) return;

            int midRow = MapConstants.ViewRowSize / 2;
            int midCol = MapConstants.ViewColSize / 2;

            fg[midRow - 1, midCol - 1] = sprite.TL;
            fg[midRow - 1, midCol] = sprite.TR;
            fg[midRow, midCol - 1] = sprite.BL;
            fg[midRow, midCol] = sprite.BR;
        }

        private int[,] BuildLayerViewport((int playerRow, int playerCol) playerPos, bool isForeground)
        {
            int halfRows = MapConstants.ViewRowSize / 2;
            int halfCols = MapConstants.ViewColSize / 2;
            var view = new int[MapConstants.ViewRowSize, MapConstants.ViewColSize];

            for (int r = 0; r < MapConstants.ViewRowSize; r++)
            {
                for (int c = 0; c < MapConstants.ViewColSize; c++)
                {
                    int srcRow = playerPos.playerRow - halfRows + r;
                    int srcCol = playerPos.playerCol - halfCols + c;

                    view[r, c] = SampleTile(srcRow, srcCol, isForeground);
                }
            }

            return view;
        }

        // ---------------------------------------------------------------
        // Neighbor-aware tile sampling
        // ---------------------------------------------------------------

        private int SampleTile(int row, int col, bool isForeground)
        {
            int mapRows = _backgroundTiles.GetLength(0);
            int mapCols = _backgroundTiles.GetLength(1);

            // Inside active map
            if ((uint)row < (uint)mapRows && (uint)col < (uint)mapCols)
                return GetActiveLayer(isForeground)[row, col];

            // Outside — try neighbor maps
            var neighbor = FindNeighbor(row, col, mapRows, mapCols);
            if (neighbor == null) return 0;

            var (neighborMap, neighborRow, neighborCol) = neighbor.Value;
            var (nbg, nfg) = GetCachedTiles(neighborMap);
            var layer = isForeground ? nfg : nbg;

            if ((uint)neighborRow < (uint)layer.GetLength(0) &&
                (uint)neighborCol < (uint)layer.GetLength(1))
                return layer[neighborRow, neighborCol];

            return 0;
        }

        private (MapDomain map, int row, int col)? FindNeighbor(int row, int col, int mapRows, int mapCols)
        {
            // North neighbor — row is above the active map
            if (row < 0)
            {
                var connection = GetNeighbor(ConnectionDirection.North);
                if (connection == null) return null;

                var (nbg, _) = GetCachedTiles(connection.ConnectedMap);
                int neighborRows = nbg.GetLength(0);

                // Flip: row -1 maps to the last row of the north map, etc.
                int neighborRow = neighborRows + row;
                int neighborCol = col + connection.Margin;
                return (connection.ConnectedMap, neighborRow, neighborCol);
            }

            // South neighbor — row is below the active map
            if (row >= mapRows)
            {
                var connection = GetNeighbor(ConnectionDirection.South);
                if (connection == null) return null;

                int neighborRow = row - mapRows;
                int neighborCol = col + connection.Margin;
                return (connection.ConnectedMap, neighborRow, neighborCol);
            }

            // West neighbor — col is left of the active map
            if (col < 0)
            {
                var connection = GetNeighbor(ConnectionDirection.West);
                if (connection == null) return null;

                var (nbg, _) = GetCachedTiles(connection.ConnectedMap);
                int neighborCols = nbg.GetLength(1);

                int neighborRow = row + connection.Margin;
                int neighborCol = neighborCols + col;
                return (connection.ConnectedMap, neighborRow, neighborCol);
            }

            // East neighbor — col is right of the active map
            if (col >= mapCols)
            {
                var connection = GetNeighbor(ConnectionDirection.East);
                if (connection == null) return null;

                int neighborRow = row + connection.Margin;
                int neighborCol = col - mapCols;
                return (connection.ConnectedMap, neighborRow, neighborCol);
            }

            return null;
        }

        private ConnectedMapDomain? GetNeighbor(ConnectionDirection direction)
            => _activeMap.ConnectedMaps.FirstOrDefault(c => c.ConnectionDirection == direction);

        // ---------------------------------------------------------------
        // Cache helpers
        // ---------------------------------------------------------------

        private int[,] GetActiveLayer(bool isForeground)
            => isForeground ? _foregroundTiles : _backgroundTiles;

        private (int[,] bg, int[,] fg) GetCachedTiles(MapDomain map)
        {
            if (_mapCache.TryGetValue(map, out var cached)) return cached;
            var built = (BuildTileArray(map.BackgroundBlocks, map), BuildTileArray(map.Blocks, map));
            _mapCache[map] = built;
            return built;
        }

        private void PreloadNeighbors(MapDomain map)
        {
            foreach (var connection in map.ConnectedMaps)
                GetCachedTiles(connection.ConnectedMap); // builds + caches if not already present
        }

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
