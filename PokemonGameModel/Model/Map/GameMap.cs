using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Manager;

namespace PokemonGameModel.Model.Map
{
    public class GameMap
    {
        private static GameMap? _instance;
        private static readonly object _lock = new object();

        public Dictionary<string, MapNode> MapNodes { get; } = new();
        public List<(TileTypeFirstLayer, TileTypeSecondLayer)> WorldTiles { get; private set; } = new();
        public int WorldWidth { get; private set; }
        public int WorldHeight { get; private set; }

        private GameMap(MapDataList def)
        {
            BuildGraph(def);
            ComputeOffsets();
            BuildWorldTiles();
        }

        public static GameMap GetInstance(MapDataList def)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new GameMap(def);
                }
            }
            return _instance;
        }

        public static GameMap Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("GameMap is not initialized. Call GetInstance(def) first.");
                return _instance;
            }
        }

        private void BuildGraph(MapDataList def)
        {
            // Create nodes
            foreach (var map in def.maps)
            {
                MapNodes[map.Name] = new MapNode(map);
            }

            // Assign neighbors by name
            foreach (var map in def.maps)
            {
                var node = MapNodes[map.Name];
                if (!string.IsNullOrEmpty(map.LeftMap) && MapNodes.ContainsKey(map.LeftMap))
                    node.Neighbors[Direction.Left] = MapNodes[map.LeftMap];
                if (!string.IsNullOrEmpty(map.RightMap) && MapNodes.ContainsKey(map.RightMap))
                    node.Neighbors[Direction.Right] = MapNodes[map.RightMap];
                if (!string.IsNullOrEmpty(map.UpMap) && MapNodes.ContainsKey(map.UpMap))
                    node.Neighbors[Direction.Up] = MapNodes[map.UpMap];
                if (!string.IsNullOrEmpty(map.DownMap) && MapNodes.ContainsKey(map.DownMap))
                    node.Neighbors[Direction.Down] = MapNodes[map.DownMap];
            }
        }

        private void ComputeOffsets()
        {
            // BFS from central start map
            var visited = new HashSet<MapNode>();
            var queue = new Queue<MapNode>();
            var startNode = MapNodes.Values.First(); // pick first map as origin
            startNode.Offset = (0, 0);
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (!visited.Add(node)) continue;
                var (ox, oy) = node.Offset.Value;

                foreach (var kvp in node.Neighbors)
                {
                    var dir = kvp.Key;
                    var neighbor = kvp.Value;

                    if (neighbor.Offset != null) continue;

                    // Assign offset relative to current map
                    switch (dir)
                    {
                        case Direction.Left:
                            neighbor.Offset = (ox - neighbor.Map.Width, oy);
                            break;
                        case Direction.Right:
                            neighbor.Offset = (ox + node.Map.Width, oy);
                            break;
                        case Direction.Up:
                            neighbor.Offset = (ox, oy - neighbor.Map.Height);
                            break;
                        case Direction.Down:
                            neighbor.Offset = (ox, oy + node.Map.Height);
                            break;
                    }

                    queue.Enqueue(neighbor);
                }
            }
        }

        private void BuildWorldTiles()
        {
            // Calculate min/max for world size
            int minX = MapNodes.Values.Min(n => n.Offset!.Value.X);
            int minY = MapNodes.Values.Min(n => n.Offset!.Value.Y);
            int maxX = MapNodes.Values.Max(n => n.Offset!.Value.X + n.Map.Width);
            int maxY = MapNodes.Values.Max(n => n.Offset!.Value.Y + n.Map.Height);

            WorldWidth = maxX - minX;
            WorldHeight = maxY - minY;

            WorldTiles = Enumerable.Repeat((TileTypeFirstLayer.Empty, TileTypeSecondLayer.None),
                                           WorldWidth * WorldHeight).ToList();

            foreach (var node in MapNodes.Values)
            {
                var ox = node.Offset!.Value.X - minX;
                var oy = node.Offset!.Value.Y - minY;

                foreach (var region in node.Map.Regions)
                {
                    for (int y = 0; y < region.Height; y++)
                    {
                        for (int x = 0; x < region.Width; x++)
                        {
                            int worldX = ox + region.StartX + x;
                            int worldY = oy + region.StartY + y;
                            int index = worldY * WorldWidth + worldX;
                            if (index >= 0 && index < WorldTiles.Count)
                                WorldTiles[index] = (region.Title, TileTypeSecondLayer.None);
                        }
                    }
                }
            }
        }
    }

    public class MapNode
    {
        public MapData Map { get; }
        public Dictionary<Direction, MapNode> Neighbors { get; } = new();
        public (int X, int Y)? Offset { get; set; }

        public MapNode(MapData map) => Map = map;
    }

}
