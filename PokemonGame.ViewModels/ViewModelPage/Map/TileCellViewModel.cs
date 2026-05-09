using System.Collections.ObjectModel;
using System.Windows.Media;
using PokemonGame.Model.Enums;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Map
{
    public class TileRowViewModel
    {
        public ObservableCollection<TileCellViewModel> Cells { get; } = new();
    }

    public class TileCellViewModel : ViewModelBase
    {
        // ── Backing fields ───────────────────────────────────────────────────
        private int _tileId;
        private int _row;
        private int _col;
        private CollisionType _collision;
        private bool _isPlayerHere;
        private int _npcId;
        private int _npcVisionId;
        private ImageSource? _tileImage;
        private bool _isForeground;

        // ── Basic tile data ──────────────────────────────────────────────────
        public int TileId { get => _tileId; set => SetProperty(ref _tileId, value); }
        public int Row { get => _row; set => SetProperty(ref _row, value); }
        public int Col { get => _col; set => SetProperty(ref _col, value); }
        public ImageSource? TileImage { get => _tileImage; set => SetProperty(ref _tileImage, value); }
        public bool IsForeground { get => _isForeground; set => SetProperty(ref _isForeground, value); }

        public CollisionType Collision
        {
            get => _collision;
            set { if (SetProperty(ref _collision, value)) RefreshVisuals(); }
        }

        // ── Player & NPC ─────────────────────────────────────────────────────
        public bool IsPlayerHere
        {
            get => _isPlayerHere;
            set { if (SetProperty(ref _isPlayerHere, value)) RefreshVisuals(); }
        }

        public int NpcId
        {
            get => _npcId;
            set { if (SetProperty(ref _npcId, value)) RefreshVisuals(); }
        }

        public int NpcVisionId
        {
            get => _npcVisionId;
            set { if (SetProperty(ref _npcVisionId, value)) RefreshVisuals(); }
        }

        // ── THE FIX: Master Update Method ────────────────────────────────────
        /// <summary>
        /// Updates the cell data all at once and forces WPF to redraw the UI.
        /// This prevents the "stuck grid" bug when adjacent tiles are identical.
        /// </summary>
        public void UpdateCell(int mapX, int mapY, int tileId, ImageSource? tileImage, CollisionType collision, bool isPlayerHere, int npcId = 0)
        {
            _row = mapX;
            _col = mapY;
            _tileId = tileId;
            _tileImage = tileImage;
            _collision = collision;
            _isPlayerHere = isPlayerHere;
            _npcId = npcId;

            OnPropertyChanged(string.Empty); // This should already be here
            OnPropertyChanged(nameof(DebugText)); // Add this to be safe
        }

        // ── Derived visuals (Read-Only) ──────────────────────────────────────
        public bool IsNpcHere => _npcId != 0;
        public bool IsInNpcVision => _npcVisionId != 0;

        public string NpcSymbol => _npcId switch
        {
            0 => string.Empty,
            _ when _npcId % 2 != 0 => "T",
            _ => "N",
        };
        // Add this property to TileCellViewModel
        public string DebugText => $"[{Row},{Col}]\nID:{TileId}";
        public string PlayerDotVisibility => _isPlayerHere ? "Visible" : "Collapsed";
        public string NpcSymbolVisibility => IsNpcHere ? "Visible" : "Collapsed";

        public string Tooltip =>
            $"[{Row},{Col}]  id:{TileId}  {Collision}" +
            (IsNpcHere ? $"  NPC:{_npcId}" : string.Empty) +
            (IsInNpcVision ? $"  seen-by:{_npcVisionId}" : string.Empty);

        public Brush TileForeground => _collision switch
        {
            CollisionType.Blocked => Rgb(0x55, 0x20, 0x20),
            CollisionType.WildGrass => Rgb(0x3A, 0x7A, 0x3F),
            CollisionType.HM => Rgb(0x2A, 0x60, 0x9A),
            CollisionType.JumpLeft or CollisionType.JumpRight or CollisionType.JumpDown or CollisionType.JumpUp => Rgb(0x9A, 0x8A, 0x2A),
            _ => Rgb(0x55, 0x88, 0x55),
        };

        public Brush NpcSymbolForeground => Rgb(0xFF, 0x99, 0x00);

        public Brush CellBackground
        {
            get
            {
                if (_isPlayerHere) return Rgb(0x33, 0x11, 0x11);
                if (IsNpcHere) return Rgb(0x1A, 0x1A, 0x3A);

                if (IsInNpcVision)
                    return _collision switch
                    {
                        CollisionType.WildGrass => Rgb(0x10, 0x22, 0x0E),
                        CollisionType.HM => Rgb(0x0A, 0x10, 0x28),
                        CollisionType.Blocked => Rgb(0x20, 0x08, 0x08),
                        _ => Rgb(0x18, 0x18, 0x2E),
                    };

                return _collision switch
                {
                    CollisionType.Blocked => Rgb(0x18, 0x08, 0x08),
                    CollisionType.WildGrass => Rgb(0x08, 0x12, 0x08),
                    CollisionType.HM => Rgb(0x08, 0x0C, 0x18),
                    CollisionType.JumpLeft or CollisionType.JumpRight or CollisionType.JumpDown or CollisionType.JumpUp => Rgb(0x18, 0x16, 0x06),
                    _ => Rgb(0x10, 0x10, 0x10),
                };
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private void RefreshVisuals()
        {
            OnPropertyChanged(nameof(CellBackground));
            OnPropertyChanged(nameof(TileForeground));
            OnPropertyChanged(nameof(PlayerDotVisibility));
            OnPropertyChanged(nameof(NpcSymbolVisibility));
            OnPropertyChanged(nameof(NpcSymbolForeground));
            OnPropertyChanged(nameof(Tooltip));
            OnPropertyChanged(nameof(IsNpcHere));
            OnPropertyChanged(nameof(IsInNpcVision));
            OnPropertyChanged(nameof(NpcSymbol));
        }

        private static SolidColorBrush Rgb(byte r, byte g, byte b) => new(Color.FromRgb(r, g, b));
    }
}