using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using PokemonGame.ViewModels.ViewModelPage;

namespace PokemonGame.Views.UserControls
{
    public sealed class MapOverlayVisual : FrameworkElement
    {
        private static readonly Brush PlayerFill = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x33)));
        private static readonly Pen PlayerStroke = Freeze(new Pen(new SolidColorBrush(Color.FromRgb(0xFF, 0x88, 0x88)), 1));
        private static readonly Brush NpcBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0x99, 0x00)));
        private static readonly Brush VisionBrush = Freeze(new SolidColorBrush(Color.FromArgb(0x33, 0x0D, 0x0D, 0x3A)));
        private static readonly Brush DebugTextBrush = Freeze(new SolidColorBrush(Colors.White));

        private static readonly Typeface NpcTypeface = new Typeface(new FontFamily("Consolas"),
            FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        private static readonly Typeface DebugTypeface = new Typeface(new FontFamily("Consolas"),
            FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(IReadOnlyList<CanvasOverlayItem>),
                typeof(MapOverlayVisual),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public IReadOnlyList<CanvasOverlayItem> Items
        {
            get => (IReadOnlyList<CanvasOverlayItem>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            var items = Items;
            if (items == null || items.Count == 0) return;

            const double cell = MapViewModel.CellPx;

            foreach (var item in items)
            {
                double l = item.Left;
                double t = item.Top;

                if (item.HasCollision && item.CollisionColor != null && item.CollisionColor.Length > 1)
                {
                    var brush = BrushCache.Get(item.CollisionColor);
                    if (brush != null)
                        dc.DrawRectangle(brush, null, new Rect(l, t, cell, cell));
                }

                if (item.IsVision)
                    dc.DrawRectangle(VisionBrush, null, new Rect(l, t, cell, cell));

                if (item.IsPlayer)
                    dc.DrawEllipse(PlayerFill, PlayerStroke,
                        new Point(l + cell - 3 - 5, t + 3 + 5), 5, 5);

                if (item.IsNpc && item.NpcSymbol != null)
                {
                    var ft = MakeText(item.NpcSymbol, NpcTypeface, NpcBrush, 13);
                    dc.DrawText(ft, new Point(l + (cell - ft.Width) / 2,
                                              t + (cell - ft.Height) / 2));
                }

                if (item.IsDebug)
                {
                    if (item.DebugTintColor != null && item.DebugTintColor.Length > 1)
                    {
                        var tint = BrushCache.Get(item.DebugTintColor);
                        if (tint != null)
                            dc.DrawRectangle(tint, null, new Rect(l, t, cell, cell));
                    }
                    if (item.DebugText != null)
                    {
                        var ft = MakeText(item.DebugText, DebugTypeface, DebugTextBrush, 8);
                        dc.DrawText(ft, new Point(l + (cell - ft.Width) / 2,
                                                  t + (cell - ft.Height) / 2));
                    }
                }
            }
        }

        private static FormattedText MakeText(string text, Typeface tf, Brush fg, double size) =>
            new FormattedText(
                text, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, tf, size, fg,
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

        private static T Freeze<T>(T obj) where T : Freezable { obj.Freeze(); return obj; }
    }

    internal static class BrushCache
    {
        private static readonly Dictionary<string, Brush> _cache = new Dictionary<string, Brush>();

        public static Brush Get(string hex)
        {
            Brush b;
            if (_cache.TryGetValue(hex, out b)) return b;
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                b = new SolidColorBrush(color);
                b.Freeze();
                _cache[hex] = b;
                return b;
            }
            catch { return null; }
        }
    }
}