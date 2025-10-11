using PokemonGame.ViewModels.Backpack;
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

namespace PokemonGame.Views.Pages
{
    /// <summary>
    /// Interaction logic for Backpack.xaml
    /// </summary>
    public partial class Backpack : Page
    {
        public Backpack()
        {
            InitializeComponent();
            var vm = new MiniMapViewModel();
            vm.LoadTestTilesConnected(); // Load fake tiles for testing
            MiniMapControl.DataContext = vm;
        }
    }
}
