using System.Collections.ObjectModel;
<<<<<<< HEAD
using System.Collections.Specialized;
=======
using System.Numerics;
>>>>>>> ffedec0895c70be5f6563ca012858e64cb30befe
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
<<<<<<< HEAD
using CommunityToolkit.Mvvm.Input;
=======
using Microsoft;
using Microsoft.ServiceHub.Resources;
>>>>>>> ffedec0895c70be5f6563ca012858e64cb30befe
using PokemonGame.Model.Config;
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
    // -------------------------------------------------------------------------
    // MapLoader  (unchanged)
    // -------------------------------------------------------------------------
    public sealed class MapLoader
    {
        private readonly IMapService _mapService;

        // ── Bug #5 fix (Low): Cache keyed on map NAME (string), not MapDomain ──
        // reference. The old _cache was Dictionary<int, MapDomain> keyed on
        // bundle.Map.Id and lived only for the duration of one Load() call
        // (cleared at the top). MapState._mapCache was keyed on MapDomain object
        // reference — every Load() produced new instances so the cache never hit.
        //
        // Fix: promote the name→domain cache to a static session-level cache so
        // the same MapDomain instance is returned for repeated loads of the same
        // map, making MapState._mapCache (object-reference keyed) actually work.
        private static readonly Dictionary<string, MapDomain> _sessionCache = new(StringComparer.OrdinalIgnoreCase);

        // Per-load cycle cache (id→domain) prevents infinite recursion on
        // circular connections within a single Load() call.
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

        /// <summary>Call when map data may have changed (e.g. after saving in editor).</summary>
        public static void InvalidateCache(string mapName) => _sessionCache.Remove(mapName);
        public static void InvalidateAll() => _sessionCache.Clear();

        private MapDomain BuildDomain(MapBundle bundle)
        {
            if (_cycleCache.TryGetValue(bundle.Map.Id, out var existing)) return existing;

            // ── Bug #3 fix (Medium): Domain added to cache BEFORE ConnectedMaps ──
            // are populated. If map A connects to map B which connects back to A,
            // BuildDomain(B) calls BuildDomain(A) which found A in the cache but
            // with an empty ConnectedMaps list. Now we register in _cycleCache
            // immediately (to break cycles) but defer adding to _sessionCache
            // until after all children are populated — done in Load() above after
            // BuildDomain() returns fully constructed.

            var domain = new MapDomain
            {
                Name = bundle.Map.Name,
                Width = bundle.Map.Width,
                Height = bundle.Map.Height,

                // ── Bug #4 fix (Low): FlyWrapLoc and TownMapLoc never populated ─
                // The old code never read FlyWrapX/Y or TownMapX/Y from the bundle.
                FlyWrapLoc = (bundle.Map.FlyWrapX, bundle.Map.FlyWrapY),
                TownMapLoc = (bundle.Map.TownMapX, bundle.Map.TownMapY),

                BackgroundBlocks = BuildTiles(bundle.Tiles, TileLayerType.Ground),
                Blocks = BuildTiles(bundle.Tiles, TileLayerType.Objects),
                CollisionObjects = BuildCollisionObjects(bundle.Collisions),
                ConnectedMaps = new List<ConnectedMapDomain>(),
                Wraps = new List<WrapDomain>(),
                Npc = new List<NpcObjectDomain>(),
            };

            // Register in cycle-breaker cache immediately so recursive calls see it.
            _cycleCache[bundle.Map.Id] = domain;

            // Now populate children — any back-edges will hit the cycle cache above.
            foreach (var conn in bundle.Connections)
            {
                var nb = _mapService.GetMap(conn.ConnectedMapId);
                if (nb == null) continue;

                // ── Bug #1 fix (Critical): validate Direction before casting ────
                if (!Enum.IsDefined(typeof(ConnectionDirection), conn.Direction))
                {
                    // Log and skip rather than producing a garbage enum member
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

        private static List<CollisionObjectDomain> BuildCollisionObjects(
            IReadOnlyList<MapCollisionObjectData> rows)
        {
            var result = new List<CollisionObjectDomain>(rows.Count);
            foreach (var r in rows)
            {
                // ── Bug #1 fix: validate CollisionType before casting ───────────
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
            // ── Bug #1 fix: validate all enum fields from DB ───────────────────
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
        // RangeObservableCollection
        // Replaces 1500 individual Add() notifications with a single Reset —
        // one WPF layout pass per move instead of one per overlay item.
        // =========================================================================
        public class RangeObservableCollection<T> : ObservableCollection<T>
        {
            public void Reset(IEnumerable<T> newItems)
            {
                Items.Clear();
                foreach (var item in newItems)
                    Items.Add(item);

                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));
            }
        }

    // =========================================================================
    // RelayCommand
    // =========================================================================


    // =========================================================================
    // CanvasOverlayItem
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
        public string? NpcSymbol { get; set; }
        public string? Tooltip { get; set; }
        public bool IsDebug { get; set; }
        public string? DebugText { get; set; }
        public string DebugTintColor { get; set; } = "Transparent";
    }

    // =========================================================================
    // MapViewModel
    // =========================================================================
    public class MapViewModel : ViewModelBase, IDisposable
        {
            public const double CellPx = 36.0;
            private const int MapTilePx = 8;

            // ── Fields ───────────────────────────────────────────────────────────
            private readonly PlayerDomain _player;
            private readonly MapLoader _mapLoader;
            private MapManager? _mapManager;    // null until InitializeAsync completes

            private readonly Dictionary<string, BitmapImage> _mapImageCache = new();
            private Dictionary<(int row, int col), int> _npcSquareMap = new();

            private bool _disposed;
            private bool _pendingOverlayRebuild;

            // Stored so we can unsubscribe (prevents memory leak / zombie ticks)
            private EventHandler? _npcTickHandler;
            private Action? _dialogueOpenedHandler;
            private Action? _dialogueClosedHandler;

<<<<<<< HEAD
            // ── Observable state ─────────────────────────────────────────────────
            private ImageSource? _mapImageSource;
            public ImageSource? MapImageSource
            {
                get => _mapImageSource;
                private set => SetProperty(ref _mapImageSource, value);
            }

            private double _imageDisplayWidth;
            private double _imageDisplayHeight;
            public double ImageDisplayWidth { get => _imageDisplayWidth; private set => SetProperty(ref _imageDisplayWidth, value); }
            public double ImageDisplayHeight { get => _imageDisplayHeight; private set => SetProperty(ref _imageDisplayHeight, value); }
=======
        public string CollisionAtCursor { get => _collisionAtCursor; private set => SetProperty(ref _collisionAtCursor, value); }
        public string LastMoveResult { get => _lastMoveResult; private set => SetProperty(ref _lastMoveResult, value); }
        public string InspectResult { get => _inspectResult; private set => SetProperty(ref _inspectResult, value); }
        public bool IsShowingBackground { get => _isShowingBackground; private set => SetProperty(ref _isShowingBackground, value); }
        public bool IsShowingForeground { get => _isShowingForeground; private set => SetProperty(ref _isShowingForeground, value); }
        // In MapViewModel.cs
        private ImageSource? _playerImage;
        public ImageSource? PlayerImage
        {
            get => _playerImage;
            private set => SetProperty(ref _playerImage, value);
        }

        public double PlayerPixelX => 20 + (MapConstants.ViewColSize / 2) * 36.0 - 18; // 20=padding, 18=center of tile
        public double PlayerPixelY => 20 + (MapConstants.ViewRowSize / 2) * 36.0 - 36; // shift up for 24px sprite (scaled to 72)
        // ── Commands ─────────────────────────────────────────────────────────────
        private Action? _focusCallback;
        public void RegisterFocusCallback(Action focus) => _focusCallback = focus;
>>>>>>> ffedec0895c70be5f6563ca012858e64cb30befe

            private bool _isReady;
            public bool IsReady { get => _isReady; private set => SetProperty(ref _isReady, value); }

            private bool _isDebugMode;
            public bool IsDebugMode { get => _isDebugMode; set => SetProperty(ref _isDebugMode, value); }

            private string _collisionAtCursor = string.Empty;
            private string _lastMoveResult = string.Empty;
            private string _inspectResult = string.Empty;
            private bool _isShowingBackground = true;
            private bool _isShowingForeground;
            private NpcObjectDomain? _activeNpc;

            public string CollisionAtCursor { get => _collisionAtCursor; private set => SetProperty(ref _collisionAtCursor, value); }
            public string LastMoveResult { get => _lastMoveResult; private set => SetProperty(ref _lastMoveResult, value); }
            public string InspectResult { get => _inspectResult; private set => SetProperty(ref _inspectResult, value); }
            public bool IsShowingBackground { get => _isShowingBackground; private set => SetProperty(ref _isShowingBackground, value); }
            public bool IsShowingForeground { get => _isShowingForeground; private set => SetProperty(ref _isShowingForeground, value); }

            // ── Computed header properties ────────────────────────────────────────
            private SquareMapState SquareMap => _mapManager!.SquareMap;
            public string MapName => _mapManager?.ActiveMap.Name ?? string.Empty;
            public int MapWidth => _mapManager?.ActiveMap.Width ?? 0;
            public int MapHeight => _mapManager?.ActiveMap.Height ?? 0;
            public int SquareRows => _mapManager != null ? SquareMap.SquareRows : 0;
            public int SquareCols => _mapManager != null ? SquareMap.SquareCols : 0;
            public string FacingText => _player.FacingDirection.ToString();
            public int PlayerSquareRow => _mapManager != null
                ? SquareMap.TileToSquare(_player.playerLoc.y, _player.playerLoc.x).row : 0;
            public int PlayerSquareCol => _mapManager != null
                ? SquareMap.TileToSquare(_player.playerLoc.y, _player.playerLoc.x).col : 0;

            private IReadOnlyList<CanvasOverlayItem> _overlaySnapshot = Array.Empty<CanvasOverlayItem>();
            public IReadOnlyList<CanvasOverlayItem> OverlaySnapshot
            {
                get => _overlaySnapshot;
                private set => SetProperty(ref _overlaySnapshot, value);
            }
        // ── Dialogue ─────────────────────────────────────────────────────────
        public DialogueViewModel Dialogue { get; } = new();

            // ── Viewport dimensions ───────────────────────────────────────────────
            public double ViewportWidthPx => MapConstants.ViewColSize * CellPx;
            public double ViewportHeightPx => MapConstants.ViewRowSize * CellPx;

            // ── Commands ─────────────────────────────────────────────────────────
            public ShowLayerCommand ShowBackgroundCommand { get; }
            public ShowLayerCommand ShowForegroundCommand { get; }
            public MoveCommand MoveUpCommand { get; }
            public MoveCommand MoveDownCommand { get; }
            public MoveCommand MoveLeftCommand { get; }
            public MoveCommand MoveRightCommand { get; }
            public InspectCommand InspectCommand { get; }
            public ICommand ToggleDebugCommand { get; }
            public ICommand PickChoice1Command { get; }
            public ICommand PickChoice2Command { get; }
            public ICommand PickChoice3Command { get; }

            private Action? _focusCallback;
            public void RegisterFocusCallback(Action focus) => _focusCallback = focus;

        // ── Constructor — cheap shell only, no DB calls ───────────────────────
        public MapViewModel()
        {
            _player = PlayerDomain.Instance;
            _mapLoader = new MapLoader(new MapService());

            ShowBackgroundCommand = new ShowLayerCommand(this, background: true);
            ShowForegroundCommand = new ShowLayerCommand(this, background: false);
            InspectCommand = new InspectCommand(this);
            MoveUpCommand = new MoveCommand(this, FacingDirection.Up);
            MoveDownCommand = new MoveCommand(this, FacingDirection.Down);
            MoveLeftCommand = new MoveCommand(this, FacingDirection.Left);
            MoveRightCommand = new MoveCommand(this, FacingDirection.Right);
            ToggleDebugCommand = new RelayCommand(() => ToggleDebug());
            PickChoice1Command = new RelayCommand(() => Dialogue.PickChoice(0));
            PickChoice2Command = new RelayCommand(() => Dialogue.PickChoice(1));
            PickChoice3Command = new RelayCommand(() => Dialogue.PickChoice(2));

            Dialogue.FocusRequested += () => _focusCallback?.Invoke();

            // Kick off async load immediately — Task.Run moves the DB work off the
            // UI thread, so the constructor returns fast and the window stays responsive.
            _ = InitializeAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    System.Diagnostics.Debug.WriteLine("InitializeAsync failed: " + t.Exception);
            });
        }

        // ── Async init — called by InitializeBehavior on Page Loaded ─────────
        public void Initialize() => _ = InitializeAsync();

            private async Task InitializeAsync()
            {
                // DB load off UI thread so window never freezes
                MapDomain startMap = await Task.Run(() => _mapLoader.Load("Pallet Town"));

                // Back on UI thread for all WPF/state work
                _player.CurrentMap = startMap;
                if (_player.playerLoc == default)
                    _player.playerLoc = (12, 14);  // x=tileCol, y=tileRow

                _mapManager = new MapManager(_player);
                _mapManager.TrainerSpotted += OnPlayerSpotted;
                _mapManager.NpcInteracted += OnNpcInteracted;

                // Store handlers so we can unsubscribe in Dispose()
                _npcTickHandler = (_, _) =>
                {
                    // BeginInvoke (async) avoids deadlock if ClockManager uses DispatcherTimer
                    Application.Current?.Dispatcher.BeginInvoke(() =>
                    {
                        if (_disposed) return;
                        _mapManager.TickNpcs();
                        RebuildNpcMap();
                        // Skip if DialogueClosed is already doing a rebuild this frame
                        if (!_pendingOverlayRebuild)
                            RefreshOverlays();
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
                    // Single authoritative rebuild; flag prevents tick from doubling it
                    _pendingOverlayRebuild = true;
                    RefreshOverlays();
                    _pendingOverlayRebuild = false;
                };

                ClockManager.Instance.NpcTick += _npcTickHandler;
                Dialogue.DialogueOpened += _dialogueOpenedHandler;
                Dialogue.DialogueClosed += _dialogueClosedHandler;

                ClockManager.Instance.Start();
                RebuildGrid();
                IsReady = true;
            }

            // ── IDisposable ───────────────────────────────────────────────────────
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                if (_npcTickHandler != null)
                    ClockManager.Instance.NpcTick -= _npcTickHandler;
                if (_dialogueOpenedHandler != null)
                    Dialogue.DialogueOpened -= _dialogueOpenedHandler;
                if (_dialogueClosedHandler != null)
                    Dialogue.DialogueClosed -= _dialogueClosedHandler;

                if (_mapManager != null)
                {
                    _mapManager.TrainerSpotted -= OnPlayerSpotted;
                    _mapManager.NpcInteracted -= OnNpcInteracted;
                }

                ClockManager.Instance.Stop();
            }

            // ── Bitmap ───────────────────────────────────────────────────────────
            private BitmapImage? GetMapBitmap()
            {
                string path = @"file:///C:/Users/yoav/source/repos/PokemonGame/PokemonGame.ViewModels/ViewModelPage/PalletTown.png";
                if (_mapImageCache.TryGetValue(path, out var cached)) return cached;
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(path, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    _mapImageCache[path] = bmp;
                    return bmp;
                }
                catch { return null; }
            }

            private void UpdateMapImageSource()
            {
                var sheet = GetMapBitmap();
                if (sheet == null) { MapImageSource = null; return; }

                int viewCols = MapConstants.ViewColSize;
                int viewRows = MapConstants.ViewRowSize;

                // playerLoc: x=tileCol, y=tileRow
                int startTileRow = _player.playerLoc.y - viewRows / 2;
                int startTileCol = _player.playerLoc.x - viewCols / 2;

                int px = startTileCol * MapTilePx;
                int py = startTileRow * MapTilePx;
                int pw = viewCols * MapTilePx;
                int ph = viewRows * MapTilePx;

                int imgW = sheet.PixelWidth;
                int imgH = sheet.PixelHeight;
                px = Math.Max(0, Math.Min(px, imgW - 1));
                py = Math.Max(0, Math.Min(py, imgH - 1));
                pw = Math.Min(pw, imgW - px);
                ph = Math.Min(ph, imgH - py);

                if (pw <= 0 || ph <= 0) { MapImageSource = null; return; }

<<<<<<< HEAD
                try
=======
        // ── RebuildGrid — called on move and map change ───────────────────────────
        // Does NOT allocate new cells. Does NOT clear the tile slice cache.
        // Just writes new values into the existing TileCellViewModel objects.
        private static readonly string PlayerSpritePath =
            @"C:\Users\yoav\source\repos\PokemonGame\PokemonGame\Assets\Images\Player\";

        private ImageSource? LoadSprite(string filename)
        {
            PlayerDomain.Instance.Gender = Gender.Female;
            string fullPath = PlayerSpritePath + PlayerDomain.Instance.Gender.ToString() + @"\" + filename;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }
        public void RebuildGrid()
        {
            var (bg, fg, _, playerSprite) = _mapManager.GetViewport();
            PlayerImage = LoadSprite(playerSprite.ImagePath);
            var tileLayer = _isShowingBackground ? bg : fg;

            int viewportRows = tileLayer.GetLength(0); // Vertical
            int viewportCols = tileLayer.GetLength(1); // Horizontal
            int tps = MapConstants.TilesPerSquare;

            int halfRows = viewportRows / 2;
            int halfCols = viewportCols / 2;

            RebuildNpcMap();
            var vl = SquareMap.VisionLayer;

            for (int r = 0; r < viewportRows; r++)
            {
                for (int c = 0; c < viewportCols; c++)
>>>>>>> ffedec0895c70be5f6563ca012858e64cb30befe
                {
                    var crop = new CroppedBitmap(sheet, new Int32Rect(px, py, pw, ph));
                    crop.Freeze();
                    MapImageSource = crop;
                    double scale = CellPx / MapTilePx;
                    ImageDisplayWidth = pw * scale;
                    ImageDisplayHeight = ph * scale;
                }
                catch { MapImageSource = null; ImageDisplayWidth = 0; ImageDisplayHeight = 0; }
            }

            // ── NPC square map ────────────────────────────────────────────────────
            private void RebuildNpcMap()
            {
                _npcSquareMap.Clear();
                foreach (var npc in _mapManager!.ActiveMap.Npc)
                {
                    // Location: x=tileCol, y=tileRow
                    var (r, c) = SquareMap.TileToSquare(npc.Location.y, npc.Location.x);
                    _npcSquareMap[(r, c)] = npc.NpcInfo.Id;
                }
            }

            // ── Grid rebuild ──────────────────────────────────────────────────────
            public void RebuildGrid()
            {
                var (bg, fg, _) = _mapManager!.GetViewport();
                var tileLayer = _isShowingBackground ? bg : fg;

                int viewRows = tileLayer.GetLength(0);
                int viewCols = tileLayer.GetLength(1);
                int tps = MapConstants.TilesPerSquare;
                int halfRows = viewRows / 2;
                int halfCols = viewCols / 2;

                RebuildNpcMap();
                UpdateMapImageSource();

                var vl = SquareMap.VisionLayer;
                var cellData = new List<(int sqRow, int sqCol, bool isPlayer, int npcId, int visionId, CollisionType col)>(viewRows * viewCols);

                for (int r = 0; r < viewRows; r++)
                {
                    for (int c = 0; c < viewCols; c++)
                    {
                        // playerLoc: x=tileCol, y=tileRow
                        int mapTileRow = _player.playerLoc.y - halfRows + r;
                        int mapTileCol = _player.playerLoc.x - halfCols + c;
                        int mapSqRow = mapTileRow / tps;
                        int mapSqCol = mapTileCol / tps;

                        _npcSquareMap.TryGetValue((mapSqRow, mapSqCol), out int npcId);

                        int vr = r / tps;
                        int vc = c / tps;
                        int visionId = (vr < vl.GetLength(0) && vc < vl.GetLength(1)) ? vl[vr, vc] : 0;

                        cellData.Add((
                            mapSqRow, mapSqCol,
                            r == halfRows && c == halfCols,
                            npcId,
                            visionId,
                            SquareMap.GetSquare(mapSqRow, mapSqCol)?.SquareType ?? CollisionType.None
                        ));
                    }
                }

                RebuildOverlaysFromData(cellData, viewRows, viewCols);

                var (psr, psc) = SquareMap.TileToSquare(_player.playerLoc.y, _player.playerLoc.x);
                CollisionAtCursor = SquareMap.GetCollision(psr, psc).ToString();
                NotifyHeaderProperties();
            }

<<<<<<< HEAD
            // ── Overlays ──────────────────────────────────────────────────────────
            private void RebuildOverlaysFromData(
                List<(int sqRow, int sqCol, bool isPlayer, int npcId, int visionId, CollisionType col)> cellData,
                int viewRows, int viewCols)
=======
            var (psr, psc) = SquareMap.TileToSquare(_player.playerLoc.x, _player.playerLoc.y);
            CollisionAtCursor = SquareMap.GetCollision(psr, psc).ToString();

            NotifyHeaderProperties();
        }

        // ── Move ──────────────────────────────────────────────────────────────────

        public void Move(FacingDirection direction)
        {
            if (Dialogue.IsOpen) return;
            _player.IsMoving = true;
            _player.AdvanceAnimation();          // advances the tick before building viewport

            var result = _mapManager.TryMove(direction);
            if (result.Success)
>>>>>>> ffedec0895c70be5f6563ca012858e64cb30befe
            {
                var newItems = new List<CanvasOverlayItem>(viewRows * viewCols * 2);

                for (int r = 0; r < viewRows; r++)
                {
                    for (int c = 0; c < viewCols; c++)
                    {
                        var (sqRow, sqCol, isPlayer, npcId, visionId, collision) = cellData[r * viewCols + c];
                        bool isNpc = npcId != 0;
                        bool isVision = visionId != 0;

                        string tooltip = $"[{sqRow},{sqCol}]  {collision}" +
                                         (isNpc ? $"  NPC:{npcId}" : string.Empty) +
                                         (isVision ? $"  seen-by:{visionId}" : string.Empty);

                        var (colColor, showCol) = CollisionDebugColor(collision);
                        if (showCol)
                            newItems.Add(new CanvasOverlayItem
                            {
                                Left = c * CellPx,
                                Top = r * CellPx,
                                HasCollision = true,
                                CollisionColor = colColor,
                                Tooltip = tooltip,
                            });

                        if (isVision && !isPlayer && !isNpc)
                            newItems.Add(new CanvasOverlayItem
                            {
                                Left = c * CellPx,
                                Top = r * CellPx,
                                IsVision = true,
                                Tooltip = tooltip,
                            });

                        if (isPlayer)
                            newItems.Add(new CanvasOverlayItem
                            {
                                Left = c * CellPx,
                                Top = r * CellPx,
                                IsPlayer = true,
                            });

                        if (isNpc)
                            newItems.Add(new CanvasOverlayItem
                            {
                                Left = c * CellPx,
                                Top = r * CellPx,
                                IsNpc = true,
                                IsTrainer = npcId % 2 != 0,
                                NpcSymbol = npcId % 2 != 0 ? "T" : "N",
                                Tooltip = tooltip,
                            });

                        if (_isDebugMode)
                            newItems.Add(new CanvasOverlayItem
                            {
                                Left = c * CellPx,
                                Top = r * CellPx,
                                IsDebug = true,
                                DebugText = $"{sqRow},{sqCol}",
                                DebugTintColor = CollisionToDebugColor(collision),
                            });
                    }
                }

                // Single Reset notification — one layout pass regardless of item count
                OverlaySnapshot = newItems;
            }

        public void RefreshOverlays()
            {
                var vl = SquareMap.VisionLayer;
                int viewRows = MapConstants.ViewRowSize;
                int viewCols = MapConstants.ViewColSize;
                int tps = MapConstants.TilesPerSquare;
                int halfRows = viewRows / 2;
                int halfCols = viewCols / 2;

                var cellData = new List<(int sqRow, int sqCol, bool isPlayer, int npcId, int visionId, CollisionType col)>(viewRows * viewCols);

                for (int r = 0; r < viewRows; r++)
                {
                    for (int c = 0; c < viewCols; c++)
                    {
                        int mapTileRow = _player.playerLoc.y - halfRows + r;
                        int mapTileCol = _player.playerLoc.x - halfCols + c;
                        int mapSqRow = mapTileRow / tps;
                        int mapSqCol = mapTileCol / tps;

                        _npcSquareMap.TryGetValue((mapSqRow, mapSqCol), out int npcId);

                        int vr = r / tps;
                        int vc = c / tps;
                        int visionId = (vr < vl.GetLength(0) && vc < vl.GetLength(1)) ? vl[vr, vc] : 0;

                        cellData.Add((
                            mapSqRow, mapSqCol,
                            r == halfRows && c == halfCols,
                            npcId,
                            visionId,
                            SquareMap.GetSquare(mapSqRow, mapSqCol)?.SquareType ?? CollisionType.None
                        ));
                    }
                }

                RebuildOverlaysFromData(cellData, viewRows, viewCols);
            }

            // ── Actions ───────────────────────────────────────────────────────────
            public void ToggleDebug()
            {
                IsDebugMode = !IsDebugMode;
                RebuildGrid();
            }

            public void Move(FacingDirection direction)
            {
<<<<<<< HEAD
                if (Dialogue.IsOpen) return;

                var result = _mapManager!.TryMove(direction);
                if (result.Success)
                {
                    LastMoveResult = $"Moved {direction}";
                    RebuildGrid();
                    if (result.WildEncounterTriggered) LastMoveResult += " + Wild Encounter!";
                    if (result.SpottedByNpcId != 0) LastMoveResult += $" + Spotted by NPC {result.SpottedByNpcId}!";
                }
                else
                {
                    LastMoveResult = $"Blocked ({direction})";
                    OnPropertyChanged(nameof(FacingText));
                }
            }

            public void Inspect()
            {
                if (Dialogue.IsOpen) { Dialogue.Advance(); return; }

                _mapManager!.TryInteractWithNpc();
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
=======
                _player.IsMoving = false;        // blocked — snap back to standing
                LastMoveResult = $"Blocked ({direction})";
>>>>>>> ffedec0895c70be5f6563ca012858e64cb30befe
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
                CollisionType.Blocked => ("#99FF2222", true),
                CollisionType.WildGrass => ("#9922CC44", true),
                CollisionType.HM => ("#992255FF", true),
                CollisionType.JumpLeft => ("#99FFCC00", true),
                CollisionType.JumpRight => ("#99FFCC00", true),
                CollisionType.JumpUp => ("#99FFCC00", true),
                CollisionType.JumpDown => ("#99FFCC00", true),
                CollisionType.None => (string.Empty, false),
                _ => ("#99FF00FF", true),
            };

            private static string CollisionToDebugColor(CollisionType c) => c switch
            {
                CollisionType.Blocked => "#55FF0000",
                CollisionType.WildGrass => "#5500FF00",
                CollisionType.HM => "#550000FF",
                CollisionType.JumpLeft or CollisionType.JumpRight
                    or CollisionType.JumpDown or CollisionType.JumpUp => "#55FFFF00",
                _ => "#00000000",
            };
        }
    }

