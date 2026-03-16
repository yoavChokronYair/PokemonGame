using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PokemonGame.Converters
{
    public class IntEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is int i && p is string s && int.TryParse(s, out int pi) && i == pi
                ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }
}
