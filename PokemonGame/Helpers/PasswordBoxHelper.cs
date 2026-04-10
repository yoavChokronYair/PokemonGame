using System.Windows;
using System.Windows.Controls;

namespace PokemonGame.Helpers
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.RegisterAttached(
                "Binding",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnBindingChanged));

        private static bool _updating = false;

        public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);
        public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);

        public static bool GetBinding(DependencyObject d) => (bool)d.GetValue(BindingProperty);
        public static void SetBinding(DependencyObject d, bool value) => d.SetValue(BindingProperty, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PasswordBox box)) return;
            if (_updating) return;

            box.PasswordChanged -= PasswordChanged;
            box.Password = (string)e.NewValue;
            box.PasswordChanged += PasswordChanged;
        }

        private static void OnBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PasswordBox box)) return;

            if ((bool)e.OldValue) box.PasswordChanged -= PasswordChanged;
            if ((bool)e.NewValue) box.PasswordChanged += PasswordChanged;
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is PasswordBox box)) return;
            _updating = true;
            SetBoundPassword(box, box.Password);
            _updating = false;
        }
    }
}