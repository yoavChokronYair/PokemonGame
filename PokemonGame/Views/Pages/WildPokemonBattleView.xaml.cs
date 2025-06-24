using PokemonGame.Model.BattleSystem.Bot;
using PokemonGame.Model.BattleSystem.Player;
using PokemonGame.Model.Data;
using PokemonGame.Model.Manager;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel;
using PokemonGame.ViewModel.ViewModelHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PokemonGame.Views.Pages
{
    /// <summary>
    /// Interaction logic for WildPokemonBattle.xaml
    /// </summary>
    public partial class WildPokemonBattleView : System.Windows.Controls.Page
    {
        private readonly WildPokemonBattleViewModel _viewModel;
        private readonly WildPokemonBot _wildPokemon;
        private readonly PlayerPokemonBot _playerPokemon;
        private readonly NavigationStore _navigationStore;


        public WildPokemonBattleView(Encounter encounter)
        {
            InitializeComponent();
            EnemyPokemonGeneration wildPokemon = new EnemyPokemonGeneration(
                encounter,
                GameDataManager.Instance.PokemonData.AllPokemons.FirstOrDefault(p => p.Name == encounter.Pokemon)
            );
            _navigationStore = new NavigationStore();
           
            var basePokemon = GameDataManager.Instance.PokemonData.AllPokemons
                .FirstOrDefault(p => p.Number == wildPokemon.PokedexID);
            PlayerPokemonGeneration playerPokemon = PlayerPokemonManager.Instance._playerPokemonTeam[0];
            _wildPokemon = new WildPokemonBot(wildPokemon,playerPokemon);
            List<PlayerPokemonGeneration> list = new List<PlayerPokemonGeneration>();
            list.Add(playerPokemon);
            _playerPokemon = new PlayerPokemonBot(list,wildPokemon);
            _viewModel = new WildPokemonBattleViewModel(_playerPokemon, _wildPokemon,_navigationStore);
            DataContext = _viewModel;
            SetPokemonImages(wildPokemon.PokedexID);
        }
       
        private void SetPokemonImages(int pokedexId)
        {
            var uri = new Uri($"pack://application:,,,/Images/GenOnePokemon/{pokedexId}.png");
            var image = new BitmapImage(uri);
            WildPokemonImage.Source = image;
            WildPokemonImageTeam.Source = _playerPokemon._ActivePokemon.Image;
           
        }

        private void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Tab ||
                e.Key == Key.Up ||
                e.Key == Key.Down ||
                e.Key == Key.Left ||
                e.Key == Key.Right)
            {
                // Prevent default behavior for Tab and arrow keys
                e.Handled = true;
                return;
            }
        }
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this); 
        }
    }
}
