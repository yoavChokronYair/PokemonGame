using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PokemonGame.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOnline && isOnline)
            {
                // Green for online
                return new SolidColorBrush(Colors.ForestGreen);
            }

            // Dim grey for offline
            return new SolidColorBrush(Colors.DimGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}