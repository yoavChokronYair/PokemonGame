
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

    }
}

