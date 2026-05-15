using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PokemonGame.Converters
{
    public class BoolToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? new Thickness(2) : new Thickness(0);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();


    }
}
