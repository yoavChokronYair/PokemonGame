using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Data.NpcData;
using PokemonGameModel.Model.Manager;
using System.Windows.Media;

namespace PokemonGame.Model.Map
{
    public class GameMap
    {
        public TileTypeFirstLayer[,,] MultiLayerTiles; // [layer, y, x]


        public MapData currentMapData;
        public TileTypeFirstLayer[][] Tiles { get; set; }
        public string Name { get; set; }

        public Dictionary<string, GameMap> neighborMaps = new Dictionary<string, GameMap>();

        public GameMap(MapData def, HashSet<string> visited = null)
        {
            if (visited == null)
                visited = new HashSet<string>();

            GenerateGameMapFromRegions(def, visited);
        }

        public void GenerateGameMapFromRegions(MapData def, HashSet<string> visited, int trainerVision = 8)
        {
            if (visited.Contains(def.Name)) return;
            visited.Add(def.Name);

            // 2 layers: [0] = base layer (terrain), [1] = overlay (trainers, vision, etc.)
            var tiles = new TileTypeFirstLayer[2, def.Height, def.Width];

            // Initialize base layer to Empty
            for (int y = 0; y < def.Height; y++)
            {
                for (int x = 0; x < def.Width; x++)
                {
                    tiles[0, y, x] = TileTypeFirstLayer.Empty;
                    tiles[1, y, x] = TileTypeFirstLayer.None; // Optional: define `None` for no overlay
                }
            }

            // Populate terrain tiles into base layer
            foreach (var region in def.Regions)
            {
                for (int y = 0; y < region.Height; y++)
                {
                    for (int x = 0; x < region.Width; x++)
                    {
                        int tileX = region.StartX + x;
                        int tileY = region.StartY + y;

                        if (tileX >= 0 && tileX < def.Width && tileY >= 0 && tileY < def.Height)
                            tiles[0, tileY, tileX] = region.Title;
                    }
                }
            }

            // Todo:add trainer
        
            // Save map
            MultiLayerTiles = tiles; // Replace 'Tiles' with 'MultiLayerTiles' (you’ll need to define this in your class)
            Name = def.Name;
            currentMapData = def;

            // Generate neighbors
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

        public TileTypeFirstLayer GetTileAt(int x, int y)
        {
            if (x >= 0 && x < currentMapData.Width && y >= 0 && y < currentMapData.Height)
            {
                var overlay = MultiLayerTiles[1, y, x];
                if (overlay != TileTypeFirstLayer.None)
                    return overlay;

                return MultiLayerTiles[0, y, x];
            }

            return TileTypeFirstLayer.Empty;
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

            // Handle cross-map movement
            if (HandleMapBoundary(ref newX, ref newY, newDirection))
            {
                x = newX;
                y = newY;
                direction = newDirection;
                return true;
            }

            if (IsWithinBounds(newX, newY))
            {
                TileTypeFirstLayer overlay = MultiLayerTiles[1, newY, newX];
                TileTypeFirstLayer baseTile = MultiLayerTiles[0, newY, newX];
                TileTypeFirstLayer tileToUse = overlay != TileTypeFirstLayer.None ? overlay : baseTile;
                if ((int)tileToUse == -2 || tileToUse == TileTypeFirstLayer.Trainer)
                    return false;

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
            if (newMapData != null)
            {
                var visited = new HashSet<string>();
                GenerateGameMapFromRegions(newMapData, visited);
                x = newX;
                y = newY;
                return true;
            }

            return false;
        }

        private bool IsWithinBounds(int x, int y)
        {
            return x >= 0 && x < currentMapData.Width &&
                   y >= 0 && y < currentMapData.Height;
        }
        public List<TileRenderInfo> GetViewportTiles(int centerX, int centerY, int viewWidth, int viewHeight, Direction playerDirection, Direction enemyDirection, int tileWidth, int tileHeight)
        {
            //UpdateTrainerVisionOverlay(enemyDirection);

            var tiles = new List<TileRenderInfo>();
            int halfWidth = viewWidth / 2;
            int halfHeight = viewHeight / 2;

            for (int row = 0; row < viewHeight; row++)
            {
                for (int col = 0; col < viewWidth; col++)
                {
                    int mapX = centerX - halfWidth + col;
                    int mapY = centerY - halfHeight + row;

                    TileTypeFirstLayer tileType = GetTileWithNeighborFallback(mapX, mapY);

                    var info = new TileRenderInfo
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Color = tileType.ToString(),
                        X1 = 0,
                        Y1 = 0,
                        X2 = 0,
                        Y2 = 0
                    };

                    if (mapX == centerX && mapY == centerY)
                    {
                        SetPlayerTileRenderInfo(info, playerDirection, tileWidth, tileHeight);
                    }

                    tiles.Add(info);
                }
            }

            //UpdateTrainerTile(tiles, centerX, centerY, viewWidth, viewHeight, enemyDirection, tileWidth, tileHeight);
            //HighlightTrainerDirectionLine(tiles, centerX, centerY, viewWidth, viewHeight, enemyDirection, tileWidth, tileHeight);

            return tiles;
        }

        //public void UpdateTrainerTile(List<TileRenderInfo> tiles, int centerX, int centerY, int viewWidth, int viewHeight, Direction enemyDirection, int tileWidth, int tileHeight)
        //{
        //    TrainerData trainer = GameDataManager.Instance.TrainerData.trainers
        //        .FirstOrDefault(m => m == Name);

        //    if (trainer == null) return;

        //    int halfWidth = viewWidth / 2;
        //    int halfHeight = viewHeight / 2;

        //    int trainerRelativeX = trainer.StartX - (centerX - halfWidth);
        //    int trainerRelativeY = trainer.StartY - (centerY - halfHeight);

        //    if (trainerRelativeX < 0 || trainerRelativeX >= viewWidth ||
        //        trainerRelativeY < 0 || trainerRelativeY >= viewHeight)
        //        return;

        //    int index = trainerRelativeY * viewWidth + trainerRelativeX;
        //    if (index < 0 || index >= tiles.Count) return;

        //    var info = tiles[index];

        //    TileType trainerTileType = TileTypeFirstLayer.Trainer;
        //    info.Color = GetBrushForTile(trainerTileType);
        //    info.X1 = tileWidth / 2;
        //    info.Y1 += tileHeight / 2;

        //    switch (enemyDirection)
        //    {
        //        case Direction.Up:
        //            info.X2 = info.X1;
        //            info.Y2 = 0;
        //            break;
        //        case Direction.Down:
        //            info.X2 = info.X1;
        //            info.Y2 = tileHeight;
        //            break;
        //        case Direction.Left:
        //            info.X2 = 0;
        //            info.Y2 = info.Y1;
        //            break;
        //        case Direction.Right:
        //            info.X2 = tileWidth;
        //            info.Y2 = info.Y1;
        //            break;
        //    }
        //}

        //private void HighlightTrainerDirectionLine(List<TileRenderInfo> tiles, int centerX, int centerY, int viewWidth, int viewHeight, Direction enemyDirection, int tileWidth, int tileHeight)
        //{
        //    TrainerData trainer = GameDataManager.Instance.TrainerData.trainers
        //        .FirstOrDefault(m => m.Route == Name);

        //    if (trainer == null) return;

        //    int halfWidth = viewWidth / 2;
        //    int halfHeight = viewHeight / 2;

        //    int trainerRelativeX = trainer.StartX - (centerX - halfWidth);
        //    int trainerRelativeY = trainer.StartY - (centerY - halfHeight);

        //    for (int i = 1; i <= 8; i++)
        //    {
        //        int x = trainerRelativeX;
        //        int y = trainerRelativeY;

        //        switch (enemyDirection)
        //        {
        //            case Direction.Up:
        //                y -= i;
        //                break;
        //            case Direction.Down:
        //                y += i;
        //                break;
        //            case Direction.Left:
        //                x -= i;
        //                break;
        //            case Direction.Right:
        //                x += i;
        //                break;
        //        }

        //        if (x < 0 || x >= viewWidth || y < 0 || y >= viewHeight)
        //            break;

        //        int index = y * viewWidth + x;
        //        if (index >= 0 && index < tiles.Count)
        //        {
        //            // Draw based on the overlay tile
        //            int mapX = centerX - halfWidth + x;
        //            int mapY = centerY - halfHeight + y;

        //            if (mapX >= 0 && mapX < currentMapData.Width &&
        //                mapY >= 0 && mapY < currentMapData.Height)
        //            {
        //                var overlay = MultiLayerTiles[1, mapY, mapX];
        //                if (overlay == TileType.TrainerVision)
        //                {
        //                    tiles[index].Color = GetBrushForTile(TileType.TrainerVision);
        //                }
        //            }
        //        }
        //    }
        //}
        //public void UpdateTrainerVisionOverlay(Direction direction, int trainerVision = 8)
        //{
        //    // Clear old trainer vision
        //    for (int y = 0; y < currentMapData.Height; y++)
        //    {
        //        for (int x = 0; x < currentMapData.Width; x++)
        //        {
        //            if (MultiLayerTiles[1, y, x] == TileType.TrainerVision)
        //            {
        //                MultiLayerTiles[1, y, x] = TileTypeFirstLayer.None;
        //            }
        //        }
        //    }

        //    // Find the trainer
        //    TrainerData trainer = GameDataManager.Instance.TrainerData.trainers
        //        .FirstOrDefault(m => m.Route == Name);
        //    if (trainer == null) return;

        //    int startX = trainer.StartX;
        //    int startY = trainer.StartY;

        //    for (int i = 1; i <= trainerVision; i++)
        //    {
        //        int x = startX;
        //        int y = startY;

        //        switch (direction)
        //        {
        //            case Direction.Up: y -= i; break;
        //            case Direction.Down: y += i; break;
        //            case Direction.Left: x -= i; break;
        //            case Direction.Right: x += i; break;
        //        }

        //        if (x < 0 || x >= currentMapData.Width || y < 0 || y >= currentMapData.Height)
        //            break;

        //        MultiLayerTiles[1, y, x] = TileType.TrainerVision;
        //    }
        //}

        private void SetPlayerTileRenderInfo(TileRenderInfo info, Direction playerDirection, int tileWidth, int tileHeight)
        {
            info.Color = "Red";
            info.X1 = tileWidth / 2;
            info.Y1 = tileHeight / 2;

            switch (playerDirection)
            {
                case Direction.Up:
                    info.X2 = info.X1;
                    info.Y2 = 0;
                    break;
                case Direction.Down:
                    info.X2 = info.X1;
                    info.Y2 = tileHeight;
                    break;
                case Direction.Left:
                    info.X2 = 0;
                    info.Y2 = info.Y1;
                    break;
                case Direction.Right:
                    info.X2 = tileWidth;
                    info.Y2 = info.Y1;
                    break;
            }
        }

        public TileTypeFirstLayer GetTileWithNeighborFallback(int x, int y)
        {
            // If inside current map bounds, return current map tile (overlay if exists)
            if (x >= 0 && x < currentMapData.Width && y >= 0 && y < currentMapData.Height)
            {
                var overlay = MultiLayerTiles[1, y, x];
                if (overlay != TileTypeFirstLayer.None)
                    return overlay;

                return MultiLayerTiles[0, y, x];
            }

            // Otherwise, check neighbors depending on which boundary is crossed
            if (x < 0 && neighborMaps.TryGetValue("Left", out var leftMap))
            {
                // Translate x,y to left map's local coords
                int neighborX = leftMap.currentMapData.Width + x; // since x<0, adding width shifts inside neighbor map
                int neighborY = y;
                if (neighborX >= 0 && neighborX < leftMap.currentMapData.Width &&
                    neighborY >= 0 && neighborY < leftMap.currentMapData.Height)
                    return leftMap.GetTileWithNeighborFallback(neighborX, neighborY);
            }
            else if (x >= currentMapData.Width && neighborMaps.TryGetValue("Right", out var rightMap))
            {
                int neighborX = x - currentMapData.Width;
                int neighborY = y;
                if (neighborX >= 0 && neighborX < rightMap.currentMapData.Width &&
                    neighborY >= 0 && neighborY < rightMap.currentMapData.Height)
                    return rightMap.GetTileWithNeighborFallback(neighborX, neighborY);
            }
            else if (y < 0 && neighborMaps.TryGetValue("Up", out var upMap))
            {
                int neighborX = x;
                int neighborY = upMap.currentMapData.Height + y;
                if (neighborX >= 0 && neighborX < upMap.currentMapData.Width &&
                    neighborY >= 0 && neighborY < upMap.currentMapData.Height)
                    return upMap.GetTileWithNeighborFallback(neighborX, neighborY);
            }
            else if (y >= currentMapData.Height && neighborMaps.TryGetValue("Down", out var downMap))
            {
                int neighborX = x;
                int neighborY = y - currentMapData.Height;
                if (neighborX >= 0 && neighborX < downMap.currentMapData.Width &&
                    neighborY >= 0 && neighborY < downMap.currentMapData.Height)
                    return downMap.GetTileWithNeighborFallback(neighborX, neighborY);
            }

            // If no neighbor or outside bounds in neighbor map, return black tile (or Empty)
            return TileTypeFirstLayer.Black;
        }

        public Brush GetBrushForTile(TileTypeFirstLayer type)
        {
            switch (type)
            {
                case TileTypeFirstLayer.Path: return Brushes.Gray;
                case TileTypeFirstLayer.Grass: return Brushes.Green;
                case TileTypeFirstLayer.Water: return Brushes.Blue;
                case TileTypeFirstLayer.Empty: return Brushes.White;
                case TileTypeFirstLayer.Black: return Brushes.Black;
                case TileTypeFirstLayer.Trainer: return Brushes.Yellow;
                case TileTypeFirstLayer.Interactable: return Brushes.Pink;
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
            public string Color { get; set; }
        }
    }
}
