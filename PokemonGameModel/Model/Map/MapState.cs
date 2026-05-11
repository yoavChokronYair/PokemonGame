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

        // ── Viewport ──────────────────────────────────────────────────────────

        public (int[,] background, int[,] foreground, int[,] vision, SpriteOverlay player) BuildViewPort(
    PlayerDomain player, SquareMapState squareMap)
        {
            var bg = BuildLayerViewport(player.trainerMapLocDomain.playerLoc, isForeground: false);
            var fg = new int[MapConstants.ViewRowSize, MapConstants.ViewColSize];
            var vision = BuildVisionViewport(player.trainerMapLocDomain.playerLoc, squareMap);

            StampNpcs(fg, player.trainerMapLocDomain.playerLoc);

            // Player is always dead center of the viewport, but 24px tall instead of 16px
            // so we shift it up by 8px (one extra row) so feet align with collision square
            int tileSize = MapConstants.TileSize;          // e.g. 16
            int centerX = (MapConstants.ViewColSize / 2) * tileSize;
            int centerY = (MapConstants.ViewRowSize / 2) * tileSize;
            var playerSprite = new SpriteOverlay(
                imagePath: PlayerSprites.GetFrame(player.trainerMapLocDomain.FacingDirection, player.AnimationTick, player.IsMoving),
                pixelX: centerX,
                pixelY: centerY - (24 - tileSize),   // shift up so feet sit on collision row
                width: 16,
                height: 24
            );

            return (bg, fg, vision, playerSprite);
        }

        // ── Private — viewport building ───────────────────────────────────────

        private int[,] BuildLayerViewport((int x, int y) pos, bool isForeground)
        {
            // pos.x = tileCol, pos.y = tileRow
            int halfRows = MapConstants.ViewRowSize / 2;
            int halfCols = MapConstants.ViewColSize / 2;
            var view = new int[MapConstants.ViewRowSize, MapConstants.ViewColSize];

            for (int r = 0; r < MapConstants.ViewRowSize; r++)
                for (int c = 0; c < MapConstants.ViewColSize; c++)
                    view[r, c] = SampleTile(pos.y - halfRows + r,   // tileRow
                                            pos.x - halfCols + c,   // tileCol
                                            isForeground);
            return view;
        }

        private int[,] BuildVisionViewport((int x, int y) pos, SquareMapState squareMap)
        {
            // pos.x = tileCol, pos.y = tileRow
            int vRows = MapConstants.ViewRowSize / MapConstants.TilesPerSquare;
            int vCols = MapConstants.ViewColSize / MapConstants.TilesPerSquare;
            var view = new int[vRows, vCols];
            var (psr, psc) = squareMap.TileToSquare(pos.y, pos.x);   // (tileRow, tileCol)
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
            // pos.x = tileCol, pos.y = tileRow
            int halfRows = MapConstants.ViewRowSize / 2;
            int halfCols = MapConstants.ViewColSize / 2;

            foreach (var npc in _activeMap.Npc)
            {
                if (npc.Sprite == null) continue;
                var sprite = npc.Sprite.GetSprite(npc.direction);
                if (sprite == null) continue;

                // npc.Location.y = tileRow, npc.Location.x = tileCol
                int r = npc.Location.y - pos.playerRow + halfRows - 1;
                int c = npc.Location.x - pos.playerCol + halfCols - 1;

                if (r < 0 || r + 1 >= fg.GetLength(0) ||
                    c < 0 || c + 1 >= fg.GetLength(1)) continue;

                fg[r, c] = sprite.Value.TL;
                fg[r, c + 1] = sprite.Value.TR;
                fg[r + 1, c] = sprite.Value.BL;
                fg[r + 1, c + 1] = sprite.Value.BR;
            }
        }

        // ── Private — neighbor-aware tile sampling ────────────────────────────

        private int SampleTile(int tileRow, int tileCol, bool isForeground)
        {
            var tiles = isForeground ? _foregroundTiles : _backgroundTiles;
            int mapRows = tiles.GetLength(0);
            int mapCols = tiles.GetLength(1);

            if ((uint)tileRow < (uint)mapRows && (uint)tileCol < (uint)mapCols)
                return tiles[tileRow, tileCol];

            var neighbor = FindNeighbor(tileRow, tileCol, mapRows, mapCols);
            if (neighbor == null) return 0;

            var (neighborMap, nRow, nCol) = neighbor.Value;
            var (nbg, nfg) = GetCachedTiles(neighborMap);
            var ntiles = isForeground ? nfg : nbg;

            return (uint)nRow < (uint)ntiles.GetLength(0) &&
                   (uint)nCol < (uint)ntiles.GetLength(1)
                ? ntiles[nRow, nCol]
                : 0;
        }

        // ── Bug #7 fix: Margin applied to tile coords but Margin is square units
        //
        // FindNeighbor was adding Margin (square units) directly to tileRow/tileCol
        // (tile units). Must convert: tileMargin = Margin * TilesPerSquare.
        private (MapDomain map, int row, int col)? FindNeighbor(int tileRow, int tileCol, int mapRows, int mapCols)
        {
            int tps = MapConstants.TilesPerSquare;

            if (tileRow < 0)
            {
                var conn = GetNeighbor(ConnectionDirection.North);
                if (conn == null) return null;
                int nRows = GetCachedTiles(conn.ConnectedMap).bg.GetLength(0);
                return (conn.ConnectedMap, nRows + tileRow, tileCol + conn.Margin * tps);
            }
            if (tileRow >= mapRows)
            {
                var conn = GetNeighbor(ConnectionDirection.South);
                if (conn == null) return null;
                return (conn.ConnectedMap, tileRow - mapRows, tileCol + conn.Margin * tps);
            }
            if (tileCol < 0)
            {
                var conn = GetNeighbor(ConnectionDirection.West);
                if (conn == null) return null;
                int nCols = GetCachedTiles(conn.ConnectedMap).bg.GetLength(1);
                return (conn.ConnectedMap, tileRow + conn.Margin * tps, nCols + tileCol);
            }
            if (tileCol >= mapCols)
            {
                var conn = GetNeighbor(ConnectionDirection.East);
                if (conn == null) return null;
                return (conn.ConnectedMap, tileRow + conn.Margin * tps, tileCol - mapCols);
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
                BuildTileArray(map.BackgroundBlocks, map),
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

        private static int[,] BuildTileArray(List<TileDomain> blocks, MapDomain map)
        {
            var tiles = new int[map.Height, map.Width];
            foreach (var tile in blocks)
            {
                // tile.Y = tileRow, tile.X = tileCol
                if ((uint)tile.Y < (uint)map.Height &&
                    (uint)tile.X < (uint)map.Width)
                    tiles[tile.Y, tile.X] = tile.Tileid;
            }
            return tiles;
        }
    }
}