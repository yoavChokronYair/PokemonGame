using PokemonGame.Model.Data;
using PokemonGame.Model.Manager;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PokemonGame.Views
{
    /// <summary>
    /// Interaction logic for WildPokemonBattle.xaml
    /// </summary>
    public partial class WildPokemonBattle : Page
    {
        PokemonStatsViewModel pokemonStatsViewModel;
        public WildPokemonBattle(Encounter encounter)
        {
            InitializeComponent();
            WildPokemonGenartion wildPokemon = new WildPokemonGenartion(encounter, GameDataManager.Instance.PokemonData.AllPokemons.FirstOrDefault(p => p.Name == encounter.Pokemon));
            WildPokemonImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/GenOnePokemon/{wildPokemon.ID}.png"));
            WildPokemonImageTeam.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/GenOnePokemon/{wildPokemon.ID}.png"));
            var pokemon = GameDataManager.Instance.PokemonData.AllPokemons
                .FirstOrDefault(p => p.Number == wildPokemon.ID);

            pokemonStatsViewModel = new PokemonStatsViewModel(pokemon, pokemon,wildPokemon,wildPokemon); 
            this.DataContext = pokemonStatsViewModel;

        }
    }
}
