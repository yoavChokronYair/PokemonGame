using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Map
{
    public class TileDomain
    {
        public int Tileid { get; set; }
        public TileType TileType { get; set; }
    }
    public class BlockDomain
    {
        public TileDomain[] Tiles = new TileDomain[16];
    }
    public class MapDomain
    {
        public string Name { get; set; }
        public List<BlockDomain> Blocks {  get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public BlockDomain DefultBlockID { get; set; }
        public string Song { get; set; }

        public int[,] FlyWrapLoc = new int[1,1];
        public int[,] TownMapLoc = new int[1,1];
        public MapTilesType TilesType { get; set; }
        public List<ConnectedMapDomain> ConnectedMaps { get; set; } = new();
    }
    public class ConnectedMapDomain
    {
        public MapDomain ConnectedMap { get; set;}
        public ConnectionDirection ConnectionDirection { get; set; }
        public int Margin { get; set; }
    }


    public class MapState
    {
        private const int BlockSize = 4;

        public FacingDirection PlayerFacing { get; set; } = FacingDirection.Down;

        private MapDomain _activeMap;
        private int[,] _blockTiles;
        private readonly Dictionary<MapDomain, int[,]> _mapCache = new();

        public int MapRows => _blockTiles.GetLength(0);
        public int MapCols => _blockTiles.GetLength(1);
        public string MapName => _activeMap?.Name ?? "Unknown";

        public MapState(MapDomain startMap)
        {
            _activeMap = startMap;
            _blockTiles = GetCachedTiles(startMap);
        }

        // Returns the 11×11 view centred on the player, with tile value 9 at the centre.
        public int[,] BuildViewport((int Row, int Col) playerPos)
        {
            const int viewRows = 11;
            const int viewCols = 11;
            int halfRows = viewRows / 2;
            int halfCols = viewCols / 2;
            int totalRows = _blockTiles.GetLength(0);
            int totalCols = _blockTiles.GetLength(1);

            var view = new int[viewRows, viewCols];

            for (int r = 0; r < viewRows; r++)
            {
                for (int c = 0; c < viewCols; c++)
                {
                    int srcRow = playerPos.Row - halfRows + r;
                    int srcCol = playerPos.Col - halfCols + c;

                    view[r, c] =
                        (uint)srcRow < (uint)totalRows && (uint)srcCol < (uint)totalCols
                            ? _blockTiles[srcRow, srcCol]
                            : ResolveNeighborTile(srcRow, srcCol);
                }
            }

            view[halfRows, halfCols] = 9; // player marker
            return view;
        }

        // Returns true when the player can step onto the target tile.
        public bool CanMoveTo((int Row, int Col) pos, (int dRow, int dCol) delta)
        {
            int newRow = pos.Row + delta.dRow;
            int newCol = pos.Col + delta.dCol;
            int totalRows = _blockTiles.GetLength(0);
            int totalCols = _blockTiles.GetLength(1);

            if (newRow >= 0 && newRow < totalRows && newCol >= 0 && newCol < totalCols)
                return _blockTiles[newRow, newCol] != 0;

            var dir = GetOutOfBoundsDirection(newRow, newCol, totalRows, totalCols);
            if (dir is null) return false;

            var connection = GetConnection(dir.Value);
            if (connection is null) return false;

            var (nr, nc) = TranslateToNeighbor(newRow, newCol, dir.Value, connection, totalRows, totalCols);
            var neighbor = GetCachedTiles(connection.ConnectedMap);

            return (uint)nr < (uint)neighbor.GetLength(0) &&
                    (uint)nc < (uint)neighbor.GetLength(1) &&
                    neighbor[nr, nc] != 0;
        }

        // Switches the active map and translates the player position.
        // Returns the original position if no connection exists.
        public (int Row, int Col) TryTransition((int Row, int Col) pos, ConnectionDirection dir)
        {
            var connection = GetConnection(dir);
            if (connection is null) return pos;

            int oldRows = _blockTiles.GetLength(0);
            int oldCols = _blockTiles.GetLength(1);

            _activeMap = connection.ConnectedMap;
            _blockTiles = GetCachedTiles(_activeMap);

            int newRows = _blockTiles.GetLength(0);
            int newCols = _blockTiles.GetLength(1);
            int margin = connection.Margin;

            return dir switch
            {
                ConnectionDirection.Up => (newRows - 1, pos.Col - margin),
                ConnectionDirection.Down => (0, pos.Col - margin),
                ConnectionDirection.Left => (pos.Row - margin, newCols - 1),
                ConnectionDirection.Right => (pos.Row - margin, 0),
                _ => pos
            };
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private int ResolveNeighborTile(int srcRow, int srcCol)
        {
            int totalRows = _blockTiles.GetLength(0);
            int totalCols = _blockTiles.GetLength(1);

            var dir = GetOutOfBoundsDirection(srcRow, srcCol, totalRows, totalCols);
            if (dir is null) return -1;

            var connection = GetConnection(dir.Value);
            if (connection is null) return -1;

            var (nr, nc) = TranslateToNeighbor(srcRow, srcCol, dir.Value, connection, totalRows, totalCols);
            var neighbor = GetCachedTiles(connection.ConnectedMap);

            return (uint)nr < (uint)neighbor.GetLength(0) &&
                    (uint)nc < (uint)neighbor.GetLength(1)
                ? neighbor[nr, nc]
                : -1;
        }

        private static (int r, int c) TranslateToNeighbor(
            int row, int col,
            ConnectionDirection dir,
            ConnectedMapDomain connection,
            int totalRows, int totalCols)
        {
            var neighbor = connection.ConnectedMap;
            int nr = neighbor.Height * BlockSize;
            int nc = neighbor.Width * BlockSize;

            return dir switch
            {
                ConnectionDirection.Up => (nr + row, col - connection.Margin),
                ConnectionDirection.Down => (row - totalRows, col - connection.Margin),
                ConnectionDirection.Left => (row - connection.Margin, nc + col),
                ConnectionDirection.Right => (row - connection.Margin, col - totalCols),
                _ => (row, col)
            };
        }

        private static ConnectionDirection? GetOutOfBoundsDirection(
            int row, int col, int totalRows, int totalCols)
        {
            if (row < 0) return ConnectionDirection.Up;
            if (row >= totalRows) return ConnectionDirection.Down;
            if (col < 0) return ConnectionDirection.Left;
            if (col >= totalCols) return ConnectionDirection.Right;
            return null;
        }

        private ConnectedMapDomain? GetConnection(ConnectionDirection direction)
            => _activeMap.ConnectedMaps.FirstOrDefault(c => c.ConnectionDirection == direction);

        private int[,] GetCachedTiles(MapDomain map)
        {
            if (_mapCache.TryGetValue(map, out var cached)) return cached;
            var built = BuildTileArray(map);
            _mapCache[map] = built;
            return built;
        }

        private static int[,] BuildTileArray(MapDomain map)
        {
            int rows = map.Height * BlockSize;
            int cols = map.Width * BlockSize;
            var tiles = new int[rows, cols];

            for (int b = 0; b < map.Blocks.Count; b++)
            {
                var block = map.Blocks[b];
                int blockRow = b / map.Width;
                int blockCol = b % map.Width;

                for (int t = 0; t < block.Tiles.Length; t++)
                {
                    var tile = block.Tiles[t];
                    if (tile is null) continue;

                    int globalRow = blockRow * BlockSize + t / BlockSize;
                    int globalCol = blockCol * BlockSize + t % BlockSize;
                    tiles[globalRow, globalCol] = tile.Tileid;
                }
            }

            return tiles;
        }
    }
}

