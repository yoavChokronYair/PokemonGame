using System.Windows;

namespace PokemonGame.Views.Windows
{
    public partial class InputDialog : Window
    {
        public string ResponseText { get; set; } = "";
        public string Message { get; set; } = "";
        public string TitleText { get; set; } = "";

        public InputDialog(string title, string message, string defaultValue = "")
        {
            InitializeComponent();
            TitleText = title;
            Message = message;
            ResponseText = defaultValue;
            DataContext = this;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // closes dialog and returns true
        }
    }
}
