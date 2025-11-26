using System.Windows;
using System.Windows.Controls;

namespace PokemonGame.Views.Controls
{
    // Moved enum OUTSIDE the class to avoid naming conflicts
    public enum BackgroundType
    {
        White,
        Blue,
        Red
    }

    public class BoardBackground : ContentControl
    {
        // Define the dependency property with the correct enum type
        public static readonly DependencyProperty BackgroundTypeProperty =
            DependencyProperty.Register(
                nameof(BackgroundType),
                typeof(BackgroundType),  // This now correctly references the enum
                typeof(BoardBackground),
                new PropertyMetadata(BackgroundType.White));

        // Property accessor
        public BackgroundType BackgroundType
        {
            get => (BackgroundType)GetValue(BackgroundTypeProperty);
            set => SetValue(BackgroundTypeProperty, value);
        }

        // Static constructor to set the default style
        static BoardBackground()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BoardBackground),
                new FrameworkPropertyMetadata(typeof(BoardBackground)));
        }
    }
}