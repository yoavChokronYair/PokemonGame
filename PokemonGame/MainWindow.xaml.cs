using PokemonGame.Model.Data;
using PokemonGame.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using PokemonGame.Enums;
using PokemonGame.Views;
using System;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Manager;
namespace PokemonGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GameViewModel _viewModel;


        public MainWindow()
        {

            GameDataManager.Instance.LoadAllData();
            InitializeComponent();
            this.DataContext = new GameViewModel(GameDataManager.Instance.MapData.maps[0]);
            _viewModel = new GameViewModel(GameDataManager.Instance.MapData.maps[0]);
            RouteEncounterHelper routeEncounterViewModel = new RouteEncounterHelper(GameDataManager.Instance.RouteData);
            Encounter encounter = routeEncounterViewModel.GetRandomEncounter("Route 1", "grass");
            //MainFrame.Navigate(new WildPokemonBattle(encounter));
        }

        private async Task ScrollDownAsync()
        {
          
            const int tileSize = 40;
            const int step = 1;            // pixels per frame (higher = faster)
            const int delay = 1;           // milliseconds between frames (lower = faster)

            double scrollOffsetY = 0;

            for (int i = 0; i < tileSize; i += step)
            {
                scrollOffsetY += step;
                MapTransform.Y = scrollOffsetY;
                await Task.Delay(delay);
            }

            // Shift tile data and reset transform
            if (DataContext is GameViewModel viewModel)
            {
                viewModel.ShiftTilesDown();
            }

            MapTransform.Y = 0;
        }
        private async Task ScrollLeftAsync()
        {
            const int tileSize = 32;
            double scrollOffsetX = 0;

            for (int i = 0; i < tileSize; i++)
            {
                scrollOffsetX -= 1;
                MapTransform.X = scrollOffsetX;
                await Task.Delay(1);
            }

            if (DataContext is GameViewModel viewModel)
                viewModel.ShiftTilesLeft();

            MapTransform.X = 0;
        }

        private async Task ScrollRightAsync()
        {
            const int tileSize = 32;
            double scrollOffsetX = 0;

            for (int i = 0; i < tileSize; i++)
            {
                scrollOffsetX += 1;
                MapTransform.X = scrollOffsetX;
                await Task.Delay(1);
            }

            if (DataContext is GameViewModel viewModel)
                viewModel.ShiftTilesRight();

            MapTransform.X = 0;
        }
        private async Task ScrollUpAsync()
        {
            const int tileSize = 32;
            double scrollOffsetY = 0;

            for (int i = 0; i < tileSize; i++)
            {
                scrollOffsetY -= 1;
                MapTransform.Y = scrollOffsetY;
                await Task.Delay(1);
            }

            if (DataContext is GameViewModel viewModel)
                viewModel.ShiftTilesUp();

            MapTransform.Y = 0;
        }
        private async void UpButton_Click(object sender, RoutedEventArgs e)
        {
            await ScrollUpAsync();
        }

        private async void DownButton_Click(object sender, RoutedEventArgs e)
        {
            await ScrollDownAsync();
        }

        private async void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            await ScrollLeftAsync();
        }

        private async void RightButton_Click(object sender, RoutedEventArgs e)
        {
            await ScrollRightAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            var button = sender as Button;
            var tile = button?.DataContext;


            var source = tile; // or tileViewModel.TileType
            foreach (var uitiles in _viewModel.TileImages)
            {
                if (uitiles.Value.ToString() == tile.ToString())
                {
                    if (uitiles.Key == TileType.Grass)
                    {
                        RouteEncounterHelper routeEncounterViewModel = new RouteEncounterHelper(GameDataManager.Instance.RouteData);
                        Encounter encounter = routeEncounterViewModel.GetRandomEncounter("Route 1", "grass");
                        if(random.Next(0,255) < 50) // 20% chance of encounter
                        {
                            // Navigate to the battle view with the encounter
                            MainFrame.Navigate(new WildPokemonBattle(encounter));
                        }
                    }
                }
            }
        }
    }
}

