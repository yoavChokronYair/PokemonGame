using System;
using System.Globalization;
using System.Windows.Data;

namespace PokemonGame.Converters
{
    public class ThirdOfHeightConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is double h ? Math.Max(32, h / 3.0) : 40.0;

        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }
}
