
using PokemonGame.ViewModels;
using PokemonGame.Views.Pages;
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
            MainFrame.Navigate(new Backpack());
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

