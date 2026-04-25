using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Model.Model.Map;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage
{
    // -----------------------------------------------------------------------
    // ShowLayerCommand
    // -----------------------------------------------------------------------
    public class ShowLayerCommand : CommandBase
    {
        private readonly MapViewModel _vm;
        private readonly bool _background;

        public ShowLayerCommand(MapViewModel vm, bool background)
        {
            _vm = vm;
            _background = background;
        }

        public override void Execute(object? parameter) => _vm.SwitchLayer(_background);
    }

    // -----------------------------------------------------------------------
    // MoveCommand — bound to WASD / arrow keys from code-behind
    // -----------------------------------------------------------------------
    public class MoveCommand : CommandBase
    {
        private readonly MapViewModel _vm;
        private readonly FacingDirection _direction;

        public MoveCommand(MapViewModel vm, FacingDirection direction)
        {
            _vm = vm;
            _direction = direction;
        }

        public override void Execute(object? parameter) => _vm.Move(_direction);
    }

    // -----------------------------------------------------------------------
    // Cell — one square on the grid
    // -----------------------------------------------------------------------
    public class TileCellViewModel : ViewModelBase
    {
        private bool _isPlayerHere;
        private int _tileId;
        private int _row;
        private int _col;
        private CollisionType _collision;

        public int TileId
        {
            get => _tileId;
            set => SetProperty(ref _tileId, value);
        }

        public int Row
        {
            get => _row;
            set => SetProperty(ref _row, value);
        }

        public int Col
        {
            get => _col;
            set => SetProperty(ref _col, value);
        }

        public CollisionType Collision
        {
            get => _collision;
            set
            {
                if (SetProperty(ref _collision, value))
                {
                    OnPropertyChanged(nameof(TileForeground));
                    OnPropertyChanged(nameof(CellBackground));
                    OnPropertyChanged(nameof(Tooltip));
                }
            }
        }

        public bool IsPlayerHere
        {
            get => _isPlayerHere;
            set
            {
                if (SetProperty(ref _isPlayerHere, value))
                {
                    OnPropertyChanged(nameof(PlayerDotVisibility));
                    OnPropertyChanged(nameof(CellBackground));
                }
            }
        }

        public string PlayerDotVisibility => _isPlayerHere ? "Visible" : "Collapsed";

        public string Tooltip => $"[{Row},{Col}]  id:{TileId}  {Collision}";

        public Brush TileForeground => _collision switch
        {
            CollisionType.Unwalkable or CollisionType.Blocked
                => new SolidColorBrush(Color.FromRgb(0x55, 0x20, 0x20)),
            CollisionType.WildGrass
                => new SolidColorBrush(Color.FromRgb(0x3A, 0x7A, 0x3F)),
            CollisionType.HM
                => new SolidColorBrush(Color.FromRgb(0x2A, 0x60, 0x9A)),
            CollisionType.JumpLeft or CollisionType.JumpRight
                or CollisionType.JumpDown or CollisionType.JumpUp
                => new SolidColorBrush(Color.FromRgb(0x9A, 0x8A, 0x2A)),
            _ => new SolidColorBrush(Color.FromRgb(0x55, 0x88, 0x55)),
        };

        public Brush CellBackground
        {
            get
            {
                if (_isPlayerHere)
                    return new SolidColorBrush(Color.FromRgb(0x33, 0x11, 0x11));

                return _collision switch
                {
                    CollisionType.Unwalkable or CollisionType.Blocked
                        => new SolidColorBrush(Color.FromRgb(0x18, 0x08, 0x08)),
                    CollisionType.WildGrass
                        => new SolidColorBrush(Color.FromRgb(0x08, 0x12, 0x08)),
                    CollisionType.HM
                        => new SolidColorBrush(Color.FromRgb(0x08, 0x0C, 0x18)),
                    _ => new SolidColorBrush(Color.FromRgb(0x10, 0x10, 0x10)),
                };
            }
        }
    }

    // -----------------------------------------------------------------------
    // Row — one horizontal strip of cells
    // -----------------------------------------------------------------------
    public class TileRowViewModel
    {
        public ObservableCollection<TileCellViewModel> Cells { get; } = new();
    }

    // -----------------------------------------------------------------------
    // MapViewModel
    // -----------------------------------------------------------------------
    public class MapViewModel : ViewModelBase
    {
        private readonly MapManager _mapManager;
        private SquareMapState _squareMapState;
        private readonly PlayerDomain _player;

        private bool _isShowingBackground = true;
        private bool _isShowingForeground;
        private string _collisionAtCursor = string.Empty;
        private string _lastMoveResult = string.Empty;
        private TileCellViewModel? _currentPlayerCell;

        public ObservableCollection<TileRowViewModel> TileRows { get; } = new();

        // ── Header ──────────────────────────────────────────────────────────
        public string MapName => _mapManager.ActiveMap.Name;
        public int MapWidth => _mapManager.ActiveMap.Width;
        public int MapHeight => _mapManager.ActiveMap.Height;
        public int SquareRows => _squareMapState.SquareRows;
        public int SquareCols => _squareMapState.SquareCols;

        public int PlayerSquareRow
            => _squareMapState.TileToSquare(_player.playerLoc.x, _player.playerLoc.y).row;
        public int PlayerSquareCol
            => _squareMapState.TileToSquare(_player.playerLoc.x, _player.playerLoc.y).col;

        public string FacingText => _player.facingDirection.ToString();

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

        // ── Constructor ─────────────────────────────────────────────────────
        public MapViewModel()
        {
            _player = PlayerDomain.Instance;

            _player.CurrentMap ??= MapBootstrap.CreatePlaceholderMap();
            _player.playerLoc = _player.playerLoc == default ? (4, 4) : _player.playerLoc;

            _squareMapState = new SquareMapState(_player.CurrentMap);
            _mapManager = new MapManager(_player);

            ShowBackgroundCommand = new ShowLayerCommand(this, background: true);
            ShowForegroundCommand = new ShowLayerCommand(this, background: false);

            MoveUpCommand = new MoveCommand(this, FacingDirection.Up);
            MoveDownCommand = new MoveCommand(this, FacingDirection.Down);
            MoveLeftCommand = new MoveCommand(this, FacingDirection.Left);
            MoveRightCommand = new MoveCommand(this, FacingDirection.Right);

            RebuildGrid();
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
                    _squareMapState = new SquareMapState(_mapManager.ActiveMap);
                    RebuildGrid();
                }
                else
                {
                    Refresh();
                }
            }
            else
            {
                LastMoveResult = $"Blocked ({direction})";
                OnPropertyChanged(nameof(FacingText));
            }
        }

        // ── Called after same-map move ───────────────────────────────────────
        public void Refresh()
        {
            UpdatePlayerMarker();
            OnPropertyChanged(nameof(PlayerSquareRow));
            OnPropertyChanged(nameof(PlayerSquareCol));
            OnPropertyChanged(nameof(FacingText));
        }

        // ── Full rebuild after warp/connection ───────────────────────────────
        public void RebuildGrid()
        {
            TileRows.Clear();
            _currentPlayerCell = null;

            var (bg, fg) = _mapManager.GetViewport();
            var layer = _isShowingBackground ? bg : fg;

            int rows = layer.GetLength(0);
            int cols = layer.GetLength(1);

            var (playerSr, playerSc) = _squareMapState.TileToSquare(
                _player.playerLoc.x, _player.playerLoc.y);

            for (int r = 0; r < _squareMapState.SquareRows; r++)
            {
                var rowVm = new TileRowViewModel();

                for (int c = 0; c < _squareMapState.SquareCols; c++)
                {
                    var square = _squareMapState.GetSquare(r, c);
                    var (tr, tc) = _squareMapState.SquareToTile(r, c);

                    var cell = new TileCellViewModel
                    {
                        TileId = (tr < rows && tc < cols) ? layer[tr, tc] : 0,
                        Row = r,
                        Col = c,
                        Collision = square?.SquareType ?? CollisionType.Unwalkable,
                        IsPlayerHere = (r == playerSr && c == playerSc),
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

        // ── Internals ────────────────────────────────────────────────────────
        internal void SwitchLayer(bool background)
        {
            IsShowingBackground = background;
            IsShowingForeground = !background;
            RebuildGrid();
        }

        private void UpdatePlayerMarker()
        {
            if (_currentPlayerCell != null)
                _currentPlayerCell.IsPlayerHere = false;

            var (sr, sc) = _squareMapState.TileToSquare(
                _player.playerLoc.x, _player.playerLoc.y);

            if (sr < TileRows.Count && sc < TileRows[sr].Cells.Count)
            {
                _currentPlayerCell = TileRows[sr].Cells[sc];
                _currentPlayerCell.IsPlayerHere = true;
            }

            CollisionAtCursor = _squareMapState.GetCollision(sr, sc).ToString();
        }
    }

    // -----------------------------------------------------------------------
    // MapBootstrap — two connected maps: Pallet Town (south) ↔ Route 1 (north)
    // -----------------------------------------------------------------------
    public static class MapBootstrap
    {
        // Tile IDs — must match ResolveSquareType ranges in SquareMapState
        private const int TileWalkable = 1;
        private const int TileBlocked = 0;   // IsBlocked → id == 0
        private const int TileWater = 50;  // IsWater   → 50–59
        private const int TileGrass = 40;  // IsGrass   → 40–49

        public static MapDomain CreatePlaceholderMap()
        {
            // Build both maps first so we can cross-reference them
            var palletTown = BuildPalletTown();
            var route1 = BuildRoute1();

            // Connect: Pallet Town north edge → Route 1
            palletTown.ConnectedMaps.Add(new ConnectedMapDomain
            {
                ConnectedMap = route1,
                ConnectionDirection = ConnectionDirection.North,
                Margin = 0,
            });

            // Connect: Route 1 south edge → Pallet Town
            route1.ConnectedMaps.Add(new ConnectedMapDomain
            {
                ConnectedMap = palletTown,
                ConnectionDirection = ConnectionDirection.South,
                Margin = 0,
            });

            return palletTown;
        }

        // ── Pallet Town — 30×30, open town feel ──────────────────────────────
        private static MapDomain BuildPalletTown()
        {
            const int width = 30;
            const int height = 30;
            var grid = new int[height, width];

            Fill(grid, TileWalkable);

            // Border walls — leave top row open (cols 13–16) as the north exit
            BorderWalls(grid, width, height, openNorthColStart: 13, openNorthColEnd: 17);

            // Interior wall with a gap to create two rooms
            for (int c = 1; c < 12; c++) grid[14, c] = TileBlocked;
            for (int c = 18; c < width - 1; c++) grid[14, c] = TileBlocked;

            // Vertical wall segment
            for (int r = 6; r < 14; r++) grid[r, 20] = TileBlocked;
            for (int r = 6; r < 14; r++) grid[r, 21] = TileBlocked;

            // Grass patches
            FillRect(grid, 2, 2, 4, 6, TileGrass);
            FillRect(grid, 2, 22, 4, 6, TileGrass);
            FillRect(grid, 16, 2, 6, 4, TileGrass);
            FillRect(grid, 22, 22, 6, 6, TileGrass);

            // Water
            FillRect(grid, 16, 20, 6, 8, TileWater);

            return new MapDomain
            {
                Name = "Pallet Town",
                Width = width,
                Height = height,
                BackgroundBlocks = Flatten(grid, width, height),
                Blocks = new List<TileDomain>(),
                ConnectedMaps = new List<ConnectedMapDomain>(),
                Wraps = new List<WrapDomain>(),
            };
        }

        // ── Route 1 — 30×20, tall grass route heading north ─────────────────
        private static MapDomain BuildRoute1()
        {
            const int width = 30;
            const int height = 20;
            var grid = new int[height, width];

            Fill(grid, TileWalkable);

            // Border walls — leave bottom row open (cols 13–16) matching Pallet Town exit
            BorderWalls(grid, width, height, openSouthColStart: 13, openSouthColEnd: 17);

            // Dense grass on both sides of a central path (cols 12–17 stay walkable)
            FillRect(grid, 2, 1, 16, 12, TileGrass);
            FillRect(grid, 2, 18, 16, 11, TileGrass);

            // Rocky blocked sections breaking up the grass
            FillRect(grid, 4, 4, 2, 4, TileBlocked);
            FillRect(grid, 10, 6, 2, 4, TileBlocked);
            FillRect(grid, 6, 20, 2, 4, TileBlocked);
            FillRect(grid, 12, 22, 2, 4, TileBlocked);

            // Small pond
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

        // ── Helpers ──────────────────────────────────────────────────────────
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
                // Top wall — skip open passage
                if (c < openNorthColStart || c >= openNorthColEnd)
                    grid[0, c] = TileBlocked;

                // Bottom wall — skip open passage
                if (c < openSouthColStart || c >= openSouthColEnd)
                    grid[height - 1, c] = TileBlocked;
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
