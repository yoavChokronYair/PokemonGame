using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonGameModel.Enums;
using PokemonGameModel.Interface;
using PokemonGameModel.Model.Data;
using PokemonGameModel.Model.Helper;
using PokemonGameModel.Model.Manager;
using PokemonGameModel.Model.Map;
using PokemonGameModel.Model.PokemonCreation;
using PokemonGameModel.ViewModel;
using PokemonGameModel.ViewModel.Map;
using PokemonGameModel.Views.Pages; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
namespace PokemonGameModel
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

