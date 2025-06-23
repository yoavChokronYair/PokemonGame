using PokemonGame.ViewModel.BattleMenu;
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
using PokemonGame.ViewModel;

namespace PokemonGame.Views.Pages
{
    /// <summary>
    /// Interaction logic for BattleMenuPage.xaml
    /// </summary>
    public partial class BattleMenuPage : Page
    {
        public BattleMenuPage()
        {
            InitializeComponent();
            
        }
        
        private void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Tab ||
                e.Key == Key.Up ||
                e.Key == Key.Down ||
                e.Key == Key.Left ||
                e.Key == Key.Right)
            {
                // Prevent default behavior for Tab and arrow keys
                e.Handled = true;
                return;
            }
        }
    }
}
