using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
    // ─────────────────────────────────────────────────────────────────────────
    // MapLoader  (unchanged)
    // ─────────────────────────────────────────────────────────────────────────
    public sealed class MapLoader
    {
        private readonly IMapService _mapService;
        private readonly Dictionary<int, MapDomain> _cache = new();

        public MapLoader(IMapService mapService) => _mapService = mapService;

        public MapDomain Load(string mapName)
        {
            _cache.Clear();
            var bundle = _mapService.GetMap(mapName)
                ?? throw new InvalidOperationException($"Map '{mapName}' not found.");
            return BuildDomain(bundle);
        }

        private MapDomain BuildDomain(MapBundle bundle)
        {
            if (_cache.TryGetValue(bundle.Map.Id, out var existing)) return existing;

            var domain = new MapDomain
            {
                Name = bundle.Map.Name,
                Width = bundle.Map.Width,
                Height = bundle.Map.Height,
                BackgroundBlocks = BuildTiles(bundle.Tiles, TileLayerType.Ground),
                Blocks = BuildTiles(bundle.Tiles, TileLayerType.Objects),
                CollisionObjects = BuildCollisionObjects(bundle.Collisions),
                ConnectedMaps = new List<ConnectedMapDomain>(),
                Wraps = new List<WrapDomain>(),
                Npc = new List<NpcObjectDomain>(),
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
                result.Add(new CollisionObjectDomain
                {
                    X = r.X,
                    Y = r.Y,
                    Width = r.Width,
                    Height = r.Height,
                    CollisionType = (CollisionType)r.CollisionType,
                });
            return result;
        }

        private static NpcObjectDomain BuildNpc(NpcSpawnData spawn) => new()
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

    // ─────────────────────────────────────────────────────────────────────────
    // Canvas overlay item  — one per player/NPC marker drawn on top of tiles
    // ─────────────────────────────────────────────────────────────────────────
    public class CanvasOverlayItem
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public bool IsPlayer { get; set; }
        public bool IsNpc { get; set; }
        public bool IsTrainer { get; set; }
        public bool IsVision { get; set; }
        public string? NpcSymbol { get; set; }
        public string? Tooltip { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MapViewModel
    // ─────────────────────────────────────────────────────────────────────────
    public class MapViewModel : ViewModelBase
    {
        // ── Constants ────────────────────────────────────────────────────────
        public const double CellPx = 36.0;   // pixel size of each tile cell on screen

        // ── Core objects ─────────────────────────────────────────────────────
        private readonly MapManager _mapManager;
        private readonly PlayerDomain _player;
        private readonly MapLoader _mapLoader;

        // ── Tileset image caches ──────────────────────────────────────────────
        private readonly Dictionary<string, BitmapImage> _tilesetCache = new();
        private readonly Dictionary<int, ImageSource> _tileSliceCache = new();

        // ── State ─────────────────────────────────────────────────────────────
        private string _collisionAtCursor = string.Empty;
        private string _lastMoveResult = string.Empty;
        private string _inspectResult = string.Empty;
        private bool _isShowingBackground = true;
        private bool _isShowingForeground;
        private NpcObjectDomain? _activeNpc;

        private Dictionary<(int row, int col), int> _npcSquareMap = new();

        // ── Public data for the Canvas ────────────────────────────────────────
        // Flat list of tile images — the Canvas binds to this and positions each
        // Image at (Col * CellPx, Row * CellPx) within the viewport.
        public ObservableCollection<TileCellData> Tiles { get; } = new();

        // Overlay markers (player dot, NPC symbols, vision tint rectangles)
        public ObservableCollection<CanvasOverlayItem> Overlays { get; } = new();

        // ── Dialogue ──────────────────────────────────────────────────────────
        public DialogueViewModel Dialogue { get; } = new();

        // ── Viewport pixel size (for Canvas Width/Height binding) ─────────────
        public double ViewportWidthPx => MapConstants.ViewColSize * CellPx;
        public double ViewportHeightPx => MapConstants.ViewRowSize * CellPx;

        // ── Header properties ─────────────────────────────────────────────────
        private SquareMapState SquareMap => _mapManager.SquareMap;
        public string MapName => _mapManager.ActiveMap.Name;
        public int MapWidth => _mapManager.ActiveMap.Width;
        public int MapHeight => _mapManager.ActiveMap.Height;
        public int SquareRows => SquareMap.SquareRows;
        public int SquareCols => SquareMap.SquareCols;
        public string FacingText => _player.FacingDirection.ToString();
        public int PlayerSquareRow => SquareMap.TileToSquare(_player.playerLoc.x, _player.playerLoc.y).row;
        public int PlayerSquareCol => SquareMap.TileToSquare(_player.playerLoc.x, _player.playerLoc.y).col;

        public string CollisionAtCursor { get => _collisionAtCursor; private set => SetProperty(ref _collisionAtCursor, value); }
        public string LastMoveResult { get => _lastMoveResult; private set => SetProperty(ref _lastMoveResult, value); }
        public string InspectResult { get => _inspectResult; private set => SetProperty(ref _inspectResult, value); }
        public bool IsShowingBackground { get => _isShowingBackground; private set => SetProperty(ref _isShowingBackground, value); }
        public bool IsShowingForeground { get => _isShowingForeground; private set => SetProperty(ref _isShowingForeground, value); }

        // ── Commands ──────────────────────────────────────────────────────────
        private Action? _focusCallback;
        public void RegisterFocusCallback(Action focus) => _focusCallback = focus;

        public ShowLayerCommand ShowBackgroundCommand { get; }
        public ShowLayerCommand ShowForegroundCommand { get; }
        public MoveCommand MoveUpCommand { get; }
        public MoveCommand MoveDownCommand { get; }
        public MoveCommand MoveLeftCommand { get; }
        public MoveCommand MoveRightCommand { get; }
        public InspectCommand InspectCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────
        public MapViewModel()
        {
            _player = PlayerDomain.Instance;
            _mapLoader = new MapLoader(new MapService());

            _player.CurrentMap = _mapLoader.Load("Pallet Town");
            if (_player.playerLoc == default) _player.playerLoc = (14, 12);

            _mapManager = new MapManager(_player);
            _mapManager.TrainerSpotted += OnPlayerSpotted;
            _mapManager.NpcInteracted += OnNpcInteracted;

            ShowBackgroundCommand = new ShowLayerCommand(this, background: true);
            ShowForegroundCommand = new ShowLayerCommand(this, background: false);
            InspectCommand = new InspectCommand(this);
            MoveUpCommand = new MoveCommand(this, FacingDirection.Up);
            MoveDownCommand = new MoveCommand(this, FacingDirection.Down);
            MoveLeftCommand = new MoveCommand(this, FacingDirection.Left);
            MoveRightCommand = new MoveCommand(this, FacingDirection.Right);

            Dialogue.FocusRequested += () => _focusCallback?.Invoke();

            ClockManager.Instance.NpcTick += (_, _) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mapManager.TickNpcs();
                    RebuildNpcMap();
                    RefreshOverlays();
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
                    RebuildNpcMap();
                    RefreshOverlays();
                }
            };

            ClockManager.Instance.Start();
            RebuildGrid();
        }

        // ── Image slicing ─────────────────────────────────────────────────────

        private ImageSource? GetImageSource(int tileId)
        {
            if (tileId <= 0) return null;
            if (_tileSliceCache.TryGetValue(tileId, out var cached)) return cached;

            string path = $"pack://application:,,,/Assets/Tilesets/{_mapManager.ActiveMap.Name}.png";
            if (!_tilesetCache.TryGetValue(path, out var masterSheet))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(path, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    _tilesetCache[path] = masterSheet = bmp;
                }
                catch { return null; }
            }

            int tilePixelSize = 8;
            int tilesPerRow = masterSheet.PixelWidth / tilePixelSize;
            int x = (tileId % tilesPerRow) * tilePixelSize;
            int y = (tileId / tilesPerRow) * tilePixelSize;

            try
            {
                var slice = new CroppedBitmap(masterSheet, new Int32Rect(x, y, tilePixelSize, tilePixelSize));
                slice.Freeze();
                _tileSliceCache[tileId] = slice;
                return slice;
            }
            catch { return null; }
        }

        // ── NPC square map ────────────────────────────────────────────────────

        private void RebuildNpcMap()
        {
            _npcSquareMap.Clear();
            foreach (var npc in _mapManager.ActiveMap.Npc)
            {
                var (r, c) = SquareMap.TileToSquare(npc.Location.x, npc.Location.y);
                _npcSquareMap[(r, c)] = npc.NpcInfo.Id;
            }
        }

        // ── RebuildGrid — replaces InitGrid + RebuildGrid from old code ────────
        // Writes into Tiles (flat list) and then rebuilds Overlays.
        // Called on every move and on map change.

        public void RebuildGrid()
        {
            var (bg, fg, _) = _mapManager.GetViewport();
            var tileLayer = _isShowingBackground ? bg : fg;

            int viewRows = tileLayer.GetLength(0);
            int viewCols = tileLayer.GetLength(1);
            int tps = MapConstants.TilesPerSquare;
            int halfRows = viewRows / 2;
            int halfCols = viewCols / 2;

            RebuildNpcMap();
            var vl = SquareMap.VisionLayer;

            // Resize Tiles collection to match viewport without reallocating everything
            int needed = viewRows * viewCols;
            while (Tiles.Count < needed) Tiles.Add(new TileCellData());
            while (Tiles.Count > needed) Tiles.RemoveAt(Tiles.Count - 1);

            for (int r = 0; r < viewRows; r++)
            {
                for (int c = 0; c < viewCols; c++)
                {
                    var cell = Tiles[r * viewCols + c];
                    int tileId = tileLayer[r, c];

                    int mapTileCol = _player.playerLoc.x - halfCols + c;
                    int mapTileRow = _player.playerLoc.y - halfRows + r;
                    int mapSqRow = mapTileRow / tps;
                    int mapSqCol = mapTileCol / tps;

                    cell.Row = mapSqRow;
                    cell.Col = mapSqCol;
                    cell.TileId = tileId;
                    cell.TileImage = GetImageSource(tileId);
                    cell.IsPlayerHere = (r == halfRows && c == halfCols);
                    cell.Collision = SquareMap.GetSquare(mapSqRow, mapSqCol)?.SquareType ?? CollisionType.None;

                    _npcSquareMap.TryGetValue((mapSqRow, mapSqCol), out int npcId);
                    cell.NpcId = npcId;

                    int vr = r / tps, vc = c / tps;
                    cell.NpcVisionId = (vr < vl.GetLength(0) && vc < vl.GetLength(1)) ? vl[vr, vc] : 0;

                    // Canvas position in pixels
                    cell.CanvasLeft = c * CellPx;
                    cell.CanvasTop = r * CellPx;
                }
            }

            // Notify canvas to re-render tile images
            OnPropertyChanged(nameof(Tiles));

            // Rebuild overlay markers (player + NPCs)
            RebuildOverlays(viewRows, viewCols);

            // Update status bar
            var (psr, psc) = SquareMap.TileToSquare(_player.playerLoc.x, _player.playerLoc.y);
            CollisionAtCursor = SquareMap.GetCollision(psr, psc).ToString();
            NotifyHeaderProperties();
        }

        // ── Overlays — player dot + NPC symbols ───────────────────────────────

        private void RebuildOverlays(int viewRows, int viewCols)
        {
            Overlays.Clear();

            int tps = MapConstants.TilesPerSquare;
            int halfRows = viewRows / 2;
            int halfCols = viewCols / 2;
            var vl = SquareMap.VisionLayer;

            for (int r = 0; r < viewRows; r++)
            {
                for (int c = 0; c < viewCols; c++)
                {
                    var cell = Tiles[r * viewCols + c];

                    // Vision tint rectangle
                    if (cell.IsInNpcVision && !cell.IsPlayerHere && !cell.IsNpcHere)
                    {
                        Overlays.Add(new CanvasOverlayItem
                        {
                            Left = c * CellPx,
                            Top = r * CellPx,
                            IsVision = true,
                            Tooltip = cell.Tooltip,
                        });
                    }

                    // Player dot
                    if (cell.IsPlayerHere)
                    {
                        Overlays.Add(new CanvasOverlayItem
                        {
                            Left = c * CellPx,
                            Top = r * CellPx,
                            IsPlayer = true,
                        });
                    }

                    // NPC symbol
                    if (cell.IsNpcHere)
                    {
                        Overlays.Add(new CanvasOverlayItem
                        {
                            Left = c * CellPx,
                            Top = r * CellPx,
                            IsNpc = true,
                            IsTrainer = cell.NpcId % 2 != 0,
                            NpcSymbol = cell.NpcSymbol,
                            Tooltip = cell.Tooltip,
                        });
                    }
                }
            }
        }

        // NPC-only overlay refresh (called on NPC tick — skips tile rebuild)
        public void RefreshOverlays()
        {
            var vl = SquareMap.VisionLayer;
            int viewRows = MapConstants.ViewRowSize;
            int viewCols = MapConstants.ViewColSize;
            int tps = MapConstants.TilesPerSquare;
            int halfRows = viewRows / 2;
            int halfCols = viewCols / 2;

            // Update NpcId / NpcVisionId on existing Tiles so Tooltip stays correct
            for (int r = 0; r < viewRows; r++)
            {
                for (int c = 0; c < viewCols; c++)
                {
                    if (r * viewCols + c >= Tiles.Count) continue;
                    var cell = Tiles[r * viewCols + c];

                    int mapTileCol = _player.playerLoc.x - halfCols + c;
                    int mapTileRow = _player.playerLoc.y - halfRows + r;
                    int mapSqRow = mapTileRow / tps;
                    int mapSqCol = mapTileCol / tps;

                    _npcSquareMap.TryGetValue((mapSqRow, mapSqCol), out int npcId);
                    cell.NpcId = npcId;

                    int vr = r / tps, vc = c / tps;
                    cell.NpcVisionId = (vr < vl.GetLength(0) && vc < vl.GetLength(1)) ? vl[vr, vc] : 0;
                }
            }

            RebuildOverlays(viewRows, viewCols);
        }

        // ── Move ──────────────────────────────────────────────────────────────

        public void Move(FacingDirection direction)
        {
            if (Dialogue.IsOpen) return;

            var result = _mapManager.TryMove(direction);
            if (result.Success)
            {
                LastMoveResult = $"Moved {direction}";

                bool mapChanged = _tilesetCache.Count > 0 &&
                                  !_tilesetCache.ContainsKey(
                                      $"pack://application:,,,/Assets/Tilesets/{_mapManager.ActiveMap.Name}.png");
                if (mapChanged)
                    _tileSliceCache.Clear();

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

        // ── Inspect ───────────────────────────────────────────────────────────

        public void Inspect()
        {
            if (Dialogue.IsOpen) { Dialogue.Advance(); return; }

            _mapManager.TryInteractWithNpc();
            if (Dialogue.IsOpen) return;

            var result = _mapManager.TryInspect();
            InspectResult = result.Message;

            if (result.Type == InspectResultType.ItemPickup ||
                result.Type == InspectResultType.HmUsed)
                RebuildGrid();   // full rebuild — collision state changed

            if (result.Type == InspectResultType.NpcDialogue && result.DialogueSet != null)
                Dialogue.Open(result.DialogueSet, result.NpcName);
        }

        internal void SwitchLayer(bool background)
        {
            IsShowingBackground = background;
            IsShowingForeground = !background;
            RebuildGrid();
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