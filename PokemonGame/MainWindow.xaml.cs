
using PokemonGame.ViewModels;
using PokemonGame.ViewModels.SignUp;
using PokemonGame.Views.Pages;
using PokemonGame.Views.Pages.OnlineBattlePages;
using PokemonGame.Views.Pages.Signin;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
namespace PokemonGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
   

    public partial class MainWindow : Window
    {
        private bool _menuOpen = true;
        public MainWindow()
        {

            InitializeComponent();
            int count = 0;     
            
            // Navigate to the battle view with the encounter
            this.DataContext = new MainWindowViewModel();
            //MainFrame.Navigate(new WildPokemonBattleView(encounter));
            //MainFrame.Navigate(new Backpack());
            
            this.DataContext = new LogInViewModel();
            MainFrame.Navigate(new BattleMenuPage());
            //  MainFrame.Navigate(new GameModeChooser("TestUser"));
        }
    


  

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
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
            MainFrame.Navigate(new HistoryBattlePage());
        }

        private void FriendsBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OnlineFriendsPage());
        }

        private void TeamBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TeamSelectPage());
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BattleMenuPage());
        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
        }
    }
}

