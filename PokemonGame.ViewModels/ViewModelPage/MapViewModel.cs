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
    // MapLoader
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
    // RangeObservableCollection
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
        public const double CellPx = 36.0;
        private const int MapTilePx = 8;

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

        // ── Tick handlers (stored for unsubscribe) ────────────────────────────
        private EventHandler _npcTickHandler;
        private EventHandler _playerTickHandler;
        private readonly MovementSate _movement = new MovementSate();
        private Action _dialogueOpenedHandler;
        private Action _dialogueClosedHandler;

        // ── Movement queue ────────────────────────────────────────────────────
        // Keyed input sets this; the player tick drains it once per step.
        // Nullable — null means "no key held".

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
        public double ImageDisplayWidth { get => _imageDisplayWidth; private set => SetProperty(ref _imageDisplayWidth, value); }
        public double ImageDisplayHeight { get => _imageDisplayHeight; private set => SetProperty(ref _imageDisplayHeight, value); }
        public double ImageOffsetX { get => _imageOffsetX; private set => SetProperty(ref _imageOffsetX, value); }
        public double ImageOffsetY { get => _imageOffsetY; private set => SetProperty(ref _imageOffsetY, value); }

        // ── Player sprite ─────────────────────────────────────────────────────
        private ImageSource _playerImage;
        public ImageSource PlayerImage
        {
            get => _playerImage;
            private set => SetProperty(ref _playerImage, value);
        }

        public double PlayerPixelX => 20 + (MapConstants.ViewColSize / 2) * CellPx - 18;
        public double PlayerPixelY => 20 + (MapConstants.ViewRowSize / 2) * CellPx - 36;

        private bool _isReady;
        public bool IsReady { get => _isReady; private set => SetProperty(ref _isReady, value); }

        private bool _isDebugMode;
        public bool IsDebugMode { get => _isDebugMode; set => SetProperty(ref _isDebugMode, value); }

        private string _collisionAtCursor = string.Empty;
        private string _lastMoveResult = string.Empty;
        private string _inspectResult = string.Empty;
        private bool _isShowingBackground = true;
        private bool _isShowingForeground;
        private NpcObjectDomain _activeNpc;

        public string CollisionAtCursor { get => _collisionAtCursor; private set => SetProperty(ref _collisionAtCursor, value); }
        public string LastMoveResult { get => _lastMoveResult; private set => SetProperty(ref _lastMoveResult, value); }
        public string InspectResult { get => _inspectResult; private set => SetProperty(ref _inspectResult, value); }
        public bool IsShowingBackground { get => _isShowingBackground; private set => SetProperty(ref _isShowingBackground, value); }
        public bool IsShowingForeground { get => _isShowingForeground; private set => SetProperty(ref _isShowingForeground, value); }

        // ── Computed header properties ────────────────────────────────────────
        private SquareMapState SquareMap => _mapManager.SquareMap;
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

        private Action _focusCallback;
        public void RegisterFocusCallback(Action focus) => _focusCallback = focus;

        // ── Constructor ───────────────────────────────────────────────────────
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
            _mapManager.TrainerSpotted += OnPlayerSpotted;
            _mapManager.NpcInteracted += OnNpcInteracted;

            // ── NPC tick — moves NPCs, refreshes vision overlay ───────────────
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

            ClockManager.Instance.NpcTick += _npcTickHandler;
            ClockManager.Instance.PlayerTick += _playerTickHandler;
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

            if (_playerTickHandler != null) ClockManager.Instance.PlayerTick -= _playerTickHandler;
            if (_npcTickHandler != null) ClockManager.Instance.NpcTick -= _npcTickHandler;
            if (_dialogueOpenedHandler != null) Dialogue.DialogueOpened -= _dialogueOpenedHandler;
            if (_dialogueClosedHandler != null) Dialogue.DialogueClosed -= _dialogueClosedHandler;

            if (_mapManager != null)
            {
                _mapManager.TrainerSpotted -= OnPlayerSpotted;
                _mapManager.NpcInteracted -= OnNpcInteracted;
            }
            
            ClockManager.Instance.Stop();
        }

        // ── Movement — called by MoveCommand (keyboard/dpad) ─────────────────
        // Just queues the direction. The player tick fires at 150ms intervals
        // and consumes exactly one step per tick. Holding a key keeps the queue
        // populated via WPF key-repeat, releasing it leaves the queue empty so
        // movement stops cleanly after the current step finishes.
        public void Move(FacingDirection direction)
        {
            if(Dialogue.IsOpen) return;

            _movement.QueuedDirection = (int)direction;
            _movement.HasQueued = true;
        }

        private void proccessMovementTick()
        {
            if (Dialogue.IsOpen) return;
            if(!_movement.HasQueued) return;
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
                if (result.SpottedByNpcId != 0) LastMoveResult += $" + Spotted by NPC {result.SpottedByNpcId}!";
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
                bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
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

            // Unclamped origin — may be negative or past image edge
            int px = (_player.playerLoc.x - viewCols / 2) * MapTilePx;
            int py = (_player.playerLoc.y - viewRows / 2) * MapTilePx;
            int pw = viewCols * MapTilePx;
            int ph = viewRows * MapTilePx;

            int imgW = sheet.PixelWidth;
            int imgH = sheet.PixelHeight;

            // Pixels of the viewport that are off the top/left edge
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

            double scale = CellPx / MapTilePx;

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

            MapImageSource = crop;
            ImageDisplayWidth = cropW * scale;
            ImageDisplayHeight = cropH * scale;
            ImageOffsetX = offsetX * scale;
            ImageOffsetY = offsetY * scale;
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
        public void RebuildGrid()
        {
            var (bg, fg, _, playerSprite) = _mapManager.GetViewport();

            if (playerSprite != null)
                PlayerImage = LoadSprite(playerSprite.ImagePath);

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
                        npcId, visionId,
                        SquareMap.GetSquare(mapSqRow, mapSqCol)?.SquareType ?? CollisionType.None
                    ));
                }
            }

            RebuildOverlaysFromData(cellData, viewRows, viewCols);

            var (psr, psc) = SquareMap.TileToSquare(_player.playerLoc.y, _player.playerLoc.x);
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
                        npcId, visionId,
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
        private sealed class MovementSate
        {
            public bool HasQueued;
            public int QueuedDirection;
        }
    }
}