using System;
using System.Globalization;
using System.Windows.Data;


namespace PokemonGame.Converters
{
    public class HpToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return 0d;
            if (!double.TryParse(values[0]?.ToString(), out var current)) return 0d;
            if (!double.TryParse(values[1]?.ToString(), out var max) || max <= 0) return 0d;

            double barWidth = parameter is string p && double.TryParse(p, out var pw) ? pw : 120d;
            return Math.Max(0d, Math.Min(current / max * barWidth, barWidth));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
