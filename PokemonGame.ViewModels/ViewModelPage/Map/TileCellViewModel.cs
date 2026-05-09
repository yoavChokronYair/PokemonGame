using System.Collections.ObjectModel;
using System.Windows.Media;
using PokemonGame.Model.Enums;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Map
{
    // Lightweight data bag — no INotifyPropertyChanged.
    // The Canvas re-renders the whole viewport at once via MapViewModel,
    // so per-cell change notifications are unnecessary overhead.
    public class TileCellData
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int TileId { get; set; }
        public ImageSource? TileImage { get; set; }
        public bool IsPlayerHere { get; set; }
        public int NpcId { get; set; }
        public int NpcVisionId { get; set; }
        public CollisionType Collision { get; set; }

        // Canvas position in pixels — set by MapViewModel, read by the Canvas ItemsControl
        public double CanvasLeft { get; set; }
        public double CanvasTop { get; set; }

        // Derived — used by the Canvas overlay
        public bool IsNpcHere => NpcId != 0;
        public bool IsInNpcVision => NpcVisionId != 0;

        public string NpcSymbol => NpcId switch
        {
            0 => string.Empty,
            _ when NpcId % 2 != 0 => "T",
            _ => "N",
        };

        public string Tooltip =>
            $"[{Row},{Col}]  {Collision}" +
            (IsNpcHere ? $"  NPC:{NpcId}" : string.Empty) +
            (IsInNpcVision ? $"  seen-by:{NpcVisionId}" : string.Empty);
    }

    // Keep TileRowViewModel only as a thin row wrapper used by the Canvas helper.
    public class TileRowViewModel
    {
        public List<TileCellData> Cells { get; } = new();
    }
}