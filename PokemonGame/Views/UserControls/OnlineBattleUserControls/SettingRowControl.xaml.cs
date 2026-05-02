using System;
using System.Collections;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonGame.Views.UserControls.OnlineBattleUserControls
{
    /// <summary>
    /// Interaction logic for SettingRowControl.xaml
    /// </summary>
    public partial class SettingRowControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
         DependencyProperty.Register("Header", typeof(string), typeof(SettingRowControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty OptionsProperty =
            DependencyProperty.Register("Options", typeof(IEnumerable), typeof(SettingRowControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedOptionProperty =
            DependencyProperty.Register("SelectedOption", typeof(object), typeof(SettingRowControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // New Property to choose the style
        public static readonly DependencyProperty UseSelectorModeProperty =
            DependencyProperty.Register("UseSelectorMode", typeof(bool), typeof(SettingRowControl), new PropertyMetadata(false));

        public string Header { get => (string)GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
        public IEnumerable Options { get => (IEnumerable)GetValue(OptionsProperty); set => SetValue(OptionsProperty, value); }
        public object SelectedOption { get => GetValue(SelectedOptionProperty); set => SetValue(SelectedOptionProperty, value); }
        public bool UseSelectorMode { get => (bool)GetValue(UseSelectorModeProperty); set => SetValue(UseSelectorModeProperty, value); }
        public SettingRowControl()
        {
            InitializeComponent();
        }
        // Logic for the arrow buttons
        private void OnPrevClick(object sender, RoutedEventArgs e) => MoveSelection(-1);
        private void OnNextClick(object sender, RoutedEventArgs e) => MoveSelection(1);

        private void MoveSelection(int direction)
        {
            if (!(Options is IEnumerable<object> list) || !list.Any()) return;

            var optionsList = list.ToList();
            int index = optionsList.IndexOf(SelectedOption);
            int newIndex = index + direction;

            if (newIndex >= 0 && newIndex < optionsList.Count)
            {
                SelectedOption = optionsList[newIndex];
            }
        }
    }
}
