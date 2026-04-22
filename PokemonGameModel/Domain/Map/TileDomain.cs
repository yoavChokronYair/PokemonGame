using System.Runtime.CompilerServices;
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
        public List<TileDomain> BackgroundBlocks {  get; set; }
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
    public class PlayerState
    {
        public int LocationRow { get; set; }
        public int LocationCol { get; set; }
        public FacingDirection facingDirection { get; set; }

    }

    public class MapState
    {
        private MapDomain _activeMap;
        private int[,] _backgroundTiles;
        private int[,] _foregroundTiles;//has collision layer info, used for battle and movement
        private readonly Dictionary<MapDomain, (int[,] bg, int[,] fg)> _mapCache = new();

        public MapState(MapDomain startMap)
        {
            _activeMap = startMap;
            (_backgroundTiles, _foregroundTiles) = GetCachedTiles(startMap);
        }

        private (int[,] bg, int[,] fg) GetCachedTiles(MapDomain map)
        {
            if (_mapCache.TryGetValue(map, out var cached)) return cached;
            var built = (BuildTileArray(map.BackgroundBlocks, map), BuildTileArray(map.Blocks, map));
            _mapCache[map] = built;
            return built;
        }

        public (int[,] background, int[,] foreground) BuildViewPort((int playerRow, int playerCol) playerPos)
        {
            var bg = BuildLayerViewport(playerPos, _backgroundTiles);
            var fg = BuildLayerViewport(playerPos, _foregroundTiles);
            return (bg, fg);
        }
        
        private static int[,] BuildLayerViewport((int playerRow, int playerCol) playerPos, int[,] layer)
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

                    view[r, c] =
                        (uint)srcRow < (uint)layer.GetLength(0) && (uint)srcCol < (uint)layer.GetLength(1)
                            ? layer[srcRow, srcCol]
                            : 0;
                }
            }

            return view;
        }

        private static int[,] BuildTileArray(List<TileDomain> blocks, MapDomain map)
        {
            var tiles = new int[map.Height * MapConstants.BlockSize, map.Width * MapConstants.BlockSize];

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

