using PokemonGame.ViewModels.ViewModelHelper.Service;
using PokemonGame.ViewModels.ViewModelPage.SignUp;
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
using System.Windows.Shapes;

namespace PokemonGame.Views.Pages.SignIn
{
    /// <summary>
    /// Interaction logic for GameModeChooser.xaml
    /// </summary>
    public partial class GameModeChooser : Page
    {
        public GameModeChooser(string username)
        {
            InitializeComponent();

            DataContext = new GameModeChooserViewModel(username,new DialogService());
        }
    }
}
