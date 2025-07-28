
using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Manager;

namespace PokemonGameModel.Model.Map
{
    public class GameMap
    {
        public TileType[,,] MultiLayerTiles; // [layer, y, x]
        public MapData currentMapData;
        public TileType[][] Tiles { get; set; }
        public string Name { get; set; }
        public Dictionary<string, GameMap> neighborMaps = new Dictionary<string, GameMap>();

        public GameMap(MapData def, HashSet<string> visited = null)
        {
            if (visited == null)
            {
                visited = new HashSet<string>();
            }            GenerateGameMapFromRegions(def, visited);
        }

        public void GenerateGameMapFromRegions(MapData def, HashSet<string> visited, int trainerVision = 8)
        {
            if (visited.Contains(def.Name)) return;
            visited.Add(def.Name);

            var tiles = new TileType[2, def.Height, def.Width];

            // Initialize all tiles
            for (int y = 0; y < def.Height; y++)
            {
                for (int x = 0; x < def.Width; x++)
                {
                    tiles[0, y, x] = TileType.Empty;
                    tiles[1, y, x] = TileType.None;
                }
            }

            // Fill terrain tiles
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
                            tiles[0, tileY, tileX] = type;
                    }
                }
            }

            // Place trainer and vision
           

            MultiLayerTiles = tiles;
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
            if (!IsWithinBounds(x, y))
                return TileType.Empty;

            var overlay = MultiLayerTiles[1, y, x];
            return overlay != TileType.None ? overlay : MultiLayerTiles[0, y, x];
        }

        public bool TryMove(string input, ref int x, ref int y, ref Direction direction)
        {
            int newX = x, newY = y;
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

            if (!IsWithinBounds(newX, newY))
                return false;

            var overlay = MultiLayerTiles[1, newY, newX];
            var tile = overlay != TileType.None ? overlay : MultiLayerTiles[0, newY, newX];

            if (GetBrushForTile(tile) == "Yellow")
                return false;

            x = newX;
            y = newY;
            direction = newDirection;
            return true;
        }

        private bool HandleMapBoundary(ref int x, ref int y, Direction newDirection)
        {
            string targetMapName = null;
            int newX = x, newY = y;

            if (x < 0)
            {
                targetMapName = currentMapData.LeftMap;
                newX = currentMapData.Width - 1;
            }
            else if (x >= currentMapData.Width)
            {
                targetMapName = currentMapData.RightMap;
                newX = 0;
            }
            else if (y < 0)
            {
                targetMapName = currentMapData.UpMap;
                newY = currentMapData.Height - 1;
            }
            else if (y >= currentMapData.Height)
            {
                targetMapName = currentMapData.DownMap;
                newY = 0;
            }

            if (string.IsNullOrEmpty(targetMapName)) return false;

            var newMapData = GameDataManager.Instance.MapData.maps.FirstOrDefault(m => m.Name == targetMapName);
            if (newMapData == null) return false;

            var visited = new HashSet<string>();
            GenerateGameMapFromRegions(newMapData, visited);

            x = newX;
            y = newY;
            return true;
        }

        private bool IsWithinBounds(int x, int y)
        {
            return x >= 0 && x < currentMapData.Width && y >= 0 && y < currentMapData.Height;
        }

        public TileType GetTileWithNeighborFallback(int x, int y)
        {
            if (IsWithinBounds(x, y))
            {
                var overlay = MultiLayerTiles[1, y, x];
                return overlay != TileType.None ? overlay : MultiLayerTiles[0, y, x];
            }

            if (x < 0 && neighborMaps.TryGetValue("Left", out var leftMap))
                return leftMap.GetTileWithNeighborFallback(leftMap.currentMapData.Width + x, y);

            if (x >= currentMapData.Width && neighborMaps.TryGetValue("Right", out var rightMap))
                return rightMap.GetTileWithNeighborFallback(x - currentMapData.Width, y);

            if (y < 0 && neighborMaps.TryGetValue("Up", out var upMap))
                return upMap.GetTileWithNeighborFallback(x, upMap.currentMapData.Height + y);

            if (y >= currentMapData.Height && neighborMaps.TryGetValue("Down", out var downMap))
                return downMap.GetTileWithNeighborFallback(x, y - currentMapData.Height);

            return TileType.Black;
        }

        public List<TileRenderInfo> GetViewportTiles(
            int centerX, int centerY, int viewWidth, int viewHeight,
            Direction playerDirection, Direction enemyDirection, int tileWidth, int tileHeight)
        {
            UpdateTrainerVisionOverlay(enemyDirection);

            var tiles = new List<TileRenderInfo>();
            int halfWidth = viewWidth / 2;
            int halfHeight = viewHeight / 2;

            for (int row = 0; row < viewHeight; row++)
            {
                for (int col = 0; col < viewWidth; col++)
                {
                    int mapX = centerX - halfWidth + col;
                    int mapY = centerY - halfHeight + row;

                    var info = new TileRenderInfo
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Color = GetBrushForTile(GetTileWithNeighborFallback(mapX, mapY))
                    };

                    if (mapX == centerX && mapY == centerY)
                        SetPlayerTileRenderInfo(info, playerDirection, tileWidth, tileHeight);

                    tiles.Add(info);
                }
            }

            UpdateTrainerTile(tiles, centerX, centerY, viewWidth, viewHeight, enemyDirection, tileWidth, tileHeight);
            HighlightTrainerDirectionLine(tiles, centerX, centerY, viewWidth, viewHeight, enemyDirection, tileWidth, tileHeight);

            return tiles;
        }

        public void UpdateTrainerTile(List<TileRenderInfo> tiles, int centerX, int centerY, int viewWidth, int viewHeight, Direction enemyDirection, int tileWidth, int tileHeight)
        {

        }

        private void HighlightTrainerDirectionLine(List<TileRenderInfo> tiles, int centerX, int centerY, int viewWidth, int viewHeight, Direction enemyDirection, int tileWidth, int tileHeight)
        {

            
        }

        public void UpdateTrainerVisionOverlay(Direction direction, int trainerVision = 8)
        {
           
        }

        private void SetPlayerTileRenderInfo(TileRenderInfo info, Direction direction, int width, int height)
        {
            info.Color = "Red";
            info.X1 = width / 2;
            info.Y1 = height / 2;

            switch (direction)
            {
                case Direction.Up: info.X2 = info.X1; info.Y2 = 0; break;
                case Direction.Down: info.X2 = info.X1; info.Y2 = height; break;
                case Direction.Left: info.X2 = 0; info.Y2 = info.Y1; break;
                case Direction.Right: info.X2 = width; info.Y2 = info.Y1; break;
            }
        }

        public string GetBrushForTile(TileType type)
        {
            switch (type)
            {
                case TileType.Path:
                    return "Gray";
                case TileType.Grass:
                    return "Green";
                case TileType.Water:
                    return "Blue";
                case TileType.Empty:
                    return "White";
                case TileType.Black:
                    return "Black";
                case TileType.Trainer:
                    return "Yellow";
                case TileType.TrainerVision:
                    return "Pink";
                case TileType.None:
                    throw new NotImplementedException();
                default:
                    return "Red";
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
            public string Color { get; set; }
        }
    }
}
