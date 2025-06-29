using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Manager;
using PokemonGame.Model.Map;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel;
using PokemonGame.Views.Pages; 
using System.Windows;
using System.Windows.Controls;
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

            GameDataManager.Instance.LoadAllData();
            InitializeComponent();
            int count = 0;     
            foreach (var pokemon in GameDataManager.Instance.CaughtPokemonData.CaughtPokemons)
            {
                if(count < 6)
                {
                    PlayerPokemonGeneration playerPokemonGeneration = new PlayerPokemonGeneration(pokemon);
                    PlayerPokemonManager.Instance.AddPokemonToTeam(playerPokemonGeneration,count);
                    count++;
                }
            }
            // Navigate to the battle view with the encounter
            RouteEncounterHelper routeEncounterViewModel = new RouteEncounterHelper(GameDataManager.Instance.RouteData);
            this.DataContext = new MainWindowViewModel();
            //MainFrame.Navigate(new WildPokemonBattleView(encounter));
            //MainFrame.Navigate(new NewGameView());
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

