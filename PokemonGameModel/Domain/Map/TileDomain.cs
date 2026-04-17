using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Map
{
    public class TileDomain
    {
        public int Tileid { get; set; }
        public TileType tileType { get; set; }

    }
    public class BlockDomain
    {
        public TileDomain[] Tiles = new TileDomain[16];
    }
    public class MapDomain
    {
        public string Name { get; set; }
        public List<BlockDomain> blocks {  get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public BlockDomain defultBlockID { get; set; }
        public string song { get; set; }

        public int[,] flyWrapLoc = new int[1,1];
        public int[,] townMapLoc = new int[1,1];
        public mapTilesType tilesType { get; set; }
        public List<ConnectedMap> ConnectedMaps { get; set; } = new();
    }
    public class ConnectedMap
    {
        public MapDomain connectedMap { get; set;}
        public ConnectionDirection connectionDirection { get; set; }
        public int margin { get; set; }
    }
    public class mapState
    {
        private readonly MapDomain _activeMap;
        public int[,] blockTiles;

        public mapState(MapDomain map)
        {
            _activeMap = map;
            blockTiles = GetArrayFromMap(_activeMap);
        }
        private int[,] GetArrayFromMap(MapDomain map)
        {
            int blockSize = 4;

            int rows = _activeMap.height * blockSize;
            int cols = _activeMap.width * blockSize;

            int[,] tiles = new int[rows, cols];

            for (int b = 0; b < _activeMap.blocks.Count; b++)
            {
                var block = _activeMap.blocks[b];

                int blockRow = b / _activeMap.width;
                int blockCol = b % _activeMap.width;

                for (int t = 0; t < block.Tiles.Length; t++)
                {
                    var tile = block.Tiles[t];
                    if (tile == null) continue;

                    int localRow = t / blockSize;
                    int localCol = t % blockSize;

                    int globalRow = blockRow * blockSize + localRow;
                    int globalCol = blockCol * blockSize + localCol;

                    tiles[globalRow, globalCol] = (int)tile.tileType;
                }
            }
            return tiles;
        }
        public int[,] displayedArray((int Row, int Col) middleLoc)
        {
            int viewRows = 20;
            int viewCols = 18;

            int[,] displayArray = new int[viewRows, viewCols];

            int halfRows = viewRows / 2;
            int halfCols = viewCols / 2;

            int totalRows = blockTiles.GetLength(0);
            int totalCols = blockTiles.GetLength(1);

            for (int r = 0; r < viewRows; r++)
            {
                for (int c = 0; c < viewCols; c++)
                {
                    int sourceRow = middleLoc.Row - halfRows + r;
                    int sourceCol = middleLoc.Col - halfCols + c;

                    // ✅ Inside current map
                    if (sourceRow >= 0 && sourceRow < totalRows &&
                        sourceCol >= 0 && sourceCol < totalCols)
                    {
                        displayArray[r, c] = blockTiles[sourceRow, sourceCol];
                        continue;
                    }

                    // 🔄 Determine direction
                    ConnectionDirection? direction = null;

                    if (sourceRow < 0) direction = ConnectionDirection.Up;
                    else if (sourceRow >= totalRows) direction = ConnectionDirection.Down;
                    else if (sourceCol < 0) direction = ConnectionDirection.Left;
                    else if (sourceCol >= totalCols) direction = ConnectionDirection.Right;

                    if (direction == null)
                    {
                        displayArray[r, c] = -1;
                        continue;
                    }

                    var connection = GetConnection(direction.Value);

                    if (connection == null)
                    {
                        displayArray[r, c] = -1;
                        continue;
                    }

                    var neighborMap = connection.connectedMap;
                    var neighborTiles = GetArrayFromMap(neighborMap);

                    int neighborRows = neighborTiles.GetLength(0);
                    int neighborCols = neighborTiles.GetLength(1);

                    int newRow = sourceRow;
                    int newCol = sourceCol;

                    switch (direction)
                    {
                        case ConnectionDirection.Up:
                            newRow = neighborRows + sourceRow;
                            newCol = sourceCol - connection.margin;
                            break;

                        case ConnectionDirection.Down:
                            newRow = sourceRow - totalRows;
                            newCol = sourceCol - connection.margin;
                            break;

                        case ConnectionDirection.Left:
                            newCol = neighborCols + sourceCol;
                            newRow = sourceRow - connection.margin;
                            break;

                        case ConnectionDirection.Right:
                            newCol = sourceCol - totalCols;
                            newRow = sourceRow - connection.margin;
                            break;
                    }

                    // final bounds check
                    if (newRow >= 0 && newRow < neighborRows &&
                        newCol >= 0 && newCol < neighborCols)
                    {
                        displayArray[r, c] = neighborTiles[newRow, newCol];
                    }
                    else
                    {
                        displayArray[r, c] = -1;
                    }
                }
            }

            return displayArray;
        }
        private ConnectedMap? GetConnection(ConnectionDirection direction)
        {
            return _activeMap.ConnectedMaps
                .FirstOrDefault(c => c.connectionDirection == direction);
        }
    }

}
