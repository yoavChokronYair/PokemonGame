using System.Windows;
using System.Windows.Controls;

namespace PokemonGame.Views.Controls
{
    public enum BackgroundType
    {
        White,
        Blue,
        Red
    }
    public class BoardBackground : ContentControl
    {

        public static readonly DependencyProperty BackgroundTypeProperty =
            DependencyProperty.Register(nameof(BackgroundType), typeof(BackgroundType), typeof(BoardBackground),
                new PropertyMetadata(BackgroundType.White));

        public BackgroundType BackgroundType
        {
            get => (BackgroundType)GetValue(BackgroundTypeProperty);
            set => SetValue(BackgroundTypeProperty, value);
        }

        static BoardBackground()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BoardBackground),
                new FrameworkPropertyMetadata(typeof(BoardBackground)));
        }

    }
}
