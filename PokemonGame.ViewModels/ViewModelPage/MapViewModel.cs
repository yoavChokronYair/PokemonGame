using System.Collections.ObjectModel;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft;
using Microsoft.ServiceHub.Resources;
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
    public sealed class MapLoader
    {
        private readonly IMapService _mapService;
        private readonly Dictionary<int, MapDomain> _cache = new Dictionary<int, MapDomain>();

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

        // ── Tile layers (visual, sparse — X/Y must be preserved) ─────────────

        private enum TileLayerType { Ground = 0, Water = 1, Objects = 2, Above = 3 }

        private static List<TileDomain> BuildTiles(
            IReadOnlyList<MapTileData> tiles, TileLayerType layer)
        {
            var result = new List<TileDomain>();
            foreach (var t in tiles)
            {
                if (t.LayerType != (int)layer) continue;
                result.Add(new TileDomain
                {
                    Tileid = t.TileId,
                    X = t.X,       // ← position preserved so BuildTileArray places correctly
                    Y = t.Y,
                });
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

        // ── Tile image caches ────────────────────────────────────────────────────
        // _tilesetCache  : path → full BitmapImage (never cleared, one per tileset PNG)
        // _tileSliceCache: tileId → CroppedBitmap  (never cleared on move, only on map change)
        private readonly Dictionary<string, BitmapImage> _tilesetCache = new Dictionary<string, BitmapImage>();
        private readonly Dictionary<int, ImageSource> _tileSliceCache = new Dictionary<int, ImageSource>();

        // ── State ────────────────────────────────────────────────────────────────
        private string _collisionAtCursor = string.Empty;
        private string _lastMoveResult = string.Empty;
        private string _inspectResult = string.Empty;
        private bool _isShowingBackground = true;
        private bool _isShowingForeground;
        private TileCellViewModel? _currentPlayerCell;
        private NpcObjectDomain? _activeNpc;

        // NPC square lookup rebuilt once per tick/move, not per-cell
        private Dictionary<(int row, int col), int> _npcSquareMap = new Dictionary<(int, int), int>();

        public DialogueViewModel Dialogue { get; } = new DialogueViewModel();
        private SquareMapState SquareMap => _mapManager.SquareMap;
        public ObservableCollection<TileRowViewModel> TileRows { get; } = new ObservableCollection<TileRowViewModel>();

        // ── Header properties ────────────────────────────────────────────────────
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

        public ShowLayerCommand ShowBackgroundCommand { get; }
        public ShowLayerCommand ShowForegroundCommand { get; }
        public MoveCommand MoveUpCommand { get; }
        public MoveCommand MoveDownCommand { get; }
        public MoveCommand MoveLeftCommand { get; }
        public MoveCommand MoveRightCommand { get; }
        public InspectCommand InspectCommand { get; }

        // ── Constructor ──────────────────────────────────────────────────────────
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
                    RebuildNpcMap();
                    RefreshNpcs();
                }
            };

            ClockManager.Instance.Start();
            InitGrid();
            RebuildGrid();
        }

        // ── Image slicing ─────────────────────────────────────────────────────────
        // CroppedBitmaps are cached by tileId forever (within the same map).
        // On map change call _tileSliceCache.Clear() once.

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

        // ── Grid init — called once or on map change ──────────────────────────────
        // Allocates the TileCellViewModel objects. Never recreated on normal moves.

        private void InitGrid()
        {
            int tileRows = MapConstants.ViewRowSize;
            int tileCols = MapConstants.ViewColSize;

            TileRows.Clear();
            for (int r = 0; r < tileRows; r++)
            {
                var row = new TileRowViewModel();
                for (int c = 0; c < tileCols; c++)
                    row.Cells.Add(new TileCellViewModel());
                TileRows.Add(row);
            }
        }

        // ── NPC square map — O(nNpcs), rebuilt once per move, not per-cell ────────

        private void RebuildNpcMap()
        {
            _npcSquareMap.Clear();
            foreach (var npc in _mapManager.ActiveMap.Npc)
            {
                var (r, c) = SquareMap.TileToSquare(npc.Location.x, npc.Location.y);
                _npcSquareMap[(r, c)] = npc.NpcInfo.Id;
            }
        }

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
                {
                    var cell = TileRows[r].Cells[c];
                    int tileId = tileLayer[r, c];

                    if (cell.TileId != tileId)
                    {
                        cell.TileId = tileId;
                        cell.TileImage = GetImageSource(tileId);
                    }

                    // FIX: Map X to Columns (c) and Y to Rows (r)
                    // Assuming playerLoc.x is Column and playerLoc.y is Row
                    int mapTileCol = _player.playerLoc.x - halfCols + c;
                    int mapTileRow = _player.playerLoc.y - halfRows + r;

                    int mapSqRow = mapTileRow / tps;
                    int mapSqCol = mapTileCol / tps;

                    cell.Row = mapSqRow;
                    cell.Col = mapSqCol;

                    // ... rest of your collision and NPC logic ...
                    cell.IsPlayerHere = (r == halfRows && c == halfCols);
                    cell.Collision = SquareMap.GetSquare(mapSqRow, mapSqCol)?.SquareType ?? CollisionType.None;
                    _npcSquareMap.TryGetValue((mapSqRow, mapSqCol), out int npcId);
                    cell.NpcId = npcId;

                    // Vision indexing (ensure this matches your array structure)
                    int vr = r / tps;
                    int vc = c / tps;
                    cell.NpcVisionId = (vr < vl.GetLength(0) && vc < vl.GetLength(1))
                                        ? vl[vr, vc] : 0;
                }
            }

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
            {
                LastMoveResult = $"Moved {direction}";

                if (_mapManager.ActiveMap.Name != (TileRows.Count > 0 ? _mapManager.ActiveMap.Name : string.Empty))
                {
                    // Map changed — clear tile cache and rebuild structure
                    _tileSliceCache.Clear();
                    InitGrid();
                }

                RebuildGrid();

                if (result.WildEncounterTriggered) LastMoveResult += " + Wild Encounter!";
                if (result.SpottedByNpcId != 0) LastMoveResult += $" + Spotted by NPC {result.SpottedByNpcId}!";
            }
            else
            {
                _player.IsMoving = false;        // blocked — snap back to standing
                LastMoveResult = $"Blocked ({direction})";
                OnPropertyChanged(nameof(FacingText));
            }
        }

        // ── NPC tick refresh — only updates NPC/vision cells, skips tile images ───

        public void RefreshNpcs()
        {
            var vl = SquareMap.VisionLayer;
            int tps = MapConstants.TilesPerSquare;
            int halfTileRows = MapConstants.ViewRowSize / 2;
            int halfTileCols = MapConstants.ViewColSize / 2;

            for (int r = 0; r < TileRows.Count; r++)
            {
                for (int c = 0; c < TileRows[r].Cells.Count; c++)
                {
                    var cell = TileRows[r].Cells[c];
                    int mapSqRow = (_player.playerLoc.x - halfTileRows + r) / tps;
                    int mapSqCol = (_player.playerLoc.y - halfTileCols + c) / tps;

                    _npcSquareMap.TryGetValue((mapSqRow, mapSqCol), out int npcId);
                    cell.NpcId = npcId;

                    int vr = r / tps, vc = c / tps;
                    cell.NpcVisionId = (vr < vl.GetLength(0) && vc < vl.GetLength(1))
                        ? vl[vr, vc] : 0;
                }
            }
        }

        // ── Inspect ───────────────────────────────────────────────────────────────

        public void Inspect()
        {
            if (Dialogue.IsOpen) { Dialogue.Advance(); return; }

            _mapManager.TryInteractWithNpc();
            if (Dialogue.IsOpen) return;

            var result = _mapManager.TryInspect();
            InspectResult = result.Message;

            if (result.Type == InspectResultType.ItemPickup ||
                result.Type == InspectResultType.HmUsed)
                UpdateTileCollision(result.TargetRow, result.TargetCol, CollisionType.None);

            if (result.Type == InspectResultType.NpcDialogue && result.DialogueSet != null)
                Dialogue.Open(result.DialogueSet, result.NpcName);
        }

        private void UpdateTileCollision(int sqRow, int sqCol, CollisionType collision)
        {
            int tps = MapConstants.TilesPerSquare;
            int halfTileRows = MapConstants.ViewRowSize / 2;
            int halfTileCols = MapConstants.ViewColSize / 2;

            for (int r = 0; r < TileRows.Count; r++)
                for (int c = 0; c < TileRows[r].Cells.Count; c++)
                {
                    int msr = (_player.playerLoc.x - halfTileRows + r) / tps;
                    int msc = (_player.playerLoc.y - halfTileCols + c) / tps;
                    if (msr == sqRow && msc == sqCol)
                        TileRows[r].Cells[c].Collision = collision;
                }
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
