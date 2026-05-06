using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace PokemonGame.Converters
{
    public class RatingToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return 0d;
            if (!double.TryParse(values[0]?.ToString(), out var current)) return 0d;
            if (!double.TryParse(values[1]?.ToString(), out var max) || max <= 0) return 0d;

            double barWidth = parameter is string p && double.TryParse(p, out var pw) ? pw : 200d;
            var value = (current / max) * barWidth;
            return Math.Max(0, Math.Min(value, barWidth));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
