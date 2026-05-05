using System;
using System.Globalization;
using System.Windows.Data;
using PokemonGame.ViewModels.Store;
using PokemonGame.Views.Controls;

namespace PokemonGame.Converters
{
    public class BackgroundToBackgroundTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Background bg)
            {
                if (bg == Background.Blue) return BackgroundType.Blue;
                if (bg == Background.Red) return BackgroundType.Red;
            }
            return BackgroundType.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
