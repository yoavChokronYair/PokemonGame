using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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

    
}
