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

namespace PokemonGame.Views.Pages.OnlineBattlePages
{
    /// <summary>
    /// Interaction logic for SideMenu.xaml
    /// </summary>
    public partial class SideMenu : Page
    {
        private bool _menuOpen = true;
        public SideMenu()
        {
            InitializeComponent();
        }
        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            if (_menuOpen)
            {
                // Collapse
                MenuColumn.Width = new GridLength(75);

                HistroyBtn.Content = "";
                FriendsBtn.Content = "";
                TeamBtn.Content = "";
                ProfileBtn.Content = "";
                exitBtn.Content = "";
            }
            else
            {
                // Expand
                MenuColumn.Width = new GridLength(200);

                HistroyBtn.Content = "History";
                FriendsBtn.Content = "Friends";
                TeamBtn.Content = "Team";
                ProfileBtn.Content = "Profile";
                exitBtn.Content = "Exit";
            }

            _menuOpen = !_menuOpen;
        }

        private void HistroyBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void FriendsBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void TeamBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
