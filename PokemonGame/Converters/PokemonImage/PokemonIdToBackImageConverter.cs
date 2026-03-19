using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PokemonGame.Converters
{
    public class PokemonIdToBackImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int id)) return null;
            try { return new BitmapImage(new Uri($"pack://application:,,,/Assets/Images/PokemonSprites/back/{id}.png")); }
            catch { return null; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
