using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using PokemonGame.ViewModels.ViewModelPage;

namespace PokemonGame.View.Behaviors
{
    public static class FocusBehavior
    {
        // ── Focus on load ─────────────────────────────────────────────────
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

        // ── Register focus target on MapViewModel ─────────────────────────
        public static readonly DependencyProperty RegisterFocusTargetProperty =
            DependencyProperty.RegisterAttached(
                "RegisterFocusTarget",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnRegisterFocusTargetChanged));

        public static bool GetRegisterFocusTarget(UIElement element) =>
            (bool)element.GetValue(RegisterFocusTargetProperty);

        public static void SetRegisterFocusTarget(UIElement element, bool value) =>
            element.SetValue(RegisterFocusTargetProperty, value);

        private static void OnRegisterFocusTargetChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element)) return;
            if (!(bool)e.NewValue) return;

            element.DataContextChanged += (sender, args) =>
            {
                if (args.NewValue is IFocusTarget vm)
                    vm.RegisterFocusCallback(() => element.Focus());
            };

            element.Loaded += (sender, args) =>
            {
                if (element.DataContext is MapViewModel vm)
                    vm.RegisterFocusCallback(() => element.Focus());
            };
        }
    }
    public static class ScrollViewerOffsetBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(ScrollViewerOffsetBehavior),
                new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static double GetVerticalOffset(DependencyObject obj) =>
            (double)obj.GetValue(VerticalOffsetProperty);

        public static void SetVerticalOffset(DependencyObject obj, double value) =>
            obj.SetValue(VerticalOffsetProperty, value);

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer sv)
                sv.ScrollToVerticalOffset((double)e.NewValue);
        }
    }   

}