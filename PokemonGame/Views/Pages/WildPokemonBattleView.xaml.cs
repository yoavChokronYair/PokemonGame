using PokemonGame.Model.BattleSystem.Bot;
using PokemonGame.Model.BattleSystem.Player;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Manager;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace PokemonGame.Views.Pages
{
    /// <summary>
    /// Interaction logic for WildPokemonBattle.xaml
    /// </summary>
    public partial class WildPokemonBattleView : Page
    {
        private readonly WildPokemonBattleViewModel _viewModel;
        private readonly WildPokemonBot _wildPokemon;
        private readonly PlayerPokemonBot _playerPokemon;
        public WildPokemonBattleView(Encounter encounter)
        {
            InitializeComponent();
            EnemyPokemonGeneration wildPokemon = new EnemyPokemonGeneration(
                encounter,
                GameDataManager.Instance.PokemonData.AllPokemons.FirstOrDefault(p => p.Name == encounter.Pokemon)
            );
            var basePokemon = GameDataManager.Instance.PokemonData.AllPokemons
                .FirstOrDefault(p => p.Number == wildPokemon.PokedexID);
            PlayerPokemonGeneration playerPokemon = PlayerPokemonManager.Instance._playerPokemonTeam[0];
            _wildPokemon = new WildPokemonBot(wildPokemon,playerPokemon);
            List<PlayerPokemonGeneration> list = new List<PlayerPokemonGeneration>();
            list.Add(playerPokemon);
            _playerPokemon = new PlayerPokemonBot(list,wildPokemon);
            _viewModel = new WildPokemonBattleViewModel(_playerPokemon, _wildPokemon);
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
    }
}
