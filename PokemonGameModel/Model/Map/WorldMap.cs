using PokemonGame.Enums;
using PokemonGame.Model.Helper;
using PokemonGameModel.Model.Map;


using PokemonGame.Services.Data.MapData;

namespace PokemonGame.Model.Map
{
    public class WorldMap
    {
        private readonly WorldData[,] townMaps;
        
        public Dictionary<RouteMapData, Tile[,]> routeMapTiles = new Dictionary<RouteMapData, Tile[,]>();
        public Dictionary<TownMapData, Tile[,]> townMapTiles = new Dictionary<TownMapData, Tile[,]>();

        //town
        private readonly HashSet<(WorldData, WorldData)> connectedPairs = new();
        public WorldMap(TownMapDataList towns, RouteMapDataList routs)
        {
            this.townMaps = new WorldData[4, 4];
            ArrayHelper.SetCenter2DArray(townMaps, towns.maps[0]);
            foreach (RouteMapData route in routs.maps)
            {
                routeMapTiles.Add(route, CreateRouteTiles(route));
            }

            foreach (TownMapData town in towns.maps)
            {
                CreateTownConnections(town);
                townMapTiles.Add(town, CreateTownTiles(town));
            }
        }
        public void CreateTownConnections(TownMapData town)
        {
            if (town.connections == null)
                return;

            var townPos = ArrayHelper.FindIn2DArrayIndex(townMaps, t => t == town);
            if (townPos == null)
                return; // Skip if this town isn't placed yet

            int row = townPos.Value.Row;
            int col = townPos.Value.Col;

            Direction direction = Direction.Left;

            foreach (var neighborName in town.connections)
            {
                if (neighborName == 0)
                {
                    direction = (Direction)((int)direction + 1);
                    continue;
                }

                RouteMapData? neighborMap = (RouteMapData?)routeMapTiles.Keys.ToList().FirstOrDefault(t => t.ID == neighborName);

                if (connectedPairs.Contains((town, neighborMap)))
                {
                    direction = (Direction)((int)direction + 1);
                    continue;
                }

                int newRow = row, newCol = col;

                switch (direction)
                {
                    case Direction.Left: newCol = col - 1; break;
                    case Direction.Right: newCol = col + 1; break;
                    case Direction.Up: newRow = row - 1; break;
                    case Direction.Down: newRow = row + 1; break;
                }

                if (newRow >= 0 && newRow < townMaps.GetLength(0) &&
                    newCol >= 0 && newCol < townMaps.GetLength(1) &&
                    townMaps[newRow, newCol] == null)
                {
                    townMaps[newRow, newCol] = neighborMap;
                }

                direction = (Direction)((int)direction + 1);
                connectedPairs.Add((town, neighborMap));
            }
        }

        private Tile[,] CreateRouteTiles(RouteMapData routeData)
        {
            Tile[,] mapTiles = new Tile[routeData.Width, routeData.Height];

            for (int x = 0; x < routeData.Width; x++)
            {
                for (int y = 0; y < routeData.Height; y++)
                {
                    Tile tile = new Tile();
                    tile.BackgroundID = routeData.pathID;
                    tile.type = TileType.None;
                    mapTiles[x, y] = tile;
                }
            }
            // Fill regions with their IDs
            if (routeData.Regions != null)
            {
                foreach (var region in routeData.Regions)
                {
                    int maxX = region.StartX + region.Width;
                    int maxY = region.StartY + region.Height;

                    for (int x = region.StartX; x < maxX; x++)
                    {
                        for (int y = region.StartY; y < maxY; y++)
                        {
                            Tile tile = new Tile();
                            tile.BackgroundID = region.ID;
                            tile.type = region.TileType;
                            mapTiles[x, y] = tile;
                        }
                    }
                }
            }
            return mapTiles;
        }

        private Tile[,] CreateTownTiles(TownMapData townData)
        {
            Tile[,] mapTiles = new Tile[townData.Width, townData.Height];

            for (int x = 0; x < townData.Width; x++)
            {
                for (int y = 0; y < townData.Height; y++)
                {
                    Tile tile = new Tile();
                    tile.BackgroundID = townData.pathID;
                    tile.type = TileType.None;
                    mapTiles[x, y] = tile;
                }
            }
            // Fill regions with their IDs
            if (townData.Regions != null)
            {
                foreach (var region in townData.Regions)
                {
                    int maxX = region.StartX + region.Width;
                    int maxY = region.StartY + region.Height;

                    for (int x = region.StartX; x < maxX; x++)
                    {
                        for (int y = region.StartY; y < maxY; y++)
                        {
                            Tile tile = new Tile();
                            tile.BackgroundID = region.ID;
                            tile.type = region.TileType;
                            mapTiles[x, y] = tile;
                        }
                    }
                }
            }
            return mapTiles;
        }

    }
}
