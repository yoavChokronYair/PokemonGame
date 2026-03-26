using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PokemonGame.Converters
{
    public class CategoryToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string category) || string.IsNullOrWhiteSpace(category)) return null;

            try
            {
                var uri = new Uri($"pack://application:,,,/Assets/Images/MoveCategories/{category}IC.png");
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