using System;
using System.Globalization;
using System.Windows.Data;

namespace PokemonGame.ViewModels.Converters
{
    public class TileToPixelConverter : IValueConverter
    {
        public int TileSize { get; set; } = 8;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int pos) return pos * TileSize;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
