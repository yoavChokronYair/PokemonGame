using PokemonGame.Enums;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Manager;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel;
using PokemonGame.Views.Pages;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            this.DataContext = new GameViewModel(GameDataManager.Instance.MapData.maps[0]);
            _viewModel = new GameViewModel(GameDataManager.Instance.MapData.maps[0]);
            RouteEncounterHelper routeEncounterViewModel = new RouteEncounterHelper(GameDataManager.Instance.RouteData);
            Encounter encounter = routeEncounterViewModel.GetRandomEncounter("Route 1", "grass");
            //MainFrame.Navigate(new BattleMenu());
        }

        private async Task ScrollDownAsync()
        {
            const int tileSize = 32;
            const int step = 24;     // pixels per frame
            const int delay = 1;    // milliseconds per frame

            double scrollOffsetY = 0;

            for (int i = 0; i < tileSize; i += step)
            {
                scrollOffsetY += step;
                MapTransform.Y = scrollOffsetY;
                await Task.Delay(delay);
            }

            if (DataContext is GameViewModel viewModel)
            {
                viewModel.ShiftTilesDown();
            }

            MapTransform.Y = 0;
        }

        private async Task ScrollUpAsync()
        {
            const int tileSize = 32;
            const int step = 24;
            const int delay = 1;

            double scrollOffsetY = 0;

            for (int i = 0; i < tileSize; i += step)
            {
                scrollOffsetY -= step;
                MapTransform.Y = scrollOffsetY;
                await Task.Delay(delay);
            }

            if (DataContext is GameViewModel viewModel)
            {
                viewModel.ShiftTilesUp();
            }

            MapTransform.Y = 0;
        }

        private async Task ScrollLeftAsync()
        {
            const int tileSize = 32;
            const int step = 24;
            const int delay = 1;

            double scrollOffsetX = 0;

            for (int i = 0; i < tileSize; i += step)
            {
                scrollOffsetX -= step;
                MapTransform.X = scrollOffsetX;
                await Task.Delay(delay);
            }

            if (DataContext is GameViewModel viewModel)
            {
                viewModel.ShiftTilesLeft();
            }

            MapTransform.X = 0;
        }

        private async Task ScrollRightAsync()
        {
            const int tileSize = 32;
            const int step = 24;
            const int delay = 1;

            double scrollOffsetX = 0;

            for (int i = 0; i < tileSize; i += step)
            {
                scrollOffsetX += step;
                MapTransform.X = scrollOffsetX;
                await Task.Delay(delay);
            }

            if (DataContext is GameViewModel viewModel)
            {
                viewModel.ShiftTilesRight();
            }

            MapTransform.X = 0;
        }

        private async void UpButton_Click(object sender, RoutedEventArgs e)
        {
            await ScrollUpAsync();
        }

        private async void DownButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataManager.Instance.SaveAllData();
        }

        private async void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            await ScrollLeftAsync();
        }

        private async void RightButton_Click(object sender, RoutedEventArgs e)
        {
            await ScrollRightAsync();
        }
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    await ScrollUpAsync();
                    break;
                case Key.Down:
                    await ScrollDownAsync();
                    break;
                case Key.Left:
                    await ScrollLeftAsync();
                    break;
                case Key.Right:
                    await ScrollRightAsync();
                    break;
            }
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
                        if(random.Next(0,255) < 50) // 20% chance of encounter
                        {
                            // Navigate to the battle view with the encounter
                            Encounter encounter = routeEncounterViewModel.GetRandomEncounter("Route 1", "grass");
                            MainFrame.Navigate(new WildPokemonBattle(encounter));
                        }
                    }
                }
            }
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

