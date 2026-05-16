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

        public (int[,] background, int[,] foreground, int[,] vision,
                List<SpriteOverlay> npcs, SpriteOverlay player) BuildViewPort(
            PlayerDomain player, SquareMapState squareMap)
        {
            var bg = BuildLayerViewport(player.trainerMapLocDomain.playerLoc, isForeground: false);
            var fg = new int[MapConstants.ViewRowSize, MapConstants.ViewColSize];
            var vision = BuildVisionViewport(player.trainerMapLocDomain.playerLoc, squareMap);
            var npcs = BuildNpcOverlays(player.trainerMapLocDomain.playerLoc);

            int tileSize = MapConstants.TileSize;
            int centerX = (MapConstants.ViewColSize / 2) * tileSize;
            int centerY = (MapConstants.ViewRowSize / 2) * tileSize;

            var playerSprite = new SpriteOverlay(
                imagePath: PlayerSprites.GetFrame(
                    player.trainerMapLocDomain.FacingDirection,
                    player.AnimationTick,
                    player.IsMoving),
                pixelX: centerX,
                pixelY: centerY - (24 - tileSize),
                width: 16,
                height: 24
            );

            return (bg, fg, vision, npcs, playerSprite);
        }

        // ── NPC overlays — mirrors the player sprite exactly ──────────────────

        private List<SpriteOverlay> BuildNpcOverlays((int x, int y) pos)
        {
            int tileSize = MapConstants.TileSize;

            int halfRows = MapConstants.ViewRowSize / 2;
            int halfCols = MapConstants.ViewColSize / 2;

            var overlays = new List<SpriteOverlay>();

            foreach (var npc in _activeMap.Npc)
            {
                int relRow = npc.Location.y - pos.y + halfRows;
                int relCol = npc.Location.x - pos.x + halfCols;

                if (relRow < 0 || relRow >= MapConstants.ViewRowSize ||
                    relCol < 0 || relCol >= MapConstants.ViewColSize)
                {
                    continue;
                }

                int pixelX = relCol * tileSize;
                int pixelY = relRow * tileSize - (24 - tileSize);

                overlays.Add(new SpriteOverlay(
                    imagePath: NpcSprites.GetFrame(
                        npc.NpcInfo.SpriteId ?? 1,
                        npc.Direction,
                        0,
                        false
                    ),
                    pixelX: pixelX,
                    pixelY: pixelY,
                    width: 16,
                    height: 24
                ));
            }

            return overlays;
        }

        // ── Private — viewport building ───────────────────────────────────────

        private int[,] BuildLayerViewport((int x, int y) pos, bool isForeground)
        {
            int halfRows = MapConstants.ViewRowSize / 2;
            int halfCols = MapConstants.ViewColSize / 2;
            var view = new int[MapConstants.ViewRowSize, MapConstants.ViewColSize];

            for (int r = 0; r < MapConstants.ViewRowSize; r++)
                for (int c = 0; c < MapConstants.ViewColSize; c++)
                    view[r, c] = SampleTile(pos.y - halfRows + r,
                                            pos.x - halfCols + c,
                                            isForeground);
            return view;
        }

        private int[,] BuildVisionViewport((int x, int y) pos, SquareMapState squareMap)
        {
            int vRows = MapConstants.ViewRowSize / MapConstants.TilesPerSquare;
            int vCols = MapConstants.ViewColSize / MapConstants.TilesPerSquare;
            var view = new int[vRows, vCols];
            var (psr, psc) = squareMap.TileToSquare(pos.y, pos.x);
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
                if ((uint)tile.Y < (uint)map.Height &&
                    (uint)tile.X < (uint)map.Width)
                    tiles[tile.Y, tile.X] = tile.Tileid;
            }
            return tiles;
        }
    }
}