using PokemonGame.Enums;
using PokemonGame.Model.Helper;
using PokemonGameModel.Model.Data.MapData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokemonGameModel.Model.Map
{
    public class RouteMap
    {
        private readonly HashSet<(RouteMapData, RouteMapData)> connectedPairs = new();
        private readonly RouteMapData[,] RouteMaps;
        private readonly RouteMapDataList routes;
        private readonly Dictionary<RouteMapData, Tile[,]> routeMapTiles = new Dictionary<RouteMapData, Tile[,]>();
        public RouteMap(RouteMapDataList routs)
        {
            this.routes = routs;
            this.RouteMaps = new RouteMapData[4, 4];
            ArrayHelper.SetCenter2DArray(RouteMaps, routs.maps[0]);
            foreach (RouteMapData route in routs.maps)
            {
                routeMapTiles.Add(route, CreateRouteTiles(route));
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
    }
}
