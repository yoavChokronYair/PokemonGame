
using GalaSoft.MvvmLight.Views;
using PokemonGame.ViewModels;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.Views.Pages;
using PokemonGame.Views.Pages.OnlineBattlePages;
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
            NavigationStore navigationStore = new NavigationStore();
            this.DataContext = new MainWindowViewModel(navigationStore);
            
        }
    }
}

