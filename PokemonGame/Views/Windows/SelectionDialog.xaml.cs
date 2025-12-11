using System.Collections.Generic;
using System.Windows;

namespace PokemonGame.Views.Windows
{
    /// <summary>
    /// Interaction logic for SelectionDialog.xaml
    /// </summary>
    public partial class SelectionDialog : Window
    {
        public string SelectedOption { get; private set; }

        public SelectionDialog(string title, string message, IEnumerable<string> options)
        {
            InitializeComponent();

            Title = title;
            MessageTextBlock.Text = message;
            OptionsListBox.ItemsSource = options;
            if (OptionsListBox.Items.Count > 0)
                OptionsListBox.SelectedIndex = 0; // select first by default
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (OptionsListBox.SelectedItem != null)
                SelectedOption = OptionsListBox.SelectedItem.ToString();

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
