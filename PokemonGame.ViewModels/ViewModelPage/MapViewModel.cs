
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Model.Model.Map;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Services;
using PokemonGame.ViewModels.Translators;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.Dialogue;
using PokemonGame.ViewModels.ViewModelPage.Map.Command;

namespace PokemonGame.ViewModels.ViewModelPage
{
    public interface IFocusTarget
    {
        void RegisterFocusCallback(Action focus);
    }
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

    //just fields propertys,constructor, and InitializeAsync
    public partial class MapViewModel : ViewModelBase, IDisposable, IFocusTarget
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<ViewModelBase> _createTrainerCardViewModel;   
        public const double CellPx = 72.0;
        private const int MapTilePx = 8;

        private static readonly int ViewSqCols = MapConstants.ViewColSize / MapConstants.TilesPerSquare;
        private static readonly int ViewSqRows = MapConstants.ViewRowSize / MapConstants.TilesPerSquare;
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
        private readonly MovementState _movement = new();
        private Action _dialogueOpenedHandler;
        private Action _dialogueClosedHandler;
        private NpcObjectDomain _activeNpc;

        // ── Observable properties ─────────────────────────────────────────────
        private string _playerName;
        public string PlayerName
        {
            get => _playerName;
            private set => SetProperty(ref _playerName, value);
        }
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

        private ImageSource _playerImage;
        public ImageSource PlayerImage
        {
            get => _playerImage;
            private set => SetProperty(ref _playerImage, value);
        }

        private bool _isReady;
        public bool IsReady { get => _isReady; private set => SetProperty(ref _isReady, value); }

        private bool _isDebugMode;
        public bool IsDebugMode { get => _isDebugMode; set => SetProperty(ref _isDebugMode, value); }

        private bool _isShowingBackground = true;
        private bool _isShowingForeground;
        public bool IsShowingBackground { get => _isShowingBackground; private set => SetProperty(ref _isShowingBackground, value); }
        public bool IsShowingForeground { get => _isShowingForeground; private set => SetProperty(ref _isShowingForeground, value); }

        private string _collisionAtCursor = string.Empty;
        private string _lastMoveResult = string.Empty;
        private string _inspectResult = string.Empty;
        public string CollisionAtCursor { get => _collisionAtCursor; private set => SetProperty(ref _collisionAtCursor, value); }
        public string LastMoveResult { get => _lastMoveResult; private set => SetProperty(ref _lastMoveResult, value); }
        public string InspectResult { get => _inspectResult; private set => SetProperty(ref _inspectResult, value); }

        private IReadOnlyList<CanvasOverlayItem> _overlaySnapshot = Array.Empty<CanvasOverlayItem>();
        public IReadOnlyList<CanvasOverlayItem> OverlaySnapshot
        {
            get => _overlaySnapshot;
            private set => SetProperty(ref _overlaySnapshot, value);
        }

        // ── Computed ──────────────────────────────────────────────────────────
        private SquareMapState SquareMap => _mapManager.SquareMap;
        public string MapName => _mapManager?.ActiveMap.Name ?? string.Empty;
        public int MapWidth => _mapManager?.ActiveMap.Width ?? 0;
        public int MapHeight => _mapManager?.ActiveMap.Height ?? 0;
        public int SquareRows => _mapManager != null ? SquareMap.SquareRows : 0;
        public int SquareCols => _mapManager != null ? SquareMap.SquareCols : 0;
        public string FacingText => _player.trainerMapLocDomain.FacingDirection.ToString();
        public int PlayerSquareRow => _mapManager != null ? SquareMap.TileToSquare(_player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x).row : 0;
        public int PlayerSquareCol => _mapManager != null ? SquareMap.TileToSquare(_player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x).col : 0;
        public double PlayerPixelX => HalfSqCols * CellPx;
        public double PlayerPixelY => HalfSqRows * CellPx;
        public double ViewportWidthPx => ViewSqCols * CellPx;
        public double ViewportHeightPx => ViewSqRows * CellPx;

        public DialogueViewModel Dialogue { get; } = new();

        private Action _focusCallback;
        public void RegisterFocusCallback(Action focus) => _focusCallback = focus;

        private sealed class MovementState
        {
            public bool HasQueued;
            public int QueuedDirection;
        }

        // ── Constructor ───────────────────────────────────────────────────────
        public MapViewModel(NavigationStore navigationStore, Func<ViewModelBase> createTrainerCardViewModel)
        {
            _navigationStore = navigationStore;
            _player = PlayerDomain.Instance;
            _mapLoader = new MapLoader(new MapService());
            PlayerName = _player.trainerInfo.Name;
            InitCommands();
            Dialogue.FocusRequested += () => _focusCallback?.Invoke();

            _ = InitializeAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    System.Diagnostics.Debug.WriteLine("InitializeAsync failed: " + t.Exception);
            });
            _createTrainerCardViewModel = createTrainerCardViewModel;
        }

        private async Task InitializeAsync()
        {

            await Task.Run(() =>
            {
                var currentMapName = _player.trainerMapLocDomain?.CurrentMap?.Name;
                if (string.IsNullOrEmpty(currentMapName))
                {
                    // fallback — player wasn't loaded yet (e.g. dev startup)
                    var loader = new PlayerLoader(
                        ServiceFactory.Instance.StoryPlayerService,
                        _mapLoader);
                    loader.Load();
                }
            });
            _mapManager = new MapManager(_player);
            _mapManager.TrainerSpotted += OnPlayerSpotted;
            _mapManager.NpcInteracted += OnNpcInteracted;

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
                    ProcessMovementTick();
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
            _focusCallback?.Invoke();  
        }

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
    }
    //commands
    public partial class MapViewModel
    {
        public ShowLayerCommand ShowBackgroundCommand { get; private set; }
        public ShowLayerCommand ShowForegroundCommand { get; private set; }
        public MoveCommand MoveUpCommand { get; private set; }
        public MoveCommand MoveDownCommand { get; private set; }
        public MoveCommand MoveLeftCommand { get; private set; }
        public MoveCommand MoveRightCommand { get; private set; }
        public InspectCommand InspectCommand { get; private set; }
        public ICommand ToggleDebugCommand { get; private set; }
        public ICommand ToggleMenuCommand { get; private set; }
        public ICommand PickChoice1Command { get; private set; }
        public ICommand PickChoice2Command { get; private set; }
        public ICommand PickChoice3Command { get; private set; }
        public ICommand MenuUpCommand { get; private set; }
        public ICommand MenuDownCommand { get; private set; }
        public ICommand MenuConfirmCommand { get; private set; }
        public ICommand OpenPokedexCommand { get; private set; }
        public ICommand OpenBagCommand { get; private set; }
        public ICommand OpenPokemonCommand { get; private set; }
        public ICommand OpenPlayerCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        private void InitCommands()
        {
            ShowBackgroundCommand = new ShowLayerCommand(this, background: true);
            ShowForegroundCommand = new ShowLayerCommand(this, background: false);
            InspectCommand = new InspectCommand(this);
            MoveUpCommand = new MoveCommand(this, FacingDirection.Up);
            MoveDownCommand = new MoveCommand(this, FacingDirection.Down);
            MoveLeftCommand = new MoveCommand(this, FacingDirection.Left);
            MoveRightCommand = new MoveCommand(this, FacingDirection.Right);
            ToggleDebugCommand = new RelayCommand(() => ToggleDebug());
            ToggleMenuCommand = new RelayCommand(() => ToggleMenu());
            PickChoice1Command = new RelayCommand(() => Dialogue.PickChoice(0));
            PickChoice2Command = new RelayCommand(() => Dialogue.PickChoice(1));
            PickChoice3Command = new RelayCommand(() => Dialogue.PickChoice(2));
            MenuUpCommand = new RelayCommand(() => MenuUp());
            MenuDownCommand = new RelayCommand(() => MenuDown());
            MenuConfirmCommand = new RelayCommand(() => MenuConfirm());
            OpenPokedexCommand = new RelayCommand(() => { /* TODO */ });
            OpenBagCommand = new RelayCommand(() => { /* TODO */ });
            OpenPokemonCommand = new RelayCommand(() => { /* TODO */ });
            OpenPlayerCommand = new RelayCommand(() => { _navigationStore.CurrentViewModel = _createTrainerCardViewModel(); });
            SaveCommand = new RelayCommand(() => { /* TODO */ });
            ExitCommand = new RelayCommand(() => IsMenuOpen = false);
        }
    }
    //menu
    public partial class MapViewModel
    {
        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            private set => SetProperty(ref _isMenuOpen, value);
        }

        private int _menuIndex;
        public int MenuIndex
        {
            get => _menuIndex;
            private set => SetProperty(ref _menuIndex, value);
        }

        public void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
            MenuIndex = 0;
            if (IsMenuOpen) ClockManager.Instance.Pause();
            else ClockManager.Instance.Resume();
        }

        public void MenuUp()
        {
            if (!IsMenuOpen) return;
            MenuIndex = (MenuIndex - 1 + 6) % 6;
        }

        public void MenuDown()
        {
            if (!IsMenuOpen) return;
            MenuIndex = (MenuIndex + 1) % 6;
        }

        public void MenuConfirm()
        {
            if (!IsMenuOpen) return;
            switch (MenuIndex)
            {
                case 0: OpenPokedexCommand.Execute(null); break;
                case 1: OpenBagCommand.Execute(null); break;
                case 2: OpenPokemonCommand.Execute(null); break;
                case 3: OpenPlayerCommand.Execute(null); break;
                case 4: SaveCommand.Execute(null); break;
                case 5: ExitCommand.Execute(null); break;
            }
        }
    }
    //Movement
    public partial class MapViewModel
    {
        public void Move(FacingDirection direction)
        {
            if (Dialogue.IsOpen) return;
            if (IsMenuOpen)
            {
                if (direction == FacingDirection.Up) MenuUp();
                if (direction == FacingDirection.Down) MenuDown();
                return;
            }
            _movement.QueuedDirection = (int)direction;
            _movement.HasQueued = true;
        }

        private void ProcessMovementTick()
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
                if (result.SpottedByNpcId != 0) LastMoveResult += $" + Spotted by NPC {result.SpottedByNpcId}!";
            }
            else
            {
                _player.IsMoving = false;
                LastMoveResult = $"Blocked moving {direction}: {result.SquareType}";
                RefreshOverlays();
            }
        }
    }
    //Rendering
    public partial class MapViewModel
    {
        public void RebuildGrid()
        {
            var (_, _, _, playerSprite) = _mapManager.GetViewport();
            if (playerSprite != null)
                PlayerImage = LoadSprite(playerSprite.ImagePath);

            RebuildNpcMap();
            UpdateMapImageSource();

            var vl = SquareMap.VisionLayer;
            var (psr, psc) = SquareMap.TileToSquare(_player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x);
            var cellData = BuildCellData(psr, psc, vl);

            RebuildOverlaysFromData(cellData, ViewSqRows, ViewSqCols);
            CollisionAtCursor = SquareMap.GetCollision(psr, psc).ToString();
            NotifyHeaderProperties();
        }

        public void RefreshOverlays()
        {
            var vl = SquareMap.VisionLayer;
            var (psr, psc) = SquareMap.TileToSquare(_player.trainerMapLocDomain.playerLoc.y, _player.trainerMapLocDomain.playerLoc.x);
            RebuildOverlaysFromData(BuildCellData(psr, psc, vl), ViewSqRows, ViewSqCols);
        }

        private List<(int sqRow, int sqCol, bool isPlayer, int npcId, int visionId, CollisionType col)>
            BuildCellData(int psr, int psc, int[,] vl)
        {
            var cellData = new List<(int, int, bool, int, int, CollisionType)>(ViewSqRows * ViewSqCols);
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
                        npcId, visionId,
                        SquareMap.GetSquare(mapSqRow, mapSqCol)?.SquareType ?? CollisionType.None
                    ));
                }
            }
            return cellData;
        }

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
                    double left = c * CellPx;
                    double top = r * CellPx;

                    var (colColor, showCol) = CollisionDebugColor(collision);
                    if (showCol)
                        newItems.Add(new CanvasOverlayItem { Left = left, Top = top, HasCollision = true, CollisionColor = colColor, Tooltip = tooltip });

                    if (isVision && !isPlayer && !isNpc)
                        newItems.Add(new CanvasOverlayItem { Left = left, Top = top, IsVision = true, Tooltip = tooltip });

                    if (isPlayer)
                        newItems.Add(new CanvasOverlayItem { Left = left, Top = top, IsPlayer = true });

                    if (isNpc)
                        newItems.Add(new CanvasOverlayItem { Left = left, Top = top, IsNpc = true, IsTrainer = npcId % 2 != 0, NpcSymbol = npcId % 2 != 0 ? "T" : "N", Tooltip = tooltip });

                    if (_isDebugMode)
                        newItems.Add(new CanvasOverlayItem { Left = left, Top = top, IsDebug = true, DebugText = $"{sqRow},{sqCol}", DebugTintColor = CollisionToDebugColor(collision) });
                }
            }
            OverlaySnapshot = newItems;
        }

        private void RebuildNpcMap()
        {
            _npcSquareMap.Clear();
            foreach (var npc in _mapManager.ActiveMap.Npc)
            {
                var (r, c) = SquareMap.TileToSquare(npc.Location.y, npc.Location.x);
                _npcSquareMap[(r, c)] = npc.NpcInfo.Id;
            }
        }

        private ImageSource LoadSprite(string filename)
        {
            string fullPath = PlayerSpritePath + _player.trainerInfo.Gender.ToString() + @"\" + filename;
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

        public void ToggleDebug()
        {
            IsDebugMode = !IsDebugMode;
            RebuildGrid();
        }

        internal void SwitchLayer(bool background)
        {
            IsShowingBackground = background;
            IsShowingForeground = !background;
            RebuildGrid();
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
    //MapImage
    public partial class MapViewModel
    {
        private void UpdateMapImageSource()
        {
            var sheet = GetMapBitmap();
            if (sheet == null) { MapImageSource = null; return; }

            int tps = MapConstants.TilesPerSquare;
            int viewTileCols = ViewSqCols * tps;
            int viewTileRows = ViewSqRows * tps;

            int originTileCol = _player.trainerMapLocDomain.playerLoc.x - viewTileCols / 2;
            int originTileRow = _player.trainerMapLocDomain.playerLoc.y - viewTileRows / 2;

            int px = originTileCol * MapTilePx;
            int py = originTileRow * MapTilePx;
            int pw = viewTileCols * MapTilePx;
            int ph = viewTileRows * MapTilePx;

            double scale = CellPx / (tps * MapTilePx);
            int canvasW = (int)(pw * scale);
            int canvasH = (int)(ph * scale);

            var drawingVisual = new DrawingVisual();
            using (var ctx = drawingVisual.RenderOpen())
            {
                DrawMap(ctx, sheet, px, py, pw, ph, scale);
                DrawNeighbor(ctx, ConnectionDirection.North, px, py, pw, ph, scale, sheet.PixelWidth, sheet.PixelHeight, tps);
                DrawNeighbor(ctx, ConnectionDirection.South, px, py, pw, ph, scale, sheet.PixelWidth, sheet.PixelHeight, tps);
                DrawNeighbor(ctx, ConnectionDirection.West, px, py, pw, ph, scale, sheet.PixelWidth, sheet.PixelHeight, tps);
                DrawNeighbor(ctx, ConnectionDirection.East, px, py, pw, ph, scale, sheet.PixelWidth, sheet.PixelHeight, tps);
            }

            var rt = new RenderTargetBitmap(canvasW, canvasH, 96, 96, PixelFormats.Pbgra32);
            rt.Render(drawingVisual);
            rt.Freeze();

            MapImageSource = rt;
            ImageDisplayWidth = canvasW;
            ImageDisplayHeight = canvasH;
            ImageOffsetX = 0;
            ImageOffsetY = 0;
        }

        private void DrawMap(DrawingContext ctx, BitmapSource sheet,
            int px, int py, int pw, int ph, double scale)
        {
            int cropX = Math.Max(0, px);
            int cropY = Math.Max(0, py);
            int cropW = Math.Min(pw, sheet.PixelWidth - cropX);
            int cropH = Math.Min(ph, sheet.PixelHeight - cropY);
            int offX = Math.Max(0, -px);
            int offY = Math.Max(0, -py);
            if (cropW <= 0 || cropH <= 0) return;
            var crop = GetCrop(sheet, cropX, cropY, cropW, cropH);
            if (crop != null)
                ctx.DrawImage(crop, new Rect(offX * scale, offY * scale, cropW * scale, cropH * scale));
        }

        private void DrawNeighbor(DrawingContext ctx, ConnectionDirection dir,
            int px, int py, int pw, int ph, double scale, int mapW, int mapH, int tps)
        {
            var conn = _mapManager.ActiveMap.ConnectedMaps
                .FirstOrDefault(c => c.ConnectionDirection == dir);
            if (conn == null) return;

            var nb = GetMapBitmap(conn.ConnectedMap.Name);
            if (nb == null) return;

            int marginPx = conn.Margin * tps * MapTilePx;
            int nSrcX, nSrcY, nSrcW, nSrcH, nDstX, nDstY;

            switch (dir)
            {
                case ConnectionDirection.North when py >= 0: return;
                case ConnectionDirection.North:
                    nSrcX = Math.Max(0, px + marginPx);
                    nSrcY = nb.PixelHeight + py;
                    nDstX = (int)(Math.Max(0, -marginPx - px) * scale);
                    nDstY = 0;
                    nSrcW = Math.Min(pw, nb.PixelWidth - nSrcX);
                    nSrcH = Math.Min(-py, nb.PixelHeight - nSrcY);
                    break;

                case ConnectionDirection.South when py + ph <= mapH: return;
                case ConnectionDirection.South:
                    nSrcX = Math.Max(0, px + marginPx);
                    nSrcY = 0;
                    nDstX = (int)(Math.Max(0, -marginPx - px) * scale);
                    nDstY = (int)((mapH - py) * scale);
                    nSrcW = Math.Min(pw, nb.PixelWidth - nSrcX);
                    nSrcH = Math.Min(py + ph - mapH, nb.PixelHeight);
                    break;

                case ConnectionDirection.West when px >= 0: return;
                case ConnectionDirection.West:
                    nSrcX = nb.PixelWidth + px;
                    nSrcY = Math.Max(0, py + marginPx);
                    nDstX = 0;
                    nDstY = (int)(Math.Max(0, -marginPx - py) * scale);
                    nSrcW = Math.Min(-px, nb.PixelWidth - nSrcX);
                    nSrcH = Math.Min(ph, nb.PixelHeight - nSrcY);
                    break;

                case ConnectionDirection.East when px + pw <= mapW: return;
                case ConnectionDirection.East:
                    nSrcX = 0;
                    nSrcY = Math.Max(0, py + marginPx);
                    nDstX = (int)((mapW - px) * scale);
                    nDstY = (int)(Math.Max(0, -marginPx - py) * scale);
                    nSrcW = Math.Min(px + pw - mapW, nb.PixelWidth);
                    nSrcH = Math.Min(ph, nb.PixelHeight - nSrcY);
                    break;

                default: return;
            }

            if (nSrcW <= 0 || nSrcH <= 0 || nSrcX < 0 || nSrcY < 0) return;
            var crop = GetCrop(nb, nSrcX, nSrcY, nSrcW, nSrcH);
            if (crop != null)
                ctx.DrawImage(crop, new Rect(nDstX, nDstY, nSrcW * scale, nSrcH * scale));
        }

        private BitmapImage GetMapBitmap(string mapName)
        {
            string path = $@"C:\Users\yoav\source\repos\PokemonGame\PokemonGame\Assets\Images\Map\{mapName}.png";
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

        private BitmapImage GetMapBitmap() => GetMapBitmap(PlayerDomain.Instance.trainerMapLocDomain.CurrentMap.Name);

        private CroppedBitmap GetCrop(BitmapSource src, int x, int y, int w, int h)
        {
            try
            {
                var crop = new CroppedBitmap(src, new Int32Rect(x, y, w, h));
                crop.Freeze();
                return crop;
            }
            catch { return null; }
        }
    }
    //Npc
    public partial class MapViewModel
    {
        public void Inspect()
        {
            if (IsMenuOpen) { MenuConfirm(); return; }
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