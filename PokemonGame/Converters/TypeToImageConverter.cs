using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PokemonGame.Converters
{
    public class TypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string typeName) || string.IsNullOrWhiteSpace(typeName)) return null;

            try
            {
                var uri = new Uri($"pack://application:,,,/Assets/Images/Types/{typeName}" + "Type" + ".png");
                return new BitmapImage(uri);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}