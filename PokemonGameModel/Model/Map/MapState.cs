using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Map
{
    public class SpriteOverlay
    {
        public string ImagePath { get; set; }
        public int PixelX { get; set; }
        public int PixelY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public SpriteOverlay(string imagePath, int pixelX, int pixelY, int width, int height)
        {
            ImagePath = imagePath;
            PixelX = pixelX;
            PixelY = pixelY;
            Width = width;
            Height = height;
        }
    }
    public class MapState
    {
        private MapDomain _activeMap;
        private int[,] _backgroundTiles;
        private int[,] _foregroundTiles;

        private readonly Dictionary<MapDomain, (int[,] bg, int[,] fg)> _mapCache = new Dictionary<MapDomain, (int[,], int[,])>();

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

        // ── Viewport ─────────────────────────────────────────────────────────

        public (int[,] background, int[,] foreground, int[,] vision, SpriteOverlay player) BuildViewPort(
    PlayerDomain player, SquareMapState squareMap)
        {
            var bg = BuildLayerViewport(player.playerLoc, isForeground: false);
            var fg = new int[MapConstants.ViewRowSize, MapConstants.ViewColSize];
            var vision = BuildVisionViewport(player.playerLoc, squareMap);

            StampNpcs(fg, player.playerLoc);

            // Player is always dead center of the viewport, but 24px tall instead of 16px
            // so we shift it up by 8px (one extra row) so feet align with collision square
            int tileSize = MapConstants.TileSize;          // e.g. 16
            int centerX = (MapConstants.ViewColSize / 2) * tileSize;
            int centerY = (MapConstants.ViewRowSize / 2) * tileSize;
            var playerSprite = new SpriteOverlay(
                imagePath: PlayerSprites.GetFrame(player.FacingDirection, player.AnimationTick, player.IsMoving),
                pixelX: centerX,
                pixelY: centerY - (24 - tileSize),   // shift up so feet sit on collision row
                width: 16,
                height: 24
            );

            return (bg, fg, vision, playerSprite);
        }

        // ── Private — viewport building ───────────────────────────────────────

        private int[,] BuildLayerViewport((int playerRow, int playerCol) pos, bool isForeground)
        {
            int halfRows = MapConstants.ViewRowSize / 2;
            int halfCols = MapConstants.ViewColSize / 2;
            var view = new int[MapConstants.ViewRowSize, MapConstants.ViewColSize];

            for (int r = 0; r < MapConstants.ViewRowSize; r++)
                for (int c = 0; c < MapConstants.ViewColSize; c++)
                    view[r, c] = SampleTile(pos.playerRow - halfRows + r,
                                            pos.playerCol - halfCols + c,
                                            isForeground);
            return view;
        }

        private int[,] BuildVisionViewport((int playerRow, int playerCol) pos, SquareMapState squareMap)
        {
            int vRows = MapConstants.ViewRowSize / MapConstants.TilesPerSquare;
            int vCols = MapConstants.ViewColSize / MapConstants.TilesPerSquare;
            var view = new int[vRows, vCols];
            var (psr, psc) = squareMap.TileToSquare(pos.playerRow, pos.playerCol);
            int halfRows = vRows / 2;
            int halfCols = vCols / 2;

            for (int r = 0; r < vRows; r++)
                for (int c = 0; c < vCols; c++)
                {
                    int srcRow = psr - halfRows + r;
                    int srcCol = psc - halfCols + c;
                    if ((uint)srcRow < (uint)squareMap.SquareRows &&
                        (uint)srcCol < (uint)squareMap.SquareCols)
                        view[r, c] = squareMap.VisionLayer[srcRow, srcCol];
                }

            return view;
        }

        private void StampNpcs(int[,] fg, (int playerRow, int playerCol) pos)
        {
            int halfRows = MapConstants.ViewRowSize / 2;
            int halfCols = MapConstants.ViewColSize / 2;

            foreach (var npc in _activeMap.Npc)
            {
                if (npc.Sprite == null) continue;
                var sprite = npc.Sprite.GetSprite(npc.direction);
                if (sprite == null) continue;

                int r = npc.Location.x - pos.playerRow + halfRows - 1;
                int c = npc.Location.y - pos.playerCol + halfCols - 1;

                if (r < 0 || r + 1 >= fg.GetLength(0) ||
                    c < 0 || c + 1 >= fg.GetLength(1)) continue;

                fg[r, c] = sprite.Value.TL;
                fg[r, c + 1] = sprite.Value.TR;
                fg[r + 1, c] = sprite.Value.BL;
                fg[r + 1, c + 1] = sprite.Value.BR;
            }
        }

        // ── Private — neighbor-aware tile sampling ────────────────────────────

        private int SampleTile(int row, int col, bool isForeground)
        {
            int mapRows = _backgroundTiles.GetLength(0);
            int mapCols = _backgroundTiles.GetLength(1);

            if ((uint)row < (uint)mapRows && (uint)col < (uint)mapCols)
                return _backgroundTiles[row, col];

            var neighbor = FindNeighbor(row, col, mapRows, mapCols);
            if (neighbor == null) return 0;

            var (neighborMap, nRow, nCol) = neighbor.Value;
            var (nbg, _) = GetCachedTiles(neighborMap);

            return (uint)nRow < (uint)nbg.GetLength(0) &&
                   (uint)nCol < (uint)nbg.GetLength(1)
                ? nbg[nRow, nCol]
                : 0;
        }

        private (MapDomain map, int row, int col)? FindNeighbor(int row, int col, int mapRows, int mapCols)
        {
            if (row < 0)
            {
                var conn = GetNeighbor(ConnectionDirection.North);
                if (conn == null) return null;
                int nRows = GetCachedTiles(conn.ConnectedMap).bg.GetLength(0);
                return (conn.ConnectedMap, nRows + row, col + conn.Margin);
            }
            if (row >= mapRows)
            {
                var conn = GetNeighbor(ConnectionDirection.South);
                if (conn == null) return null;
                return (conn.ConnectedMap, row - mapRows, col + conn.Margin);
            }
            if (col < 0)
            {
                var conn = GetNeighbor(ConnectionDirection.West);
                if (conn == null) return null;
                int nCols = GetCachedTiles(conn.ConnectedMap).bg.GetLength(1);
                return (conn.ConnectedMap, row + conn.Margin, nCols + col);
            }
            if (col >= mapCols)
            {
                var conn = GetNeighbor(ConnectionDirection.East);
                if (conn == null) return null;
                return (conn.ConnectedMap, row + conn.Margin, col - mapCols);
            }
            return null;
        }

        private ConnectedMapDomain? GetNeighbor(ConnectionDirection direction)
            => _activeMap.ConnectedMaps.FirstOrDefault(c => c.ConnectionDirection == direction);

        // ── Private — cache helpers ───────────────────────────────────────────

        private (int[,] bg, int[,] fg) GetCachedTiles(MapDomain map)
        {
            if (_mapCache.TryGetValue(map, out var cached)) return cached;
            var built = (
                BuildTileArray(map.Blocks, map),
                new int[map.Height, map.Width]
            );
            _mapCache[map] = built;
            return built;
        }

        private void PreloadNeighbors(MapDomain map)
        {
            foreach (var connection in map.ConnectedMaps)
                GetCachedTiles(connection.ConnectedMap);
        }

        /// <summary>
        /// Builds a dense [height, width] tile grid from a sparse tile list.
        /// Uses each tile's X/Y coordinates to place it correctly —
        /// DO NOT use list index, tiles are sparse (empty cells are absent).
        /// </summary>
        private static int[,] BuildTileArray(List<TileDomain> blocks, MapDomain map)
        {
            var tiles = new int[map.Height, map.Width];
            foreach (var tile in blocks)
            {
                if ((uint)tile.Y < (uint)map.Height &&
                    (uint)tile.X < (uint)map.Width)
                    tiles[tile.Y, tile.X] = tile.Tileid;
            }
            return tiles;
        }
    }
}