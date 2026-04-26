using System.Collections.ObjectModel;
using System.Windows.Threading;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Npc;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Model.Model.Map;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.Map;
using PokemonGame.ViewModels.ViewModelPage.Map.Command;

namespace PokemonGame.ViewModels.ViewModelPage
{
    public class MapViewModel : ViewModelBase
    {
        private readonly MapManager _mapManager;
        private readonly PlayerDomain _player;
        private readonly DispatcherTimer _npcTimer;

        private bool _isShowingBackground = true;
        private bool _isShowingForeground;
        private string _collisionAtCursor = string.Empty;
        private string _lastMoveResult = string.Empty;
        private TileCellViewModel? _currentPlayerCell;
        private SquareMapState SquareMap => _mapManager.SquareMap;

        public ObservableCollection<TileRowViewModel> TileRows { get; } = new();

        // ── Header ──────────────────────────────────────────────────────────
        public string MapName => _mapManager.ActiveMap.Name;
        public int MapWidth => _mapManager.ActiveMap.Width;
        public int MapHeight => _mapManager.ActiveMap.Height;
        public int SquareRows => SquareMap.SquareRows;
        public int SquareCols => SquareMap.SquareCols;

        public int PlayerSquareRow
            => SquareMap.TileToSquare(_player.playerLoc.x, _player.playerLoc.y).row;
        public int PlayerSquareCol
            => SquareMap.TileToSquare(_player.playerLoc.x, _player.playerLoc.y).col;

        public string FacingText => _player.FacingDirection.ToString();

        // ── Status bar ──────────────────────────────────────────────────────
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
        private string _inspectResult = string.Empty;
        // ── Layer toggles ───────────────────────────────────────────────────
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

        // ── Commands ────────────────────────────────────────────────────────
        public ShowLayerCommand ShowBackgroundCommand { get; }
        public ShowLayerCommand ShowForegroundCommand { get; }

        public MoveCommand MoveUpCommand { get; }
        public MoveCommand MoveDownCommand { get; }
        public MoveCommand MoveLeftCommand { get; }
        public MoveCommand MoveRightCommand { get; }
        public InspectCommand InspectCommand { get; }

        // in constructor:

        // ── Constructor ─────────────────────────────────────────────────────
        public MapViewModel()
        {
            _player = PlayerDomain.Instance;

            _player.CurrentMap ??= MapBootstrap.CreatePlaceholderMap();
            _player.playerLoc = _player.playerLoc == default ? (4, 4) : _player.playerLoc;

            _mapManager = new MapManager(_player);

            ShowBackgroundCommand = new ShowLayerCommand(this, background: true);
            ShowForegroundCommand = new ShowLayerCommand(this, background: false);
            InspectCommand = new InspectCommand(this);
            MoveUpCommand = new MoveCommand(this, FacingDirection.Up);
            MoveDownCommand = new MoveCommand(this, FacingDirection.Down);
            MoveLeftCommand = new MoveCommand(this, FacingDirection.Left);
            MoveRightCommand = new MoveCommand(this, FacingDirection.Right);
            // In constructor, after RebuildGrid():
            _npcTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // adjust speed here
            };
            _npcTimer.Tick += (_, _) =>
            {
                _mapManager.TickNpcs();
                RefreshNpcs();
            };
            _npcTimer.Start();

            RebuildGrid();
        }

        public void Inspect()
        {
            var result = _mapManager.TryInspect();
            InspectResult = result.Message;

            switch (result.Type)
            {
                case InspectResultType.ItemPickup:
                case InspectResultType.HmUsed:
                    UpdateTileCollision(result.TargetRow, result.TargetCol, CollisionType.None);
                    break;
            }
        }

        private void UpdateTileCollision(int squareRow, int squareCol, CollisionType collision)
        {
            if (squareRow < TileRows.Count && squareCol < TileRows[squareRow].Cells.Count)
                TileRows[squareRow].Cells[squareCol].Collision = collision;
        }
        // ── Movement — called by MoveCommand ────────────────────────────
        public void Move(FacingDirection direction)
        {
            var mapBefore = _mapManager.ActiveMap;
            var result = _mapManager.TryMove(direction);

            if (result.Success)
            {

                LastMoveResult = $"Moved {direction}";

                if (_mapManager.ActiveMap != mapBefore)
                {
                    RebuildGrid();
                }
                else
                {
                    RefreshNpcs();
                    Refresh();
                }

                if (result.WildEncounterTriggered)
                    LastMoveResult += " + Wild Encounter!";

                if (result.SpottedByNpcId != 0)
                    LastMoveResult += $" + Spotted by NPC {result.SpottedByNpcId}!";
            }
            else
            {
                LastMoveResult = $"Blocked ({direction})";
                OnPropertyChanged(nameof(FacingText));
            }
        }


        // ── Called after same-map move ───────────────────────────────────────
        // ── Called after same-map move ───────────────────────────────────────
        public void Refresh()
        {
            UpdatePlayerMarker();
            OnPropertyChanged(nameof(PlayerSquareRow));
            OnPropertyChanged(nameof(PlayerSquareCol));
            OnPropertyChanged(nameof(FacingText));
        }

        // ── Called after TickNpcs ────────────────────────────────────────────
        public void RefreshNpcs()
        {
            for (int r = 0; r < TileRows.Count; r++)
            {
                var row = TileRows[r];
                for (int c = 0; c < row.Cells.Count; c++)
                {
                    var cell = row.Cells[c];
                    cell.NpcId = NpcIdAt(r, c);
                    cell.NpcVisionId = (r < SquareMap.VisionLayer.GetLength(0) &&
                                        c < SquareMap.VisionLayer.GetLength(1))
                                       ? SquareMap.VisionLayer[r, c]
                                       : 0;
                }
            }
        }

        // ── Full rebuild after warp/connection ───────────────────────────────
        // ── Inside MapViewModel ──────────────────────────────────────────────────────
        // Replace RebuildGrid and UpdatePlayerMarker with these versions.
        // Everything else in MapViewModel stays the same.

        public void RebuildGrid()
        {
            TileRows.Clear();
            _currentPlayerCell = null;

            var (bg, fg, vision) = _mapManager.GetViewport();
            var layer = _isShowingBackground ? bg : fg;

            int viewRows = layer.GetLength(0);
            int viewCols = layer.GetLength(1);

            var (playerSr, playerSc) = SquareMap.TileToSquare(
                _player.playerLoc.x, _player.playerLoc.y);

            for (int r = 0; r < SquareMap.SquareRows; r++)
            {
                var rowVm = new TileRowViewModel();

                for (int c = 0; c < SquareMap.SquareCols; c++)
                {
                    var square = SquareMap.GetSquare(r, c);

                    // Map-space → viewport-space offset
                    int halfViewRows = viewRows / 2;
                    int halfViewCols = viewCols / 2;
                    int vr = r - playerSr + halfViewRows;
                    int vc = c - playerSc + halfViewCols;

                    int tileId = (vr >= 0 && vr < viewRows && vc >= 0 && vc < viewCols)
                        ? layer[vr, vc]
                        : 0;

                    // Vision layer is square-space — index directly
                    int visionId = (r < vision.GetLength(0) && c < vision.GetLength(1))
                        ? vision[r, c]
                        : 0;

                    // NPC presence — check if any NPC occupies this square
                    int npcId = NpcIdAt(r, c);

                    var cell = new TileCellViewModel
                    {
                        TileId = tileId,
                        Row = r,
                        Col = c,
                        Collision = square?.SquareType ?? CollisionType.Unwalkable,
                        IsPlayerHere = r == playerSr && c == playerSc,
                        NpcVisionId = visionId,
                        NpcId = npcId,
                    };

                    if (cell.IsPlayerHere)
                        _currentPlayerCell = cell;

                    rowVm.Cells.Add(cell);
                }

                TileRows.Add(rowVm);
            }

            OnPropertyChanged(nameof(MapName));
            OnPropertyChanged(nameof(MapWidth));
            OnPropertyChanged(nameof(MapHeight));
            OnPropertyChanged(nameof(SquareRows));
            OnPropertyChanged(nameof(SquareCols));
            OnPropertyChanged(nameof(PlayerSquareRow));
            OnPropertyChanged(nameof(PlayerSquareCol));
            OnPropertyChanged(nameof(FacingText));
        }

        private void UpdatePlayerMarker()
        {
            if (_currentPlayerCell != null)
                _currentPlayerCell.IsPlayerHere = false;

            var (sr, sc) = SquareMap.TileToSquare(
                _player.playerLoc.x, _player.playerLoc.y);

            if (sr < TileRows.Count && sc < TileRows[sr].Cells.Count)
            {
                _currentPlayerCell = TileRows[sr].Cells[sc];
                _currentPlayerCell.IsPlayerHere = true;

                // Sync vision and NPC presence on light refresh
                _currentPlayerCell.NpcVisionId = SquareMap.VisionLayer[sr, sc];
                _currentPlayerCell.NpcId = NpcIdAt(sr, sc);
            }

            CollisionAtCursor = SquareMap.GetCollision(sr, sc).ToString();
        }

        /// Returns the NPC Id if any NPC is standing on this square, else 0.
        private int NpcIdAt(int squareRow, int squareCol)
        {
            var npc = _mapManager.ActiveMap.Npc.FirstOrDefault(n =>
            {
                var (r, c) = SquareMap.TileToSquare(n.Location.x, n.Location.y);
                return r == squareRow && c == squareCol;
            });
            return npc?.NpcInfo.Id ?? 0;
        }

        // ── Internals ────────────────────────────────────────────────────────
        internal void SwitchLayer(bool background)
        {
            IsShowingBackground = background;
            IsShowingForeground = !background;
            RebuildGrid();
        }
    }

    // -----------------------------------------------------------------------
    // MapBootstrap — two connected maps: Pallet Town (south) ↔ Route 1 (north)
    // -----------------------------------------------------------------------
    public static class MapBootstrap
    {
        private const int TileWalkable = 1;
        private const int TileBlocked = 0;
        private const int TileWater = 50;
        private const int TileGrass = 40;
        private const int TileWarp = 60;   // new — IsWarp → id == 60
        private const int TileJumpDown = 70;
        private const int TileJumpUp = 71;
        private const int TileJumpLeft = 72;
        private const int TileJumpRight = 73;

        public static MapDomain CreatePlaceholderMap()
        {
            var palletTown = BuildPalletTown();
            var route1 = BuildRoute1();
            var palletHouse = BuildPalletHouse();

            // ── Map connections (walking off edge) ───────────────────────────
            palletTown.ConnectedMaps.Add(new ConnectedMapDomain
            {
                ConnectedMap = route1,
                ConnectionDirection = ConnectionDirection.North,
                Margin = 0,
            });
            route1.ConnectedMaps.Add(new ConnectedMapDomain
            {
                ConnectedMap = palletTown,
                ConnectionDirection = ConnectionDirection.South,
                Margin = 0,
            });

            // ── Warps (stepping on a tile) ───────────────────────────────────

            // Pallet Town → house: tile at square (8, 7), spawn at house square (1, 3)
            palletTown.Wraps.Add(new WrapDomain
            {
                WrapLoc = (8, 7),           // square-space position on Pallet Town
                TargetMap = palletHouse,
                SpawnLoc = (row: 1, col: 3), // square-space spawn inside the house
            });

            // House → Pallet Town: tile at square (3, 3), spawn back outside the door
            palletHouse.Wraps.Add(new WrapDomain
            {
                WrapLoc = (3, 3),
                TargetMap = palletTown,
                SpawnLoc = (row: 9, col: 7), // one step south of the entrance
            });
            
            return palletTown;
        }

        // ── Pallet Town — 30×30 ───────────────────────────────────────────────
        private static MapDomain BuildPalletTown()
        {
            const int width = 30, height = 30;
            var grid = new int[height, width];

            Fill(grid, TileWalkable);
            BorderWalls(grid, width, height, openNorthColStart: 13, openNorthColEnd: 17);

            // Grass patch to the south of spawn
            FillRect(grid, 10, 2, 4, 6, TileGrass);

            // Water patch
            FillRect(grid, 16, 20, 6, 8, TileWater);

            // Warp tile
            grid[18, 14] = TileWarp;

            // Jump down ledge — tile row 8, cols 2–10
            for (int c = 2; c <= 10; c++) grid[8, c] = TileJumpDown;

            // ── NPCs ─────────────────────────────────────────────────────────────────

            var npcs = new List<NpcObjectDomain>
            {
                // Youngster Joey — patrols 4 squares up/down near the grass patch,
                // facing the player with a 3-square sight line
                new NpcObjectDomain
                {
                    NpcInfo       = new NpcDomain { Id = 1, Name = "Youngster Joey" },
                    Location      = (12, 8),                // tile-space start (square 6,4)
                    MovementType  = MovementType.Walking,
                    direction     = FacingDirection.Up,
                    DirectionA    = FacingDirection.Up,
                    DirectionB    = FacingDirection.Down,
                    StepsPerLeg   = 4,
                    CollisionType = CollisionType.Unwalkable,
                    visionRange   = 3,
                    VisionType    = VisionType.Normal,
                },

                // Old Man — stationary, faces right, no vision
                new NpcObjectDomain
                {
                    NpcInfo       = new NpcDomain { Id = 2, Name = "Old Man" },
                    Location      = (6, 20),
                    MovementType  = MovementType.Stationery,
                    direction     = FacingDirection.Right,
                    DirectionA    = FacingDirection.Right,
                    DirectionB    = FacingDirection.Down,
                    CollisionType = CollisionType.Unwalkable,
                    visionRange   = 2,
                    StepsPerLeg = 5,
                    VisionType    = VisionType.Normal,
                },
            };

            return new MapDomain
            {
                Name = "Pallet Town",
                Width = width,
                Height = height,
                BackgroundBlocks = Flatten(grid, width, height),
                Blocks = new List<TileDomain>(),
                ConnectedMaps = new List<ConnectedMapDomain>(),
                Wraps = new List<WrapDomain>(),
                Npc = npcs,
            };
        }

        // ── Pallet House — 10×8 interior ─────────────────────────────────────
        private static MapDomain BuildPalletHouse()
        {
            const int width = 10, height = 8;
            var grid = new int[height, width];  

            Fill(grid, TileWalkable);

            // Four walls, no exits cut in — the warp tile IS the exit
            for (int c = 0; c < width; c++)
            {
                grid[0, c] = TileBlocked;
                grid[height - 1, c] = TileBlocked;
            }
            for (int r = 0; r < height; r++)
            {
                grid[r, 0] = TileBlocked;
                grid[r, width - 1] = TileBlocked;
            }

            // A small table/counter in the middle so it doesn't look empty
            FillRect(grid, 2, 3, 2, 4, TileBlocked);

            // Exit warp tile at the south wall centre = tile (6,6) → square (3,3)
            grid[6, 6] = TileWarp;

            return new MapDomain
            {
                Name = "Pallet House",
                Width = width,
                Height = height,
                BackgroundBlocks = Flatten(grid, width, height),
                Blocks = new List<TileDomain>(),
                ConnectedMaps = new List<ConnectedMapDomain>(),
                Wraps = new List<WrapDomain>(),
            };
        }

        // ── Route 1 — 30×20 (unchanged) ──────────────────────────────────────
        private static MapDomain BuildRoute1()
        {
            const int width = 30, height = 20;
            var grid = new int[height, width];

            Fill(grid, TileWalkable);
            BorderWalls(grid, width, height, openSouthColStart: 13, openSouthColEnd: 17);

            FillRect(grid, 2, 1, 16, 12, TileGrass);
            FillRect(grid, 2, 18, 16, 11, TileGrass);

            FillRect(grid, 4, 4, 2, 4, TileBlocked);
            FillRect(grid, 10, 6, 2, 4, TileBlocked);
            FillRect(grid, 6, 20, 2, 4, TileBlocked);
            FillRect(grid, 12, 22, 2, 4, TileBlocked);

            FillRect(grid, 14, 2, 4, 6, TileWater);

            return new MapDomain
            {
                Name = "Route 1",
                Width = width,
                Height = height,
                BackgroundBlocks = Flatten(grid, width, height),
                Blocks = new List<TileDomain>(),
                ConnectedMaps = new List<ConnectedMapDomain>(),
                Wraps = new List<WrapDomain>(),
            };
        }

        // ── Helpers (unchanged) ───────────────────────────────────────────────
        private static void Fill(int[,] grid, int id)
        {
            int rows = grid.GetLength(0), cols = grid.GetLength(1);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    grid[r, c] = id;
        }

        private static void BorderWalls(int[,] grid, int width, int height,
            int openNorthColStart = -1, int openNorthColEnd = -1,
            int openSouthColStart = -1, int openSouthColEnd = -1)
        {
            for (int r = 0; r < height; r++)
            {
                grid[r, 0] = TileBlocked;
                grid[r, width - 1] = TileBlocked;
            }
            for (int c = 0; c < width; c++)
            {
                if (c < openNorthColStart || c >= openNorthColEnd) grid[0, c] = TileBlocked;
                if (c < openSouthColStart || c >= openSouthColEnd) grid[height - 1, c] = TileBlocked;
            }
        }

        private static void FillRect(int[,] grid, int startRow, int startCol,
                                     int rowCount, int colCount, int tileId)
        {
            int maxR = Math.Min(startRow + rowCount, grid.GetLength(0));
            int maxC = Math.Min(startCol + colCount, grid.GetLength(1));
            for (int r = startRow; r < maxR; r++)
                for (int c = startCol; c < maxC; c++)
                    grid[r, c] = tileId;
        }

        private static List<TileDomain> Flatten(int[,] grid, int width, int height)
        {
            var tiles = new List<TileDomain>(width * height);
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    tiles.Add(new TileDomain { Tileid = grid[r, c] });
            return tiles;
        }
    }
}
