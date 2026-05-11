using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PokemonGame.Converters
{
    public class MenuHighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int current = (int)value;
            int index = int.Parse(parameter.ToString());
            return current == index
                ? new SolidColorBrush(Color.FromRgb(80, 80, 180))
                : Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
