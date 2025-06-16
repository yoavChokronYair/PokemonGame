using PokemonGame.Model.Data;
using PokemonGame.Model.Manager;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static PokemonGame.Model.Helper.BattleCaculater;

namespace PokemonGame.Views.Pages
{
    /// <summary>
    /// Interaction logic for WildPokemonBattle.xaml
    /// </summary>
    public partial class WildPokemonBattle : Page
    {
        // ----------------------------
        // Fields
        // ----------------------------
        private readonly WildPokemonBattleViewModel _viewModel;
        private readonly EnemyPokemonGeneration _wildPokemon;
        private readonly PlayerPokemonGeneration _playerPokemon;

        // ----------------------------
        // Constructor
        // ----------------------------
        public WildPokemonBattle(Encounter encounter)
        {
            InitializeComponent();

            // Generate wild Pokémon data
            _wildPokemon = new EnemyPokemonGeneration(
                encounter,
                GameDataManager.Instance.PokemonData.AllPokemons.FirstOrDefault(p => p.Name == encounter.Pokemon)
            );

            // Load wild Pokémon images

            // Get base data for wild Pokémon
            var basePokemon = GameDataManager.Instance.PokemonData.AllPokemons
                .FirstOrDefault(p => p.Number == _wildPokemon.PokedexID);

            // Generate player's Pokémon with half HP for battle
            _playerPokemon = PlayerPokemonManager.Instance._playerPokemonTeam[0];

            // Initialize ViewModel and bind to DataContext
            _viewModel = new WildPokemonBattleViewModel(_playerPokemon, _wildPokemon);
            DataContext = _viewModel;
            SetPokemonImages(_wildPokemon.PokedexID);

            // Subscribe to move click event
            BattleMenuControl.MoveClicked += OnMoveClicked;
        }

        // ----------------------------
        // UI Helpers
        // ----------------------------
        private void SetPokemonImages(int pokedexId)
        {
            var uri = new Uri($"pack://application:,,,/Images/GenOnePokemon/{pokedexId}.png");
            var image = new BitmapImage(uri);
            WildPokemonImage.Source = image;
            WildPokemonImageTeam.Source = _playerPokemon.Image;
        }

        // ----------------------------
        // Battle Logic
        // ----------------------------
        private void OnMoveClicked(object sender, string moveName)
        {
            int currentIndex = 0;
            int index = -1;
            MoveData move = null;

            foreach (var entry in _viewModel.Moves)
            {
                if (entry.ename == moveName)
                {
                    move = entry;
                    index = currentIndex;
                    break;
                }
                currentIndex++;
            }
            // Consume one PP   
            _viewModel.Moves[index].PP--;
            _viewModel.CurrentPP = _viewModel.Moves[index].PP;
            // Calculate damage
            var result = BattleCalculator.ExecuteMove(_wildPokemon, _playerPokemon, move);
            var newHp = _viewModel.RivalCurrentHp - result.Damage;
            _viewModel.MaxPP = _viewModel.Team.Moves[move];
            _viewModel.Type = move.Type.ToString();
            // Update wild Pokémon HP
            _viewModel.RivalCurrentHp = Math.Max(0, newHp);

            // Optional: refresh UI if needed
        }
    }
}
