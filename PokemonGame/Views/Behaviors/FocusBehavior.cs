using System.Windows;

namespace PokemonGame.View.Behaviors
{
    /// <summary>
    /// Attached property that calls Focus() on any UIElement when it is loaded.
    /// Usage in XAML:  behaviors:FocusBehavior.FocusOnLoad="True"
    /// </summary>
    public static class FocusBehavior
    {
        public static readonly DependencyProperty FocusOnLoadProperty =
            DependencyProperty.RegisterAttached(
                "FocusOnLoad",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnFocusOnLoadChanged));

        public static bool GetFocusOnLoad(UIElement element) =>
            (bool)element.GetValue(FocusOnLoadProperty);

        public static void SetFocusOnLoad(UIElement element, bool value) =>
            element.SetValue(FocusOnLoadProperty, value);

        private static void OnFocusOnLoadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element)) return;

            if ((bool)e.NewValue)
                element.Loaded += (sender, args) => element.Focus();
            else
                element.Loaded -= (sender, args) => element.Focus();
        }
    }
}