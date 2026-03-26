
using System.Windows;
using System.Windows.Controls;
using PokemonGame.ViewModels.ViewModelPage.SignUp;


namespace PokemonGame.Views.Pages.SignIn
{
    /// <summary>
    /// Interaction logic for LogIn.xaml
    /// </summary>
    public partial class LogIn : Page
    {
        public LogIn()
        {
            InitializeComponent();            
        }
        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LogInViewModel vm)
                vm.Password = PasswordInput.Password;
        }
    }
}
