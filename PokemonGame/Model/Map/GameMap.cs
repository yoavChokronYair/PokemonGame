using PokemonGame.Enums;
using PokemonGame.Model.Data;
using PokemonGame.Model.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace PokemonGame.Model.Map
{
    public class GameMap
    {
        public MapData currentMapData;
        public TileType[][] Tiles { get; set; }
        public string Name { get; set; }

        public Dictionary<string, GameMap> neighborMaps = new Dictionary<string, GameMap>();

        public GameMap(MapData def, HashSet<string> visited = null)
        {
            if (visited == null)
                visited = new HashSet<string>();

            GenerateGameMapFromRegions(def, visited);
        }

        public void GenerateGameMapFromRegions(MapData def, HashSet<string> visited)
        {
            if (visited.Contains(def.Name)) return;
            visited.Add(def.Name);

            var tiles = new TileType[def.Height][];
            for (int y = 0; y < def.Height; y++)
            {
                tiles[y] = new TileType[def.Width];
                for (int x = 0; x < def.Width; x++)
                    tiles[y][x] = TileType.Empty;
            }

            foreach (var region in def.Regions)
            {
                if (!Enum.TryParse<TileType>(region.Name, true, out var type))
                    throw new Exception($"Invalid tile type: {region.Name}");

                for (int y = 0; y < region.Height; y++)
                {
                    for (int x = 0; x < region.Width; x++)
                    {
                        int tileX = region.StartX + x;
                        int tileY = region.StartY + y;

                        if (tileX >= 0 && tileX < def.Width && tileY >= 0 && tileY < def.Height)
                            tiles[tileY][tileX] = type;
                    }
                }
            }

            Tiles = tiles;
            Name = def.Name;
            currentMapData = def;

            neighborMaps.Clear();

            void TryAddNeighbor(string direction, string mapName)
            {
                if (!string.IsNullOrEmpty(mapName) && !visited.Contains(mapName))
                {
                    var mapData = GameDataManager.Instance.MapData.maps.FirstOrDefault(m => m.Name == mapName);
                    if (mapData != null)
                        neighborMaps[direction] = new GameMap(mapData, visited);
                }
            }

            TryAddNeighbor("Left", def.LeftMap);
            TryAddNeighbor("Right", def.RightMap);
            TryAddNeighbor("Up", def.UpMap);
            TryAddNeighbor("Down", def.DownMap);
        }



        public TileType GetTileAt(int x, int y)
        {
            if (x >= 0 && x < currentMapData.Width && y >= 0 && y < currentMapData.Height)
                return Tiles[y][x];
            return TileType.Empty;
        }

        public bool TryMove(string input, ref int x, ref int y, ref Direction direction)
        {
            int newX = x;
            int newY = y;
            Direction newDirection = direction;

            switch (input)
            {
                case "W":
                case "Up": newDirection = Direction.Up; newY--; break;
                case "S":
                case "Down": newDirection = Direction.Down; newY++; break;
                case "A":
                case "Left": newDirection = Direction.Left; newX--; break;
                case "D":
                case "Right": newDirection = Direction.Right; newX++; break;
                default: return false;
            }

            if (HandleMapBoundary(ref newX, ref newY, newDirection))
            {
                x = newX;
                y = newY;
                direction = newDirection;
                return true;
            }

            if (IsWithinBounds(newX, newY))
            {
                x = newX;
                y = newY;
                direction = newDirection;
                return true;
            }

            return false;
        }

        private bool HandleMapBoundary(ref int x, ref int y, Direction newDirection)
        {
            string targetMapName = null;
            int newX = x, newY = y;

            if (x < 0) { targetMapName = currentMapData.LeftMap; newX = Tiles[0].Length - 1; }
            else if (x >= currentMapData.Width) { targetMapName = currentMapData.RightMap; newX = 0; }
            else if (y < 0) { targetMapName = currentMapData.UpMap; newY = Tiles.Length - 1; }
            else if (y >= currentMapData.Height) { targetMapName = currentMapData.DownMap; newY = 0; }

            if (string.IsNullOrEmpty(targetMapName)) return false;

            var newMapData = GameDataManager.Instance.MapData.maps.FirstOrDefault(m => m.Name == targetMapName);
            if (newMapData != null)
            {
                var visited = new HashSet<string>();
                GenerateGameMapFromRegions(newMapData,visited);
                x = newX;
                y = newY;
                return true;
            }

            return false;
        }

        private bool IsWithinBounds(int x, int y)
        {
            return x >= 0 && x < currentMapData.Width && y >= 0 && y < currentMapData.Height;
        }

        public List<TileRenderInfo> GetViewportTiles(int centerX, int centerY, int viewWidth, int viewHeight, Direction playerDirection, int tileWidth, int tileHeight)
        {
            var tiles = new List<TileRenderInfo>();
            int halfWidth = viewWidth / 2;
            int halfHeight = viewHeight / 2;

            for (int row = 0; row < viewHeight; row++)
            {
                for (int col = 0; col < viewWidth; col++)
                {
                    int mapX = centerX - halfWidth + col;
                    int mapY = centerY - halfHeight + row;

                    TileType tileType = GetTileWithNeighborFallback(mapX, mapY);

                    var info = new TileRenderInfo
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Color = GetBrushForTile(tileType),
                        X1 = 0,
                        Y1 = 0,
                        X2 = 0,
                        Y2 = 0
                    };

                    if (mapX == centerX && mapY == centerY)
                    {
                        info.Color = Brushes.Red;
                        info.X1 = tileWidth / 2;
                        info.Y1 = tileHeight / 2;

                        switch (playerDirection)
                        {
                            case Direction.Up: info.X2 = info.X1; info.Y2 = 0; break;
                            case Direction.Down: info.X2 = info.X1; info.Y2 = tileHeight; break;
                            case Direction.Left: info.X2 = 0; info.Y2 = info.Y1; break;
                            case Direction.Right: info.X2 = tileWidth; info.Y2 = info.Y1; break;
                        }
                    }

                    tiles.Add(info);
                }
            }

            return tiles;
        }
        private TileType GetTileWithNeighborFallback(int x, int y)
        {
            if (IsWithinBounds(x, y))
                return Tiles[y][x];

            if (x < 0 && neighborMaps.TryGetValue("Left", out var leftMap) && y >= 0 && y < currentMapData.Height)
                return leftMap.GetTileAt(leftMap.currentMapData.Width + x, y);

            if (x >= currentMapData.Width && neighborMaps.TryGetValue("Right", out var rightMap) && y >= 0 && y < currentMapData.Height)
                return rightMap.GetTileAt(x - currentMapData.Width, y);

            if (y < 0 && neighborMaps.TryGetValue("Up", out var upMap) && x >= 0 && x < currentMapData.Width)
                return upMap.GetTileAt(x, upMap.currentMapData.Height + y);

            if (y >= currentMapData.Height && neighborMaps.TryGetValue("Down", out var downMap) && x >= 0 && x < currentMapData.Width)
                return downMap.GetTileAt(x, y - currentMapData.Height);

            return TileType.Black; // Black tile when there's no neighbor
        }


        public Brush GetBrushForTile(TileType type)
        {
            switch (type)
            {
                case TileType.Path: return Brushes.Gray;
                case TileType.Grass: return Brushes.Green;
                case TileType.Water: return Brushes.Blue;
                case TileType.Empty: return Brushes.White;
                case TileType.Black: return Brushes.Black;
                default: return Brushes.Red;
            }
        }

        public class TileRenderInfo
        {
            public double Width { get; set; }
            public double Height { get; set; }
            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }
            public Brush Color { get; set; }
        }
    }
}
