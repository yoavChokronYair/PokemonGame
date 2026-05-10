using System;
using System.Globalization;
using System.Windows.Data;

namespace PokemonGame.Converters
{
    public class BadgeIdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is int badgeId)
            {
                return $"/Assets/Images/Badges/{badgeId}.png";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}