using PokemonGame.ViewModels;
using System;
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
using PokemonGame.ViewModels.SignUp;
using System.Windows.Shapes;

namespace PokemonGame.Views.Pages
{
    /// <summary>
    /// Interaction logic for NewGame.xaml
    /// </summary>
    public partial class SignUp : Page
    {
        //ToDo:create real login format like in the game
        public SignUp()
        {
            InitializeComponent();
            this.DataContext = new SignUpViewModel();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LogIn());
        }
    }
}
