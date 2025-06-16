using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonGame.Views.PokemonBattle
{
    /// <summary>
    /// Interaction logic for HPBar.xaml
    /// </summary>
    public partial class HPBar : UserControl
    {
        public HPBar()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdateHealth(HealthPercent); // Initialize
        }
        public double BarWidth
        {
            get { return (double)GetValue(BarWidthProperty); }
            set { SetValue(BarWidthProperty, value); }
        }

        public static readonly DependencyProperty BarWidthProperty =
            DependencyProperty.Register("BarWidth", typeof(double), typeof(HPBar), new PropertyMetadata(173.0));

        public double BarHeight
        {
            get { return (double)GetValue(BarHeightProperty); }
            set { SetValue(BarHeightProperty, value); }
        }

        public static readonly DependencyProperty BarHeightProperty =
            DependencyProperty.Register("BarHeight", typeof(double), typeof(HPBar), new PropertyMetadata(15.0));

        public double HealthPercent
        {
            get => (double)GetValue(HealthPercentProperty);
            set => SetValue(HealthPercentProperty, value);
        }

        public static readonly DependencyProperty HealthPercentProperty =
            DependencyProperty.Register(nameof(HealthPercent), typeof(double), typeof(HPBar),
                new PropertyMetadata(1.0, OnHealthChanged));

        private static void OnHealthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HPBar bar)
                bar.UpdateHealth((double)e.NewValue);
        }

        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(HPBar),
                new PropertyMetadata(TimeSpan.FromSeconds(0.3)));

        private void UpdateHealth(double hp)
        {
            FillBar.Width = BarWidth;
            FillBar.Height = BarHeight;

            hp = Math.Max(0, Math.Min(1, hp));
            double actualMaxWidth = BackgroundBar.ActualWidth;
            if (actualMaxWidth <= 0) actualMaxWidth = 173; // fallback
            double targetWidth = actualMaxWidth * hp;

            var widthAnim = new DoubleAnimation
            {
                To = targetWidth,
                Duration = AnimationDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            FillBar.BeginAnimation(WidthProperty, widthAnim);

            if (hp > 0.5)
                SetGradient("#5ad384", "#5cd687", "#69ec9d", "#73fbad", "#9efbc6");
            else if (hp > 0.2)
                SetGradient("#ffe082", "#ffca28", "#ffb300", "#ffa000", "#ff8f00");
            else
                SetGradient("#ef9a9a", "#f44336", "#e53935", "#d32f2f", "#c62828");
        }

        private void SetGradient(string c1, string c2, string c3, string c4, string c5)
        {
            FillGradient.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString(c1);
            FillGradient.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString(c2);
            FillGradient.GradientStops[2].Color = (Color)ColorConverter.ConvertFromString(c3);
            FillGradient.GradientStops[3].Color = (Color)ColorConverter.ConvertFromString(c4);
            FillGradient.GradientStops[4].Color = (Color)ColorConverter.ConvertFromString(c5);
        }
    }
}
