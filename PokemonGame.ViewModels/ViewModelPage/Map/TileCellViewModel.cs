using System.Collections.ObjectModel;
using System.Windows.Media;
using PokemonGame.Model.Enums;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Map
{
    // -----------------------------------------------------------------------
    // Row — one horizontal strip of cells
    // -----------------------------------------------------------------------
    public class TileRowViewModel
    {
        public ObservableCollection<TileCellViewModel> Cells { get; } = new();
    }

    // -----------------------------------------------------------------------
    // Cell
    // -----------------------------------------------------------------------
    public class TileCellViewModel : ViewModelBase
    {
        // ── Backing fields ───────────────────────────────────────────────────
        private int _tileId;
        private int _row;
        private int _col;
        private CollisionType _collision;
        private bool _isPlayerHere;
        private int _npcId;        // >0 = an NPC is standing on this square
        private int _npcVisionId;  // >0 = an NPC's sight line covers this square

        // ── Basic tile data ──────────────────────────────────────────────────
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
                    RefreshVisuals();
            }
        }

        // ── Player ───────────────────────────────────────────────────────────
        public bool IsPlayerHere
        {
            get => _isPlayerHere;
            set
            {
                if (SetProperty(ref _isPlayerHere, value))
                    RefreshVisuals();
            }
        }

        // ── NPC presence (the NPC is standing on this square) ────────────────
        public int NpcId
        {
            get => _npcId;
            set
            {
                if (SetProperty(ref _npcId, value))
                {
                    OnPropertyChanged(nameof(IsNpcHere));
                    OnPropertyChanged(nameof(NpcSymbol));
                    RefreshVisuals();
                }
            }
        }

        public bool IsNpcHere => _npcId != 0;

        /// Single character stamped over the tile to represent the NPC.
        /// Odd IDs → "T" (trainer), Even IDs → "N" (neutral NPC); adjust as needed.
        public string NpcSymbol => _npcId switch
        {
            0 => string.Empty,
            _ when _npcId % 2 != 0 => "T",   // trainer
            _ => "N",   // generic NPC
        };

        // ── NPC vision (a sight line from some NPC covers this square) ───────
        public int NpcVisionId
        {
            get => _npcVisionId;
            set
            {
                if (SetProperty(ref _npcVisionId, value))
                {
                    OnPropertyChanged(nameof(IsInNpcVision));
                    RefreshVisuals();
                }
            }
        }

        public bool IsInNpcVision => _npcVisionId != 0;

        // ── Derived visuals ───────────────────────────────────────────────────

        public string PlayerDotVisibility => _isPlayerHere ? "Visible" : "Collapsed";

        /// Visibility string for the NPC symbol overlay (used in XAML).
        public string NpcSymbolVisibility => IsNpcHere ? "Visible" : "Collapsed";

        public string Tooltip =>
            $"[{Row},{Col}]  id:{TileId}  {Collision}" +
            (IsNpcHere ? $"  NPC:{_npcId}" : string.Empty) +
            (IsInNpcVision ? $"  seen-by:{_npcVisionId}" : string.Empty);

        // Text colour on top of each tile (tile-type indicator)
        public Brush TileForeground => _collision switch
        {
            CollisionType.Blocked or CollisionType.Blocked
                => Rgb(0x55, 0x20, 0x20),
            CollisionType.WildGrass
                => Rgb(0x3A, 0x7A, 0x3F),
            CollisionType.HM
                => Rgb(0x2A, 0x60, 0x9A),
            CollisionType.JumpLeft or CollisionType.JumpRight
                or CollisionType.JumpDown or CollisionType.JumpUp
                => Rgb(0x9A, 0x8A, 0x2A),
            _ => Rgb(0x55, 0x88, 0x55),
        };

        // NPC symbol colour — bright orange so it reads over any tile colour
        public Brush NpcSymbolForeground => Rgb(0xFF, 0x99, 0x00);

        // Cell background — priority: player > NPC > vision tint > terrain
        public Brush CellBackground
        {
            get
            {
                if (_isPlayerHere)
                    return Rgb(0x33, 0x11, 0x11);   // deep red — player

                if (IsNpcHere)
                    return Rgb(0x1A, 0x1A, 0x3A);   // deep indigo — NPC standing tile

                if (IsInNpcVision)
                    return _collision switch           // vision tint on top of terrain colour
                    {
                        CollisionType.WildGrass
                            => Rgb(0x10, 0x22, 0x0E), // vision-in-grass: darker teal-green
                        CollisionType.HM
                            => Rgb(0x0A, 0x10, 0x28), // vision-in-water: darker navy
                        CollisionType.Blocked or CollisionType.Blocked
                            => Rgb(0x20, 0x08, 0x08), // shouldn't usually be seen through walls
                        _ => Rgb(0x18, 0x18, 0x2E),   // vision on walkable: soft purple tint
                    };

                return _collision switch              // plain terrain colours
                {
                    CollisionType.Blocked or CollisionType.Blocked
                        => Rgb(0x18, 0x08, 0x08),
                    CollisionType.WildGrass
                        => Rgb(0x08, 0x12, 0x08),
                    CollisionType.HM
                        => Rgb(0x08, 0x0C, 0x18),
                    CollisionType.JumpLeft or CollisionType.JumpRight
                        or CollisionType.JumpDown or CollisionType.JumpUp
                        => Rgb(0x18, 0x16, 0x06),
                    _ => Rgb(0x10, 0x10, 0x10),
                };
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// Fire all visual property-change notifications in one call.
        private void RefreshVisuals()
        {
            OnPropertyChanged(nameof(CellBackground));
            OnPropertyChanged(nameof(TileForeground));
            OnPropertyChanged(nameof(PlayerDotVisibility));
            OnPropertyChanged(nameof(NpcSymbolVisibility));
            OnPropertyChanged(nameof(NpcSymbolForeground));
            OnPropertyChanged(nameof(Tooltip));
        }

        private static SolidColorBrush Rgb(byte r, byte g, byte b)
            => new(Color.FromRgb(r, g, b));
    }
}