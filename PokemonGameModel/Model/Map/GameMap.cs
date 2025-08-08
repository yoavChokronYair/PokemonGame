using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Manager;
using System.ComponentModel;

namespace PokemonGameModel.Model.Map
{
    public class GameMap
    {
        private static GameMap? _instance;
        private static readonly object _lock = new object();

        public readonly Dictionary<MapData, List<string?>> _data = new Dictionary<MapData, List<string?>>();
        public readonly Dictionary<MapData, List<(TileTypeFirstLayer, TileTypeSecondLayer)>> mapTiles = new Dictionary<MapData, List<(TileTypeFirstLayer, TileTypeSecondLayer)>>();
        public List<(TileTypeFirstLayer, TileTypeSecondLayer)> _tiles = new List<(TileTypeFirstLayer, TileTypeSecondLayer)>();
        public readonly Dictionary<MapData, List<(TileTypeFirstLayer, TileTypeSecondLayer)>> baseMapTiles
   = new Dictionary<MapData, List<(TileTypeFirstLayer, TileTypeSecondLayer)>>();
        // Private constructor so no one can "new" it directly
        private GameMap(MapDataList def)
        {
            foreach (MapData map in def.maps)
            {
                List<string?> l = new List<string?>() { map.UpMap, map.DownMap, map.LeftMap, map.RightMap };
                _data.Add(map, l);
                GenerateGameMapFromRegions(map);
                mapTiles.Add(map, _tiles);
                baseMapTiles.Add(map, new List<(TileTypeFirstLayer, TileTypeSecondLayer)>(_tiles)); // clean copy
                _tiles = new List<(TileTypeFirstLayer, TileTypeSecondLayer)>();
            }
        }

        /// <summary>
        /// Initialize the singleton with data (only first call will take effect)
        /// </summary>
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

        /// <summary>
        /// Get the already created instance (throws if not yet initialized)
        /// </summary>
        public static GameMap Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("GameMap is not initialized. Call GetInstance(def) first.");
                return _instance;
            }
        }

        public void GenerateGameMapFromRegions(MapData def)
        {
            // Initialize all tiles
            for (int y = 0; y < def.Height; y++)
            {
                for (int x = 0; x < def.Width; x++)
                {
                    _tiles.Add((TileTypeFirstLayer.Empty, TileTypeSecondLayer.None));
                }
            }
            // Fill terrain tiles
            foreach (var region in def.Regions)
            {
                for (int y = 0; y < region.Height; y++)
                {
                    for (int x = 0; x < region.Width; x++)
                    {
                        int tileX = region.StartX + x;
                        int tileY = region.StartY + y;

                        if (tileX >= 0 && tileX < def.Width && tileY >= 0 && tileY < def.Height)
                            _tiles[tileY * def.Width + tileX] = (region.Title, _tiles[tileY * def.Width + tileX].Item2);

                        if (_tiles[tileY * def.Width + tileX].Item1 == TileTypeFirstLayer.Trainer)
                        {
                            _tiles[tileY * def.Width + tileX] = (_tiles[tileY * def.Width + tileX].Item1, TileTypeSecondLayer.Interactable);
                            Direction? direction = GameDataManager.Instance.TrainerData.trainers.FirstOrDefault(m => m.Id == region.ID)?.FirstDirection;
                            SetTrainerVision(direction, tileX, tileY, def);
                        }
                    }
                }
            }
        }

        public void SetTrainerVision(Direction? direction, int startX, int startY, MapData def)
        {
            if (direction == null)
                return;

            int dx = 0, dy = 0;

            switch (direction)
            {
                case Direction.Left:
                    dx = -1;
                    break;
                case Direction.Right:
                    dx = 1;
                    break;
                case Direction.Up:
                    dy = -1;
                    break;
                case Direction.Down:
                    dy = 1;
                    break;
                default:
                    return;
            }

            for (int step = 1; step <= 8; step++)
            {
                int x = startX + dx * step;
                int y = startY + dy * step;

                int index = y * def.Width + x;

                if (x < 0 || y < 0 || x >= def.Width || y >= def.Height || index < 0)
                    break;

                _tiles[index] = (_tiles[index].Item1, TileTypeSecondLayer.Event);
            }
        }
        public List<string> ConvertToColor(MapData map)
        {
            List<string> l = new List<string>();
            var tiles = mapTiles[map];

            foreach (var tile in tiles)
            {
                // Player takes priority — overrides base color
                if (tile.Item2 == TileTypeSecondLayer.player)
                {
                    l.Add("Red");
                    continue;
                }

                // Base color mapping for first layer
                switch (tile.Item1)
                {
                    case TileTypeFirstLayer.Empty:
                        l.Add("White");
                        break;
                    case TileTypeFirstLayer.Path:
                        l.Add("Gray");
                        break;
                    case TileTypeFirstLayer.Grass:
                        l.Add("Green");
                        break;
                    case TileTypeFirstLayer.Water:
                        l.Add("Blue");
                        break;
                    case TileTypeFirstLayer.Black:
                        l.Add("Black");
                        break;
                    case TileTypeFirstLayer.Trainer:
                        l.Add("Yellow");
                        break;
                    default:
                        l.Add("Magenta"); // fallback color if unknown
                        break;
                }
            }

            return l;
        }
    }
}
