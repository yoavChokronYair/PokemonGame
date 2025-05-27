using PokemonGame.Core.Scripts.Core;
using PokemonGame.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PokemonGame.Views
{

    public partial class RadarChartControl : UserControl
    {
        public RadarChartControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty HpProperty = DependencyProperty.Register(
        nameof(Hp), typeof(double), typeof(RadarChartControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Hp
        {
            get => (double)GetValue(HpProperty);
            set => SetValue(HpProperty, value);
        }

        public static readonly DependencyProperty AttackProperty = DependencyProperty.Register(
            nameof(Attack), typeof(double), typeof(RadarChartControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Attack
        {
            get => (double)GetValue(AttackProperty);
            set => SetValue(AttackProperty, value);
        }

        public static readonly DependencyProperty DefenseProperty = DependencyProperty.Register(
            nameof(Defense), typeof(double), typeof(RadarChartControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Defense
        {
            get => (double)GetValue(DefenseProperty);
            set => SetValue(DefenseProperty, value);
        }

        public static readonly DependencyProperty SpAttackProperty = DependencyProperty.Register(
            nameof(SpAttack), typeof(double), typeof(RadarChartControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double SpAttack
        {
            get => (double)GetValue(SpAttackProperty);
            set => SetValue(SpAttackProperty, value);
        }

        public static readonly DependencyProperty SpDefenseProperty = DependencyProperty.Register(
            nameof(SpDefense), typeof(double), typeof(RadarChartControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double SpDefense
        {
            get => (double)GetValue(SpDefenseProperty);
            set => SetValue(SpDefenseProperty, value);
        }

        public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register(
            nameof(Speed), typeof(double), typeof(RadarChartControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Speed
        {
            get => (double)GetValue(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }

        // repeat for Attack, Defense, etc.

        protected override void OnRender(DrawingContext dc)
        {
            double radius = Math.Min(ActualWidth, ActualHeight) / 2 - 10;
            Point center = new Point(ActualWidth / 2, ActualHeight / 2);

            string[] statNames = { "HP", "Atk", "SpAtk", "SpDef", "Def", "Speed" };
            double[] stats = { Hp, Attack, SpAttack, SpDefense, Defense, Speed };
            double max = 200; // Or GameConstants.MaxStat

            // Draw labels
            for (int i = 0; i < 6; i++)
            {
                double angle = (-90 + i * 60) * Math.PI / 180.0;
                Point labelPos = new Point(
                    center.X + (radius + 18) * Math.Cos(angle),
                    center.Y + (radius + 18) * Math.Sin(angle));

                var text = new FormattedText(
                    stats[i].ToString(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    12,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                // Center text at point
                labelPos.Offset(-text.Width / 2, -text.Height / 2);

                dc.DrawText(text, labelPos);
            }

            // Draw stat area
            StreamGeometry geo = new StreamGeometry();
            var ctx = geo.Open();
            for (int i = 0; i < 6; i++)
            {
                double angle = (-90 + i * 60) * Math.PI / 180.0;
                double ratio = stats[i] / max;
                Point p = new Point(
                    center.X + radius * ratio * Math.Cos(angle),
                    center.Y + radius * ratio * Math.Sin(angle));

                if (i == 0) ctx.BeginFigure(p, true, true);
                else ctx.LineTo(p, true, false);
            }
            ctx.Close();

            // Draw hexagon outline
            Pen outline = new Pen(Brushes.DodgerBlue, 2);
            dc.DrawGeometry(Brushes.Transparent, outline, CreateHex(center, radius));

            // Draw filled area
            dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(160, 255, 215, 0)), null, geo);
        }


        private StreamGeometry CreateHex(Point center, double radius)
        {
            StreamGeometry geo = new StreamGeometry();
            using (var ctx = geo.Open())
            {
                for (int i = 0; i < 6; i++)
                {
                    double angle = (-90 + i * 60) * Math.PI / 180.0;
                    Point p = new Point(
                        center.X + radius * Math.Cos(angle),
                        center.Y + radius * Math.Sin(angle));

                    if (i == 0)
                        ctx.BeginFigure(p, true, true);
                    else
                        ctx.LineTo(p, true, false);
                }
            }
            return geo;
        }
    }

}
