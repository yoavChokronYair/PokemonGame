using PokemonGame.ViewModel.ViewModelHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static PokemonGame.Model.Map.GameMap;

namespace PokemonGame.ViewModel
{
    public class TileViewModel:ViewModelBase
    {
        private double _width;
        public double Width
        {
            get => _width;
            set 
            {
                _width = value; 
                OnPropertyChanged(nameof(Width));
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(nameof(Height)); }
        }

        private Brush _color;
        public Brush Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }
        private double _x1;
        public double X1
        {
            get => _x1;
            set
            {
                if (_x1 != value)
                {
                    _x1 = value;
                    OnPropertyChanged(nameof(X1));
                }
            }
        }

        private double _y1;
        public double Y1
        {
            get => _y1;
            set
            {
                if (_y1 != value)
                {
                    _y1 = value;
                    OnPropertyChanged(nameof(Y1));
                }
            }
        }
        private double _x2;
        public double X2
        {
            get => _x2;
            set
            {
                if (_x2 != value)
                {
                    _x2 = value;
                    OnPropertyChanged(nameof(X2));
                }
            }
        }

        private double _y2;
        public double Y2
        {
            get => _y2;
            set
            {
                if (_y2 != value)
                {
                    _y2 = value;
                    OnPropertyChanged(nameof(Y2));
                }
            }
        }
        public void UpdateFrom(TileRenderInfo info)
        {
            Width = info.Width;
            Height = info.Height;
            X1 = info.X1;
            Y1 = info.Y1;
            X2 = info.X2;
            Y2 = info.Y2;
            Color = info.Color;
        }


    }
}

