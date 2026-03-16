using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PokemonGame.Converters
{
    public class TypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Gray;

            string pokemonType = value.ToString().ToUpper();

            switch (pokemonType)
            {
                case "FIRE":
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#F08030");
                case "WATER":
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#6890F0");
                case "GRASS":
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#78C850");
                case "DRAGON":
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#7038F8");
                case "NORMAL":
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#A8A878");
                case "ELECTRIC":
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#F8D030");
                default:
                    return Brushes.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}