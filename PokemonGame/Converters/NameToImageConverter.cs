using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PokemonGame.Converters
{
    public class NameToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imageName = value as string;

            if (string.IsNullOrWhiteSpace(imageName))
                return null;

            try
            {
                // Ensure your images are in /Assets/Items/ and Build Action is set to "Resource"
                string uriPath = $"pack://application:,,,/Assets/Images/Items/{imageName}.png";
                return new BitmapImage(new Uri(uriPath));
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}