using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonGame.Enums;
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

            Encounter encounter = routeEncounterViewModel.GetRandomEncounter("Route 1", "grass");
            //MainFrame.Navigate(new WildPokemonBattleView(encounter));
            //MainFrame.Navigate(new NewGameView());
            GameMap gameMap = GameMap.GenerateGameMapFromRegions(GameDataManager.Instance.MapData.maps[0]);

            LoadMap(gameMap, GameDataManager.Instance.MapData.maps[0]);

        }

        private void LoadMap(GameMap gameMap,MapData map)
        {
          

            // Set the rows and columns
            MapGrid.Rows = map.Height;
            MapGrid.Columns = map.Width;
            MapGrid.Children.Clear();

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Rectangle tileRect = new Rectangle
                    {
                        Width = 30,
                        Height = 30,
                        Stroke = Brushes.Black,
                        Fill = GetBrushForTile(gameMap.Tiles[y][x])
                    };
                    MapGrid.Children.Add(tileRect);
                }
            }
        }

        private Brush GetBrushForTile(TileType type)
        {
            switch (type)
            {
                case TileType.Path:
                    return Brushes.Gray;
                case TileType.Grass:
                    return Brushes.Green;
                case TileType.Water:
                    return Brushes.Blue;
                case TileType.Empty:
                    return Brushes.White;
                default:
                    return Brushes.Red; // Unknown tile type
            }

        }




        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
           
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

