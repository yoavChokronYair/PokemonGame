using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Map
{
    public static class MapConstants
    {
        public const int BlockSize = 4;
        public const int ViewRowSize = 10;
        public const int ViewColSize = 10;
    }
    public class TileDomain
    {
        public int Tileid { get; set; }
        public TileType TileType { get; set; }
    }
    public class MapDomain
    {
        public string Name { get; set; }
        public List<TileDomain> Blocks {  get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TileDomain DefultBlockID { get; set; }
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

        private MapDomain _activeMap;
        private int[,] _blockTiles;
        private readonly Dictionary<MapDomain, int[,]> _mapCache = new();

        public MapState(MapDomain startMap)
        {
            _activeMap = startMap;
            _blockTiles = GetCachedTiles(startMap);
        }

        private int[,] GetCachedTiles(MapDomain map)
        {
            if (_mapCache.TryGetValue(map, out var cached)) return cached;
            var built = BuildTileArray(map);
            _mapCache[map] = built;
            return built;
        }

        public int[,] BuildViewport((int Row, int Col) playerPos)
        {
            int halfRows = MapConstants.ViewRowSize / 2;
            int halfCols = MapConstants.ViewColSize / 2;

            var view = new int[MapConstants.ViewRowSize, MapConstants.ViewColSize];

            for (int r = 0; r < MapConstants.ViewRowSize; r++)
            {
                for (int c = 0; c < MapConstants.ViewColSize; c++)
                {
                    int srcRow = playerPos.Row - halfRows + r;
                    int srcCol = playerPos.Col - halfCols + c;

                    view[r, c] =
                        (uint)srcRow < (uint)_blockTiles.GetLength(0) && (uint)srcCol < (uint)_blockTiles.GetLength(1)
                            ? _blockTiles[srcRow, srcCol]
                            : _blockTiles[srcRow, srcCol];
                }
            }
            return view;
        }

        private static int[,] BuildTileArray(MapDomain map)
        {
            int rows = map.Height * MapConstants.BlockSize;
            int cols = map.Width * MapConstants.BlockSize;
            var tiles = new int[rows, cols];

            for (int b = 0; b < map.Blocks.Count; b++)
            {
                var tile = map.Blocks[b];
                if (tile is null) continue;

                tiles[b / map.Width, b % map.Width] = tile.Tileid;
            }

            return tiles;
        }
    }
}

