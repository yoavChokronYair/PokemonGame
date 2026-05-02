using System.Windows;
using System.Windows.Controls;

namespace PokemonGame.Views.Controls
{
    public class BindablePasswordBox : Control
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
                nameof(Password),
                typeof(string),
                typeof(BindablePasswordBox),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnPasswordPropertyChanged));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        private bool _isUpdating;
        private PasswordBox _passwordBox;

        static BindablePasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BindablePasswordBox),
                new FrameworkPropertyMetadata(typeof(BindablePasswordBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_passwordBox != null)
                _passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

            _passwordBox = GetTemplateChild("PART_PasswordBox") as PasswordBox;

            if (_passwordBox != null)
            {
                _passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                _passwordBox.Password = Password ?? string.Empty;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _isUpdating = true;
            Password = _passwordBox.Password;
            _isUpdating = false;
        }

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BindablePasswordBox)d;
            if (!control._isUpdating && control._passwordBox != null)
            {
                control._passwordBox.Password = e.NewValue as string ?? string.Empty;
            }
        }
        public static readonly DependencyProperty IsPasswordVisibleProperty =
            DependencyProperty.Register(
                nameof(IsPasswordVisible),
                typeof(bool),
                typeof(BindablePasswordBox),
                new PropertyMetadata(false));

        public bool IsPasswordVisible
        {
            get => (bool)GetValue(IsPasswordVisibleProperty);
            set => SetValue(IsPasswordVisibleProperty, value);
        }
    }
}