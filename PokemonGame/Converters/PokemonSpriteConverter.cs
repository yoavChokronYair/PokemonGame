using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PokemonGame.Converters
{
    public class PokemonSpriteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int id)) return null;

            // Load your existing image however you normally do
            var uri = new Uri($"pack://application:,,,/Assets/Images/PokemonSprites/{id}.png");
            var original = new BitmapImage(uri);

            return TrimAndScaleSprite(new BitmapImage(uri), displayMaxSize: 160);
        }

        public static BitmapSource TrimAndScaleSprite(BitmapImage source, int displayMaxSize = 192)
        {
            var formatted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            int width = formatted.PixelWidth;
            int height = formatted.PixelHeight;
            int stride = width * 4;
            var pixels = new byte[height * stride];
            formatted.CopyPixels(pixels, stride, 0);

            int top = 0, bottom = height - 1, left = 0, right = width - 1;

            for (int y = 0; y < height; y++)
                if (Enumerable.Range(0, width).Any(x => pixels[y * stride + x * 4 + 3] > 10))
                { top = y; break; }

            for (int y = height - 1; y >= 0; y--)
                if (Enumerable.Range(0, width).Any(x => pixels[y * stride + x * 4 + 3] > 10))
                { bottom = y; break; }

            for (int x = 0; x < width; x++)
                if (Enumerable.Range(top, bottom - top + 1).Any(y => pixels[y * stride + x * 4 + 3] > 10))
                { left = x; break; }

            for (int x = width - 1; x >= 0; x--)
                if (Enumerable.Range(top, bottom - top + 1).Any(y => pixels[y * stride + x * 4 + 3] > 10))
                { right = x; break; }

            int cropW = right - left + 1;
            int cropH = bottom - top + 1;

            // How much of the original canvas does this pokemon fill? (0.0 - 1.0)
            double fillRatio = Math.Max((double)cropW / width, (double)cropH / height);

            // Scale display size proportionally — a pokemon filling 100% of canvas gets displayMaxSize
            int scaledSize = (int)Math.Round(displayMaxSize * fillRatio);
            scaledSize = Math.Max(scaledSize, 32); // minimum so tiny mons aren't invisible

            var cropped = new CroppedBitmap(formatted, new Int32Rect(left, top, cropW, cropH));

            // Scale up with NearestNeighbor to keep pixel art sharp
            var scaled = new TransformedBitmap(cropped, new ScaleTransform(
                (double)scaledSize / cropW,
                (double)scaledSize / cropH
            ));

            return scaled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class PokemonSpriteConverterBack : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int id)) return null;

            // Load your existing image however you normally do
            var uri = new Uri($"pack://application:,,,/Assets/Images/PokemonSprites/back/{id}.png");
            var original = new BitmapImage(uri);

            return TrimAndScaleSprite(new BitmapImage(uri), displayMaxSize: 160);
        }

        public static BitmapSource TrimAndScaleSprite(BitmapImage source, int displayMaxSize = 192)
        {
            var formatted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            int width = formatted.PixelWidth;
            int height = formatted.PixelHeight;
            int stride = width * 4;
            var pixels = new byte[height * stride];
            formatted.CopyPixels(pixels, stride, 0);

            int top = 0, bottom = height - 1, left = 0, right = width - 1;

            for (int y = 0; y < height; y++)
                if (Enumerable.Range(0, width).Any(x => pixels[y * stride + x * 4 + 3] > 10))
                { top = y; break; }

            for (int y = height - 1; y >= 0; y--)
                if (Enumerable.Range(0, width).Any(x => pixels[y * stride + x * 4 + 3] > 10))
                { bottom = y; break; }

            for (int x = 0; x < width; x++)
                if (Enumerable.Range(top, bottom - top + 1).Any(y => pixels[y * stride + x * 4 + 3] > 10))
                { left = x; break; }

            for (int x = width - 1; x >= 0; x--)
                if (Enumerable.Range(top, bottom - top + 1).Any(y => pixels[y * stride + x * 4 + 3] > 10))
                { right = x; break; }

            int cropW = right - left + 1;
            int cropH = bottom - top + 1;

            // How much of the original canvas does this pokemon fill? (0.0 - 1.0)
            double fillRatio = Math.Max((double)cropW / width, (double)cropH / height);

            // Scale display size proportionally — a pokemon filling 100% of canvas gets displayMaxSize
            int scaledSize = (int)Math.Round(displayMaxSize * fillRatio);
            scaledSize = Math.Max(scaledSize, 32); // minimum so tiny mons aren't invisible

            var cropped = new CroppedBitmap(formatted, new Int32Rect(left, top, cropW, cropH));

            // Scale up with NearestNeighbor to keep pixel art sharp
            var scaled = new TransformedBitmap(cropped, new ScaleTransform(
                (double)scaledSize / cropW,
                (double)scaledSize / cropH
            ));

            return scaled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
