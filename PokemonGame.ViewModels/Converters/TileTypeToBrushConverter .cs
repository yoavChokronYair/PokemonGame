using PokemonGame.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;


namespace PokemonGame.ViewModels.Converters
{
    public class TileTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TileType type)
            {
                return type switch
                {
                    TileType.Grass => Brushes.Green,
                    TileType.Water => Brushes.Blue,
                    TileType.Mountain => Brushes.Gray,
                    TileType.None => Brushes.BurlyWood,
                    _ => Brushes.BurlyWood
                };
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
