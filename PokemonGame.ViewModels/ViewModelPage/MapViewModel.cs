using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PokemonGame.Model.Domain.Dialogue;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Npc;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Model.Model.Map;
using PokemonGame.Services.Data.Map;
using PokemonGame.Services.Services;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.Dialogue;
using PokemonGame.ViewModels.ViewModelPage.Map;
using PokemonGame.ViewModels.ViewModelPage.Map.Command;

namespace PokemonGame.ViewModels.ViewModelPage
{
    public sealed class MapLoader
    {
        private readonly IMapService _mapService;
        private readonly Dictionary<int, MapDomain> _cache = new Dictionary<int, MapDomain>();


        // Call this inside your Move command after updating the player location
        // to tell the XAML to refresh the coordinates shown in the UI
  
        public MapLoader(IMapService mapService)
        {
            _mapService = mapService;
        }

        public MapDomain Load(string mapName)
        {
            _cache.Clear();
            var bundle = _mapService.GetMap(mapName)
                ?? throw new InvalidOperationException($"Map '{mapName}' not found.");
            return BuildDomain(bundle);
        }

        private MapDomain BuildDomain(MapBundle bundle)
        {
            if (_cache.TryGetValue(bundle.Map.Id, out var existing))
                return existing;

            var domain = new MapDomain
            {
                Name = bundle.Map.Name,
                Width = bundle.Map.Width,
                Height = bundle.Map.Height,
                // Visual layers — tile layers are draw-only
                BackgroundBlocks = BuildTiles(bundle.Tiles, TileLayerType.Ground),
                Blocks = BuildTiles(bundle.Tiles, TileLayerType.Objects),
                ConnectedMaps = new List<ConnectedMapDomain>(),
                Wraps = new List<WrapDomain>(),
                Npc = new List<NpcObjectDomain>(),
                // Collision comes from object layers, not tile IDs
                CollisionObjects = BuildCollisionObjects(bundle.Collisions),
            };

            _cache[bundle.Map.Id] = domain;

            foreach (var conn in bundle.Connections)
            {
                var nb = _mapService.GetMap(conn.ConnectedMapId);
                if (nb == null) continue;
                domain.ConnectedMaps.Add(new ConnectedMapDomain
                {
                    ConnectedMap = BuildDomain(nb),
                    ConnectionDirection = (ConnectionDirection)conn.Direction,
                    Margin = conn.Margin,
                });
            }

            foreach (var wrap in bundle.Wraps)
            {
                var tb = _mapService.GetMap(wrap.TargetMapId);
                if (tb == null) continue;
                domain.Wraps.Add(new WrapDomain
                {
                    WrapLoc = (wrap.WrapX, wrap.WrapY),
                    TargetMap = BuildDomain(tb),
                    SpawnLoc = (wrap.SpawnRow, wrap.SpawnCol),
                });
            }

            foreach (var spawn in bundle.NpcSpawns)
                domain.Npc.Add(BuildNpc(spawn));

            return domain;
        }

        // ── Tile layers (visual only) ─────────────────────────────────────────

        private enum TileLayerType { Ground = 0, Water = 1, Objects = 2, Above = 3 }

        private static List<TileDomain> BuildTiles(
            IReadOnlyList<MapTileData> tiles, TileLayerType layer)
        {
            var result = new List<TileDomain>();
            foreach (var t in tiles)
            {
                if (t.LayerType != (int)layer) continue;
                result.Add(new TileDomain { Tileid = t.TileId });
            }
            return result;
        }

        // ── Collision objects ─────────────────────────────────────────────────

        private static List<CollisionObjectDomain> BuildCollisionObjects(
            IReadOnlyList<MapCollisionObjectData> rows)
        {
            var result = new List<CollisionObjectDomain>(rows.Count);
            foreach (var r in rows)
            {
                result.Add(new CollisionObjectDomain
                {
                    X = r.X,
                    Y = r.Y,
                    Width = r.Width,
                    Height = r.Height,
                    CollisionType = (CollisionType)r.CollisionType,
                });
            }
            return result;
        }

        // ── NPC ───────────────────────────────────────────────────────────────

        private static NpcObjectDomain BuildNpc(NpcSpawnData spawn)
        {
            return new NpcObjectDomain
            {
                NpcInfo = new NpcDomain { Id = spawn.NpcId },
                Location = (spawn.X, spawn.Y),
                CollisionType = (CollisionType)spawn.CollisionType,
                MovementType = (MovementType)spawn.MovementType,
                direction = (FacingDirection)spawn.FacingDirection,
                DirectionA = (FacingDirection)spawn.DirectionA,
                DirectionB = (FacingDirection)spawn.DirectionB,
                StepsPerLeg = spawn.StepsPerLeg,
                visionRange = spawn.VisionRange,
                VisionType = (VisionType)spawn.VisionType,
            };
        }
    }
    public class MapViewModel : ViewModelBase
    {
        private readonly MapManager _mapManager;
        private readonly PlayerDomain _player;
        private readonly MapLoader _mapLoader;

        // --- Caching ---
        private readonly Dictionary<string, BitmapImage> _tilesetCache = new();
        private readonly Dictionary<int, ImageSource> _tileSliceCache = new();

        // --- State Fields ---
        private string _collisionAtCursor = string.Empty;
        private string _lastMoveResult = string.Empty;
        private string _inspectResult = string.Empty;
        private bool _isShowingBackground = true;
        private bool _isShowingForeground;
        private TileCellViewModel? _currentPlayerCell;
        private NpcObjectDomain? _activeNpc;

        // --- Sub-ViewModels & Properties ---
        public DialogueViewModel Dialogue { get; } = new DialogueViewModel();
        private SquareMapState SquareMap => _mapManager.SquareMap;
        public ObservableCollection<TileRowViewModel> TileRows { get; } = new ObservableCollection<TileRowViewModel>();

        public string MapName => _mapManager.ActiveMap.Name;
        public int MapWidth => _mapManager.ActiveMap.Width;
        public int MapHeight => _mapManager.ActiveMap.Height;
        public int SquareRows => SquareMap.SquareRows;
        public int SquareCols => SquareMap.SquareCols;
        public string FacingText => _player.FacingDirection.ToString();
        public int PlayerSquareRow => _player.playerLoc.x;
        public int PlayerSquareCol => _player.playerLoc.y;
        public string CollisionAtCursor
        {
            get => _collisionAtCursor;
            private set => SetProperty(ref _collisionAtCursor, value);
        }
        public string LastMoveResult
        {
            get => _lastMoveResult;
            private set => SetProperty(ref _lastMoveResult, value);
        }
        public string InspectResult
        {
            get => _inspectResult;
            private set => SetProperty(ref _inspectResult, value);
        }

        public bool IsShowingBackground
        {
            get => _isShowingBackground;
            private set => SetProperty(ref _isShowingBackground, value);
        }
        public bool IsShowingForeground
        {
            get => _isShowingForeground;
            private set => SetProperty(ref _isShowingForeground, value);
        }

        // --- Commands ---
        private Action? _focusCallback;
        public void RegisterFocusCallback(Action focus) => _focusCallback = focus;

        public ShowLayerCommand ShowBackgroundCommand { get; }
        public ShowLayerCommand ShowForegroundCommand { get; }
        public MoveCommand MoveUpCommand { get; }
        public MoveCommand MoveDownCommand { get; }
        public MoveCommand MoveLeftCommand { get; }
        public MoveCommand MoveRightCommand { get; }
        public InspectCommand InspectCommand { get; }

        public MapViewModel()
        {
            _player = PlayerDomain.Instance;
            _mapLoader = new MapLoader(new MapService());

            // Initial Bootstrapping
            _player.CurrentMap = _mapLoader.Load("Pallet Town");
            if (_player.playerLoc == default) _player.playerLoc = (14, 12);

            _mapManager = new MapManager(_player);
            _mapManager.TrainerSpotted += OnPlayerSpotted;
            _mapManager.NpcInteracted += OnNpcInteracted;

            // Command Setup
            ShowBackgroundCommand = new ShowLayerCommand(this, background: true);
            ShowForegroundCommand = new ShowLayerCommand(this, background: false);
            InspectCommand = new InspectCommand(this);
            MoveUpCommand = new MoveCommand(this, FacingDirection.Up);
            MoveDownCommand = new MoveCommand(this, FacingDirection.Down);
            MoveLeftCommand = new MoveCommand(this, FacingDirection.Left);
            MoveRightCommand = new MoveCommand(this, FacingDirection.Right);

            Dialogue.FocusRequested += () => _focusCallback?.Invoke();

            // Clock / NPC Ticking
            ClockManager.Instance.NpcTick += (_, _) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mapManager.TickNpcs();
                    RefreshNpcs();
                });
            };

            Dialogue.DialogueOpened += () => ClockManager.Instance.Pause();
            Dialogue.DialogueClosed += () =>
            {
                ClockManager.Instance.Resume();
                if (_activeNpc != null)
                {
                    _mapManager.OnNpcDialogueFinished(_activeNpc);
                    _activeNpc = null;
                    RefreshNpcs();
                }
            };

            ClockManager.Instance.Start();
            RebuildGrid();
        }

        // --- Image Slicing Logic ---
        private ImageSource? GetImageSource(int tileId)
        {
            if (tileId <= 0) return null;
            if (_tileSliceCache.TryGetValue(tileId, out var cached)) return cached;

            // 1. Path to your tileset
            var activeMap = _mapManager.ActiveMap;
            string path = $"pack://application:,,,/Assets/Tilesets/{activeMap.Name}.png";

            if (!_tilesetCache.TryGetValue(path, out var masterSheet))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    _tilesetCache[path] = masterSheet = bitmap;
                }
                catch { return null; }
            }

            // 2. GRID CALCULATION (No Metadata Required)
            // Adjust these two numbers to match your PNG layout
            int tilePixelSize = 8;
            int tilesPerRow = (int)960 / tilePixelSize;

            // Calculate X and Y based on ID (assuming ID 0 is first tile)
            int id = tileId - 1; // Adjusting to 0-based index if your IDs start at 1
            int x = (id % tilesPerRow) * tilePixelSize;
            int y = (id / tilesPerRow) * tilePixelSize;

            try
            {
                var rect = new Int32Rect(x, y, tilePixelSize, tilePixelSize);
                var slice = new CroppedBitmap(masterSheet, rect);
                slice.Freeze();
                _tileSliceCache[tileId] = slice;
                return slice;
            }
            catch { return null; }
        }

        // --- High Performance Rendering Methods ---
        public void RebuildGrid()
        {
            _tileSliceCache.Clear();

            var (bg, fg, vision) = _mapManager.GetViewport();
            var layer = _isShowingBackground ? bg : fg;

            // Get the actual bounds of the returned viewport arrays
            int viewportRows = layer.GetLength(0);
            int viewportCols = layer.GetLength(1);

            // 1. Structure: Ensure TileRows matches the VIEWPORT size, not the whole map
            if (TileRows.Count != viewportRows || (TileRows.Count > 0 && TileRows[0].Cells.Count != viewportCols))
            {
                TileRows.Clear();
                for (int r = 0; r < viewportRows; r++)
                {
                    var row = new TileRowViewModel();
                    for (int c = 0; c < viewportCols; c++)
                    {
                        row.Cells.Add(new TileCellViewModel());
                    }
                    TileRows.Add(row);
                }
            }

            // 2. Populate
            var (playerSr, playerSc) = SquareMap.TileToSquare(_player.playerLoc.x, _player.playerLoc.y);

            for (int r = 0; r < viewportRows; r++)
            {
                for (int c = 0; c < viewportCols; c++)
                {
                    var cell = TileRows[r].Cells[c];

                    // Safe access using the loops bounds (viewportRows/Cols)
                    cell.Row = r;
                    cell.Col = c;
                    cell.TileId = layer[r, c];
                    cell.TileImage = GetImageSource(cell.TileId);

                    // Use vision layer safely

                    // Map-specific data (Collision/NPCs)
                    var square = SquareMap.GetSquare(r, c);
                    cell.Collision = square?.SquareType ?? CollisionType.Blocked;
                    cell.IsPlayerHere = (r == playerSr && c == playerSc);
                    cell.NpcId = NpcIdAt(r, c);

                    if (cell.IsPlayerHere) _currentPlayerCell = cell;
                }
            }

            NotifyHeaderProperties();
        }

        public void Move(FacingDirection direction)
        {
            if (Dialogue.IsOpen) return;

            var mapBefore = _mapManager.ActiveMap;
            var result = _mapManager.TryMove(direction);

            if (result.Success)
            {
                LastMoveResult = $"Moved {direction}";

                if (_mapManager.ActiveMap != mapBefore)
                {
                    RebuildGrid(); // Map changed, wipe and redraw
                }
                else
                {
                    Refresh(); // Just update markers and NPCs
                }
            }
            else
            {
                LastMoveResult = $"Blocked ({direction})";
                OnPropertyChanged(nameof(FacingText));
            }
        }

        public void Refresh()
        {
            UpdatePlayerMarker();
            RefreshNpcs();
            OnPropertyChanged(nameof(FacingText));
        }

        public void RefreshNpcs()
        {
            var vision = SquareMap.VisionLayer;
            for (int r = 0; r < TileRows.Count; r++)
            {
                for (int c = 0; c < TileRows[r].Cells.Count; c++)
                {
                    var cell = TileRows[r].Cells[c];
                    cell.NpcId = NpcIdAt(r, c);

                    if (r < vision.GetLength(0) && c < vision.GetLength(1))
                        cell.NpcVisionId = vision[r, c];
                }
            }
        }

        private void UpdatePlayerMarker()
        {
            if (_currentPlayerCell != null)
                _currentPlayerCell.IsPlayerHere = false;

            var (sr, sc) = SquareMap.TileToSquare(_player.playerLoc.x, _player.playerLoc.y);

            if (sr < TileRows.Count && sc < TileRows[sr].Cells.Count)
            {
                _currentPlayerCell = TileRows[sr].Cells[sc];
                _currentPlayerCell.IsPlayerHere = true;
                CollisionAtCursor = SquareMap.GetCollision(sr, sc).ToString();
            }
        }

        private void NotifyHeaderProperties()
        {
            OnPropertyChanged(nameof(MapName));
            OnPropertyChanged(nameof(MapWidth));
            OnPropertyChanged(nameof(MapHeight));
            OnPropertyChanged(nameof(SquareRows));
            OnPropertyChanged(nameof(SquareCols));
            OnPropertyChanged(nameof(FacingText));
            OnPropertyChanged(nameof(PlayerSquareRow));
            OnPropertyChanged(nameof(PlayerSquareCol));
        }

        private int NpcIdAt(int squareRow, int squareCol)
        {
            var npc = _mapManager.ActiveMap.Npc.FirstOrDefault(n =>
            {
                var (r, c) = SquareMap.TileToSquare(n.Location.x, n.Location.y);
                return r == squareRow && c == squareCol;
            });
            return npc?.NpcInfo.Id ?? 0;
        }

        public void Inspect()
        {
            if (Dialogue.IsOpen) { Dialogue.Advance(); return; }

            _mapManager.TryInteractWithNpc();
            if (Dialogue.IsOpen) return;

            var result = _mapManager.TryInspect();
            InspectResult = result.Message;

            if (result.Type == InspectResultType.ItemPickup || result.Type == InspectResultType.HmUsed)
                UpdateTileCollision(result.TargetRow, result.TargetCol, CollisionType.None);

            if (result.Type == InspectResultType.NpcDialogue && result.DialogueSet != null)
                Dialogue.Open(result.DialogueSet, result.NpcName);
        }

        private void UpdateTileCollision(int squareRow, int squareCol, CollisionType collision)
        {
            if (squareRow < TileRows.Count && squareCol < TileRows[squareRow].Cells.Count)
                TileRows[squareRow].Cells[squareCol].Collision = collision;
        }

        internal void SwitchLayer(bool background)
        {
            IsShowingBackground = background;
            IsShowingForeground = !background;
            RebuildGrid();
        }

        private void OnPlayerSpotted(NpcObjectDomain npc)
        {
            if (Dialogue.IsOpen) return;
            var set = npc.NpcInfo.GetDialogue(TriggerType.Spotted);
            if (set != null) Dialogue.Open(set, npc.NpcInfo.Name ?? string.Empty);
        }

        private void OnNpcInteracted(NpcObjectDomain npc)
        {
            if (Dialogue.IsOpen) return;
            var set = npc.NpcInfo.GetDialogue(TriggerType.Interact);
            if (set != null)
            {
                _activeNpc = npc;
                Dialogue.Open(set, npc.NpcInfo.Name ?? string.Empty);
            }
        }
    }
}
