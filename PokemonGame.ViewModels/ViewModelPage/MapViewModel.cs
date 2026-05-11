using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Npc;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.Model.Model.Map;
using PokemonGame.Services.Data.Map;
using PokemonGame.Services.Services;
using PokemonGame.ViewModels.ViewModelPage.Dialogue;
using PokemonGame.ViewModels.ViewModelPage.Map.Command;
using PokemonGame.Model.Model.Managers;

namespace PokemonGame.ViewModels.ViewModelPage
{
    public interface IFocusTarget
    {
        void RegisterFocusCallback(Action focus);
    }

    // -------------------------------------------------------------------------
    // MapLoader  (unchanged)
    // -------------------------------------------------------------------------
    public sealed class MapLoader
    {
        private readonly IMapService _mapService;
        private static readonly Dictionary<string, MapDomain> _sessionCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, MapDomain> _cycleCache = new();

        public MapLoader(IMapService mapService) => _mapService = mapService;

        public MapDomain Load(string mapName)
        {
            if (_sessionCache.TryGetValue(mapName, out var cached)) return cached;
            _cycleCache.Clear();
            var bundle = _mapService.GetMap(mapName)
                ?? throw new InvalidOperationException($"Map '{mapName}' not found.");
            var domain = BuildDomain(bundle);
            _sessionCache[mapName] = domain;
            return domain;
        }

        public static void InvalidateCache(string mapName) => _sessionCache.Remove(mapName);
        public static void InvalidateAll() => _sessionCache.Clear();

        private MapDomain BuildDomain(MapBundle bundle)
        {
            if (_cycleCache.TryGetValue(bundle.Map.Id, out var existing)) return existing;

            var domain = new MapDomain
            {
                Name = bundle.Map.Name,
                Width = bundle.Map.Width,
                Height = bundle.Map.Height,
                FlyWrapLoc = (bundle.Map.FlyWrapX, bundle.Map.FlyWrapY),
                TownMapLoc = (bundle.Map.TownMapX, bundle.Map.TownMapY),
                BackgroundBlocks = BuildTiles(bundle.Tiles, TileLayerType.Ground),
                Blocks = BuildTiles(bundle.Tiles, TileLayerType.Objects),
                CollisionObjects = BuildCollisionObjects(bundle.Collisions),
                ConnectedMaps = new List<ConnectedMapDomain>(),
                Wraps = new List<WrapDomain>(),
                Npc = new List<NpcObjectDomain>(),
            };

            _cycleCache[bundle.Map.Id] = domain;

            foreach (var conn in bundle.Connections)
            {
                var nb = _mapService.GetMap(conn.ConnectedMapId);
                if (nb == null) continue;
                if (!Enum.IsDefined(typeof(ConnectionDirection), conn.Direction))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MapLoader] Skipping connection id={conn.Id}: unknown Direction={conn.Direction}");
                    continue;
                }
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

        private enum TileLayerType { Ground = 0, Water = 1, Objects = 2, Above = 3 }

        private static List<TileDomain> BuildTiles(IReadOnlyList<MapTileData> tiles, TileLayerType layer)
        {
            var result = new List<TileDomain>();
            foreach (var t in tiles)
            {
                if (t.LayerType != (int)layer) continue;
                result.Add(new TileDomain { Tileid = t.TileId, X = t.X, Y = t.Y });
            }
            return result;
        }

        private static List<CollisionObjectDomain> BuildCollisionObjects(IReadOnlyList<MapCollisionObjectData> rows)
        {
            var result = new List<CollisionObjectDomain>(rows.Count);
            foreach (var r in rows)
            {
                if (!Enum.IsDefined(typeof(CollisionType), r.CollisionType))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MapLoader] Skipping collision id={r.Id}: unknown CollisionType={r.CollisionType}");
                    continue;
                }
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

        private static NpcObjectDomain BuildNpc(NpcSpawnData spawn)
        {
            static T SafeCast<T>(int value, T fallback, string field, int spawnId)
                where T : struct, Enum
            {
                if (Enum.IsDefined(typeof(T), value)) return (T)(object)value;
                System.Diagnostics.Debug.WriteLine(
                    $"[MapLoader] NpcSpawn id={spawnId}: unknown {field}={value}, using {fallback}");
                return fallback;
            }

            return new NpcObjectDomain
            {
                NpcInfo = new NpcDomain { Id = spawn.NpcId },
                Location = (spawn.X, spawn.Y),
                CollisionType = SafeCast(spawn.CollisionType, CollisionType.Blocked, nameof(spawn.CollisionType), spawn.Id),
                MovementType = SafeCast(spawn.MovementType, MovementType.Stationary, nameof(spawn.MovementType), spawn.Id),
                direction = SafeCast(spawn.FacingDirection, FacingDirection.Down, nameof(spawn.FacingDirection), spawn.Id),
                DirectionA = SafeCast(spawn.DirectionA, FacingDirection.Down, nameof(spawn.DirectionA), spawn.Id),
                DirectionB = SafeCast(spawn.DirectionB, FacingDirection.Up, nameof(spawn.DirectionB), spawn.Id),
                StepsPerLeg = spawn.StepsPerLeg,
                visionRange = spawn.VisionRange,
                VisionType = SafeCast(spawn.VisionType, VisionType.Normal, nameof(spawn.VisionType), spawn.Id),
            };
        }
    }

    // =========================================================================
    // RangeObservableCollection  (unchanged)
    // =========================================================================
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        public void Reset(IEnumerable<T> newItems)
        {
            Items.Clear();
            foreach (var item in newItems)
                Items.Add(item);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    // =========================================================================
    // CanvasOverlayItem  (unchanged)
    // =========================================================================
    public class CanvasOverlayItem
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public bool IsPlayer { get; set; }
        public bool IsNpc { get; set; }
        public bool IsTrainer { get; set; }
        public bool IsVision { get; set; }
        public bool HasCollision { get; set; }
        public string CollisionColor { get; set; } = "Transparent";
        public string NpcSymbol { get; set; }
        public string Tooltip { get; set; }
        public bool IsDebug { get; set; }
        public string DebugText { get; set; }
        public string DebugTintColor { get; set; } = "Transparent";
    }

    // =========================================================================
    // MapViewModel
    // =========================================================================
    public class MapViewModel : ViewModelBase, IDisposable, IFocusTarget
    {
        // One cell = one SQUARE (2×2 tiles = 16px in PNG → 72px on screen, scale 4.5×)
        public const double CellPx = 72.0;
        private const int MapTilePx = 8;

        // Viewport in squares (derived once — all internal logic uses these)
        private static readonly int ViewSqCols = MapConstants.ViewColSize / MapConstants.TilesPerSquare; // 24/2 = 12
        private static readonly int ViewSqRows = MapConstants.ViewRowSize / MapConstants.TilesPerSquare; // 35/2 = 17
        // Center square index (0-based). 17→8, 12→6
        private static readonly int HalfSqRows = ViewSqRows / 2;
        private static readonly int HalfSqCols = ViewSqCols / 2;

        private static readonly string PlayerSpritePath =
            @"C:\Users\yoav\source\repos\PokemonGame\PokemonGame\Assets\Images\Player\";

        // ── Fields ───────────────────────────────────────────────────────────
        private readonly PlayerDomain _player;
        private readonly MapLoader _mapLoader;
        private MapManager _mapManager;

        private readonly Dictionary<string, BitmapImage> _mapImageCache = new();
        private readonly Dictionary<string, ImageSource> _spriteCache = new();
        private readonly Dictionary<(int, int, int, int), CroppedBitmap> _cropCache = new();
        private Dictionary<(int row, int col), int> _npcSquareMap = new();

        private bool _disposed;
        private bool _pendingOverlayRebuild;

        private EventHandler _npcTickHandler;
        private EventHandler _playerTickHandler;
        private readonly MovementSate _movement = new MovementSate();
        private Action _dialogueOpenedHandler;
        private Action _dialogueClosedHandler;

        // ── Observable state ─────────────────────────────────────────────────
        private ImageSource _mapImageSource;
        public ImageSource MapImageSource
        {
            get => _mapImageSource;
            private set => SetProperty(ref _mapImageSource, value);
        }

        private double _imageDisplayWidth;
        private double _imageDisplayHeight;
        private double _imageOffsetX;
        private double _imageOffsetY;
        public double ImageDisplayWidth  { get => _imageDisplayWidth;  private set => SetProperty(ref _imageDisplayWidth,  value); }
        public double ImageDisplayHeight { get => _imageDisplayHeight; private set => SetProperty(ref _imageDisplayHeight, value); }
        public double ImageOffsetX       { get => _imageOffsetX;       private set => SetProperty(ref _imageOffsetX,       value); }
        public double ImageOffsetY       { get => _imageOffsetY;       private set => SetProperty(ref _imageOffsetY,       value); }

        private ImageSource _playerImage;
        public ImageSource PlayerImage
        {
            get => _playerImage;
            private set => SetProperty(ref _playerImage, value);
        }

        // Player sprite sits at the center square of the canvas
        public double PlayerPixelX => HalfSqCols * CellPx;
        public double PlayerPixelY => HalfSqRows * CellPx;

        private bool _isReady;
        public bool IsReady { get => _isReady; private set => SetProperty(ref _isReady, value); }

        private bool _isDebugMode;
        public bool IsDebugMode { get => _isDebugMode; set => SetProperty(ref _isDebugMode, value); }

        private string _collisionAtCursor = string.Empty;
        private string _lastMoveResult    = string.Empty;
        private string _inspectResult     = string.Empty;
        private bool   _isShowingBackground = true;
        private bool   _isShowingForeground;
        private NpcObjectDomain _activeNpc;

        public string CollisionAtCursor { get => _collisionAtCursor; private set => SetProperty(ref _collisionAtCursor, value); }
        public string LastMoveResult    { get => _lastMoveResult;    private set => SetProperty(ref _lastMoveResult,    value); }
        public string InspectResult     { get => _inspectResult;     private set => SetProperty(ref _inspectResult,     value); }
        public bool IsShowingBackground { get => _isShowingBackground; private set => SetProperty(ref _isShowingBackground, value); }
        public bool IsShowingForeground { get => _isShowingForeground; private set => SetProperty(ref _isShowingForeground, value); }

        // ── Computed header properties ────────────────────────────────────────
        private SquareMapState SquareMap => _mapManager.SquareMap;
        public string MapName   => _mapManager?.ActiveMap.Name   ?? string.Empty;
        public int    MapWidth  => _mapManager?.ActiveMap.Width  ?? 0;
        public int    MapHeight => _mapManager?.ActiveMap.Height ?? 0;
        public int    SquareRows => _mapManager != null ? SquareMap.SquareRows : 0;
        public int    SquareCols => _mapManager != null ? SquareMap.SquareCols : 0;
        public string FacingText      => _player.FacingDirection.ToString();
        public int    PlayerSquareRow => _mapManager != null
            ? SquareMap.TileToSquare(_player.playerLoc.y, _player.playerLoc.x).row : 0;
        public int    PlayerSquareCol => _mapManager != null
            ? SquareMap.TileToSquare(_player.playerLoc.y, _player.playerLoc.x).col : 0;

        // ── Overlay snapshot ──────────────────────────────────────────────────
        private IReadOnlyList<CanvasOverlayItem> _overlaySnapshot = Array.Empty<CanvasOverlayItem>();
        public IReadOnlyList<CanvasOverlayItem> OverlaySnapshot
        {
            get => _overlaySnapshot;
            private set => SetProperty(ref _overlaySnapshot, value);
        }

        // ── Dialogue ─────────────────────────────────────────────────────────
        public DialogueViewModel Dialogue { get; } = new();

        // ── Viewport dimensions ───────────────────────────────────────────────
        // Canvas size = number of square cells × pixels-per-cell
        public double ViewportWidthPx  => ViewSqCols * CellPx;   // 12 × 72 = 864
        public double ViewportHeightPx => ViewSqRows * CellPx;   // 17 × 72 = 1224

        // ── Commands ─────────────────────────────────────────────────────────
        public ShowLayerCommand ShowBackgroundCommand { get; }
        public ShowLayerCommand ShowForegroundCommand { get; }
        public MoveCommand      MoveUpCommand    { get; }
        public MoveCommand      MoveDownCommand  { get; }
        public MoveCommand      MoveLeftCommand  { get; }
        public MoveCommand      MoveRightCommand { get; }
        public InspectCommand   InspectCommand   { get; }
        public ICommand ToggleDebugCommand { get; }
        public ICommand PickChoice1Command { get; }
        public ICommand PickChoice2Command { get; }
        public ICommand PickChoice3Command { get; }

        private Action _focusCallback;
        public void RegisterFocusCallback(Action focus) => _focusCallback = focus;

        // ── Constructor ───────────────────────────────────────────────────────
        public MapViewModel()
        {
            _player     = PlayerDomain.Instance;
            _mapLoader  = new MapLoader(new MapService());

            ShowBackgroundCommand = new ShowLayerCommand(this, background: true);
            ShowForegroundCommand = new ShowLayerCommand(this, background: false);
            InspectCommand        = new InspectCommand(this);
            MoveUpCommand         = new MoveCommand(this, FacingDirection.Up);
            MoveDownCommand       = new MoveCommand(this, FacingDirection.Down);
            MoveLeftCommand       = new MoveCommand(this, FacingDirection.Left);
            MoveRightCommand      = new MoveCommand(this, FacingDirection.Right);
            ToggleDebugCommand    = new RelayCommand(() => ToggleDebug());
            PickChoice1Command    = new RelayCommand(() => Dialogue.PickChoice(0));
            PickChoice2Command    = new RelayCommand(() => Dialogue.PickChoice(1));
            PickChoice3Command    = new RelayCommand(() => Dialogue.PickChoice(2));

            Dialogue.FocusRequested += () => _focusCallback?.Invoke();

            _ = InitializeAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    System.Diagnostics.Debug.WriteLine("InitializeAsync failed: " + t.Exception);
            });
        }

        public void Initialize() => _ = InitializeAsync();

        private async Task InitializeAsync()
        {
            MapDomain startMap = await Task.Run(() => _mapLoader.Load("Pallet Town"));

            _player.CurrentMap = startMap;
            if (_player.playerLoc == default)
                _player.playerLoc = (12, 14);

            _mapManager = new MapManager(_player);
            _mapManager.TrainerSpotted  += OnPlayerSpotted;
            _mapManager.NpcInteracted   += OnNpcInteracted;

            _npcTickHandler = (_, _) =>
            {
                Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    if (_disposed) return;
                    _mapManager.TickNpcs();
                    RebuildNpcMap();
                    if (!_pendingOverlayRebuild)
                        RefreshOverlays();
                });
            };

            _playerTickHandler = (_, _) =>
            {
                Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    if (_disposed) return;
                    proccessMovementTick();
                });
            };

            _dialogueOpenedHandler = () => ClockManager.Instance.Pause();

            _dialogueClosedHandler = () =>
            {
                ClockManager.Instance.Resume();
                if (_activeNpc != null)
                {
                    _mapManager.OnNpcDialogueFinished(_activeNpc);
                    _activeNpc = null;
                    RebuildNpcMap();
                }
                _pendingOverlayRebuild = true;
                RefreshOverlays();
                _pendingOverlayRebuild = false;
            };

            ClockManager.Instance.NpcTick    += _npcTickHandler;
            ClockManager.Instance.PlayerTick += _playerTickHandler;
            Dialogue.DialogueOpened          += _dialogueOpenedHandler;
            Dialogue.DialogueClosed          += _dialogueClosedHandler;

            ClockManager.Instance.Start();
            RebuildGrid();
            IsReady = true;
        }

        // ── IDisposable ───────────────────────────────────────────────────────
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_playerTickHandler   != null) ClockManager.Instance.PlayerTick -= _playerTickHandler;
            if (_npcTickHandler      != null) ClockManager.Instance.NpcTick    -= _npcTickHandler;
            if (_dialogueOpenedHandler != null) Dialogue.DialogueOpened        -= _dialogueOpenedHandler;
            if (_dialogueClosedHandler != null) Dialogue.DialogueClosed        -= _dialogueClosedHandler;

            if (_mapManager != null)
            {
                _mapManager.TrainerSpotted -= OnPlayerSpotted;
                _mapManager.NpcInteracted  -= OnNpcInteracted;
            }

            ClockManager.Instance.Stop();
        }

        // ── Movement ─────────────────────────────────────────────────────────
        public void Move(FacingDirection direction)
        {
            if (Dialogue.IsOpen) return;
            _movement.QueuedDirection = (int)direction;
            _movement.HasQueued = true;
        }

        private void proccessMovementTick()
        {
            if (Dialogue.IsOpen) return;
            if (!_movement.HasQueued) return;
            _movement.HasQueued = false;
            var direction = (FacingDirection)_movement.QueuedDirection;
            _player.IsMoving = true;
            _player.AdvanceAnimation();

            var result = _mapManager.TryMove(direction);
            if (result.Success)
            {
                LastMoveResult = $"Moved {direction}";
                RebuildGrid();
                if (result.WildEncounterTriggered) LastMoveResult += " + Wild Encounter!";
                if (result.SpottedByNpcId != 0)   LastMoveResult += $" + Spotted by NPC {result.SpottedByNpcId}!";
            }
            else
            {
                _player.IsMoving = false;
                LastMoveResult = $"Blocked moving {direction}: {result.SquareType}";
                RefreshOverlays();
            }
        }

        // ── Player sprite ─────────────────────────────────────────────────────
        private ImageSource LoadSprite(string filename)
        {
            string fullPath = PlayerSpritePath + _player.Gender.ToString() + @"\" + filename;
            if (_spriteCache.TryGetValue(fullPath, out var cached)) return cached;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource    = new Uri(fullPath, UriKind.Absolute);
                bmp.CacheOption  = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                _spriteCache[fullPath] = bmp;
                return bmp;
            }
            catch { return null; }
        }

        // ── Map bitmap ────────────────────────────────────────────────────────
        private BitmapImage GetMapBitmap()
        {
            string path = @"file:///C:/Users/yoav/source/repos/PokemonGame/PokemonGame.ViewModels/ViewModelPage/PalletTown.png";
            if (_mapImageCache.TryGetValue(path, out var cached)) return cached;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource   = new Uri(path, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                _mapImageCache[path] = bmp;
                return bmp;
            }
            catch { return null; }
        }

        // ── Map image crop ────────────────────────────────────────────────────
        // The PNG is in tile-space (8px per tile).
        // The canvas is in square-space (CellPx=72 per square = 2 tiles = 16px in PNG).
        // We crop ViewSqRows*2 × ViewSqCols*2 tiles centred on the player,
        // then stretch that crop to fill the canvas exactly.
        private void UpdateMapImageSource()
        {
            var sheet = GetMapBitmap();
            if (sheet == null) { MapImageSource = null; return; }

            int tps = MapConstants.TilesPerSquare; // 2

            // How many tiles the viewport covers
            int viewTileCols = ViewSqCols * tps;   // 12 × 2 = 24
            int viewTileRows = ViewSqRows * tps;   // 17 × 2 = 34

            // Top-left tile of the viewport (may be negative near map edges)
            // playerLoc.x = tile col, playerLoc.y = tile row
            int originTileCol = _player.playerLoc.x - viewTileCols / 2;  // centre col
            int originTileRow = _player.playerLoc.y - viewTileRows / 2;  // centre row

            // Convert to PNG pixel space
            int px = originTileCol * MapTilePx;
            int py = originTileRow * MapTilePx;
            int pw = viewTileCols  * MapTilePx;   // 24 × 8 = 192 px
            int ph = viewTileRows  * MapTilePx;   // 34 × 8 = 272 px

            int imgW = sheet.PixelWidth;
            int imgH = sheet.PixelHeight;

            // How many pixels of the viewport are off the top/left edge of the PNG
            int offsetX = Math.Max(0, -px);
            int offsetY = Math.Max(0, -py);

            int cropX = Math.Max(0, px);
            int cropY = Math.Max(0, py);
            int cropW = Math.Min(pw - offsetX, imgW - cropX);
            int cropH = Math.Min(ph - offsetY, imgH - cropY);

            if (cropW <= 0 || cropH <= 0)
            {
                MapImageSource = null;
                ImageDisplayWidth = ImageDisplayHeight = ImageOffsetX = ImageOffsetY = 0;
                return;
            }

            // scale: 1 tile in PNG = MapTilePx px; 1 square on canvas = CellPx px
            // 1 square = tps tiles → scale = CellPx / (tps * MapTilePx) = 72/16 = 4.5
            double scale = CellPx / (tps * MapTilePx);

            var key = (cropX, cropY, cropW, cropH);
            if (!_cropCache.TryGetValue(key, out var crop))
            {
                try
                {
                    crop = new CroppedBitmap(sheet, new Int32Rect(cropX, cropY, cropW, cropH));
                    crop.Freeze();
                    _cropCache[key] = crop;
                }
                catch { MapImageSource = null; return; }
            }

            MapImageSource     = crop;
            ImageDisplayWidth  = cropW   * scale;
            ImageDisplayHeight = cropH   * scale;
            ImageOffsetX       = offsetX * scale;
            ImageOffsetY       = offsetY * scale;
        }

        // ── NPC square map ────────────────────────────────────────────────────
        private void RebuildNpcMap()
        {
            _npcSquareMap.Clear();
            foreach (var npc in _mapManager.ActiveMap.Npc)
            {
                var (r, c) = SquareMap.TileToSquare(npc.Location.y, npc.Location.x);
                _npcSquareMap[(r, c)] = npc.NpcInfo.Id;
            }
        }

        // ── Grid rebuild ──────────────────────────────────────────────────────
        // Loops over SQUARES (not tiles). One overlay cell = one square = CellPx wide.
        public void RebuildGrid()
        {
            var (bg, fg, _, playerSprite) = _mapManager.GetViewport();

            if (playerSprite != null)
                PlayerImage = LoadSprite(playerSprite.ImagePath);

            RebuildNpcMap();
            UpdateMapImageSource();

            var vl = SquareMap.VisionLayer;

            // Player's current map square
            var (psr, psc) = SquareMap.TileToSquare(_player.playerLoc.y, _player.playerLoc.x);

            var cellData = new List<(int sqRow, int sqCol, bool isPlayer,
                                     int npcId, int visionId, CollisionType col)>(ViewSqRows * ViewSqCols);

            for (int r = 0; r < ViewSqRows; r++)
            {
                for (int c = 0; c < ViewSqCols; c++)
                {
                    // Absolute map square this viewport cell represents
                    int mapSqRow = psr - HalfSqRows + r;
                    int mapSqCol = psc - HalfSqCols + c;

                    _npcSquareMap.TryGetValue((mapSqRow, mapSqCol), out int npcId);

                    // VisionLayer is map-absolute square coords — index directly
                    int visionId = 0;
                    if ((uint)mapSqRow < (uint)vl.GetLength(0) &&
                        (uint)mapSqCol < (uint)vl.GetLength(1))
                        visionId = vl[mapSqRow, mapSqCol];

                    cellData.Add((
                        mapSqRow, mapSqCol,
                        r == HalfSqRows && c == HalfSqCols,  // true only at player's square
                        npcId,
                        visionId,
                        SquareMap.GetSquare(mapSqRow, mapSqCol)?.SquareType ?? CollisionType.None
                    ));
                }
            }

            RebuildOverlaysFromData(cellData, ViewSqRows, ViewSqCols);

            CollisionAtCursor = SquareMap.GetCollision(psr, psc).ToString();
            NotifyHeaderProperties();
        }

        // ── Overlay rebuild ───────────────────────────────────────────────────
        private void RebuildOverlaysFromData(
            List<(int sqRow, int sqCol, bool isPlayer, int npcId, int visionId, CollisionType col)> cellData,
            int viewRows, int viewCols)
        {
            var newItems = new List<CanvasOverlayItem>(viewRows * viewCols * 2);

            for (int r = 0; r < viewRows; r++)
            {
                for (int c = 0; c < viewCols; c++)
                {
                    var (sqRow, sqCol, isPlayer, npcId, visionId, collision) = cellData[r * viewCols + c];
                    bool isNpc    = npcId    != 0;
                    bool isVision = visionId != 0;

                    string tooltip = $"[{sqRow},{sqCol}]  {collision}" +
                                     (isNpc    ? $"  NPC:{npcId}"        : string.Empty) +
                                     (isVision ? $"  seen-by:{visionId}" : string.Empty);

                    double left = c * CellPx;
                    double top  = r * CellPx;

                    var (colColor, showCol) = CollisionDebugColor(collision);
                    if (showCol)
                        newItems.Add(new CanvasOverlayItem
                        {
                            Left = left, Top = top,
                            HasCollision  = true,
                            CollisionColor = colColor,
                            Tooltip = tooltip,
                        });

                    if (isVision && !isPlayer && !isNpc)
                        newItems.Add(new CanvasOverlayItem
                        {
                            Left = left, Top = top,
                            IsVision = true,
                            Tooltip  = tooltip,
                        });

                    if (isPlayer)
                        newItems.Add(new CanvasOverlayItem
                        {
                            Left = left, Top = top,
                            IsPlayer = true,
                        });

                    if (isNpc)
                        newItems.Add(new CanvasOverlayItem
                        {
                            Left = left, Top = top,
                            IsNpc      = true,
                            IsTrainer  = npcId % 2 != 0,
                            NpcSymbol  = npcId % 2 != 0 ? "T" : "N",
                            Tooltip    = tooltip,
                        });

                    if (_isDebugMode)
                        newItems.Add(new CanvasOverlayItem
                        {
                            Left = left, Top = top,
                            IsDebug        = true,
                            DebugText      = $"{sqRow},{sqCol}",
                            DebugTintColor = CollisionToDebugColor(collision),
                        });
                }
            }

            OverlaySnapshot = newItems;
        }

        // ── Overlay refresh (NPC tick / dialogue close — no image rebuild) ────
        public void RefreshOverlays()
        {
            var vl  = SquareMap.VisionLayer;
            var (psr, psc) = SquareMap.TileToSquare(_player.playerLoc.y, _player.playerLoc.x);

            var cellData = new List<(int sqRow, int sqCol, bool isPlayer,
                                     int npcId, int visionId, CollisionType col)>(ViewSqRows * ViewSqCols);

            for (int r = 0; r < ViewSqRows; r++)
            {
                for (int c = 0; c < ViewSqCols; c++)
                {
                    int mapSqRow = psr - HalfSqRows + r;
                    int mapSqCol = psc - HalfSqCols + c;

                    _npcSquareMap.TryGetValue((mapSqRow, mapSqCol), out int npcId);

                    int visionId = 0;
                    if ((uint)mapSqRow < (uint)vl.GetLength(0) &&
                        (uint)mapSqCol < (uint)vl.GetLength(1))
                        visionId = vl[mapSqRow, mapSqCol];

                    cellData.Add((
                        mapSqRow, mapSqCol,
                        r == HalfSqRows && c == HalfSqCols,
                        npcId,
                        visionId,
                        SquareMap.GetSquare(mapSqRow, mapSqCol)?.SquareType ?? CollisionType.None
                    ));
                }
            }

            RebuildOverlaysFromData(cellData, ViewSqRows, ViewSqCols);
        }

        // ── Actions ───────────────────────────────────────────────────────────
        public void ToggleDebug()
        {
            IsDebugMode = !IsDebugMode;
            RebuildGrid();
        }

        public void Inspect()
        {
            if (Dialogue.IsOpen) { Dialogue.Advance(); return; }

            _mapManager.TryInteractWithNpc();
            if (Dialogue.IsOpen) return;

            var result = _mapManager.TryInspect();
            InspectResult = result.Message;

            if (result.Type == InspectResultType.ItemPickup ||
                result.Type == InspectResultType.HmUsed)
                RebuildGrid();

            if (result.Type == InspectResultType.NpcDialogue && result.DialogueSet != null)
                Dialogue.Open(result.DialogueSet, result.NpcName);
        }

        internal void SwitchLayer(bool background)
        {
            IsShowingBackground = background;
            IsShowingForeground = !background;
            RebuildGrid();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
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

        private static (string color, bool show) CollisionDebugColor(CollisionType c) => c switch
        {
            CollisionType.Blocked   => ("#99FF2222", true),
            CollisionType.WildGrass => ("#9922CC44", true),
            CollisionType.HM        => ("#992255FF", true),
            CollisionType.JumpLeft  => ("#99FFCC00", true),
            CollisionType.JumpRight => ("#99FFCC00", true),
            CollisionType.JumpUp    => ("#99FFCC00", true),
            CollisionType.JumpDown  => ("#99FFCC00", true),
            CollisionType.None      => (string.Empty, false),
            _                       => ("#99FF00FF", true),
        };

        private static string CollisionToDebugColor(CollisionType c) => c switch
        {
            CollisionType.Blocked   => "#55FF0000",
            CollisionType.WildGrass => "#5500FF00",
            CollisionType.HM        => "#550000FF",
            CollisionType.JumpLeft or CollisionType.JumpRight
                or CollisionType.JumpDown or CollisionType.JumpUp => "#55FFFF00",
            _ => "#00000000",
        };

        private sealed class MovementSate
        {
            public bool HasQueued;
            public int  QueuedDirection;
        }
    }
}