    using PokemonGame.Core.Scripts.Core;
using PokemonGame.Model;
using PokemonGame.Model.Data;
    using PokemonGame.ViewModel;
    using System;
    using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;


    namespace PokemonGame.Views
    {
        /// <summary>
        /// Interaction logic for PokemonStatsView.xaml
        /// </summary>
        public partial class PokemonStatsView : UserControl
        {
        
            public PokemonStatsViewModel PokemonStatsViewModel;
            public int count;
            public PokemonStatsView()
            {
                InitializeComponent();
                PokemonStatsViewModel = new PokemonStatsViewModel(GameDataManager.Instance.PokemonData.AllPokemons[count]);
                this.DataContext = PokemonStatsViewModel;
            }
            private void Button_Click(object sender, RoutedEventArgs e)
            {
                count++;
                this.PokemonStatsViewModel = new PokemonStatsViewModel(GameDataManager.Instance.PokemonData.AllPokemons[count]);
                this.DataContext = PokemonStatsViewModel;
                if (count >= GameDataManager.Instance.PokemonData.AllPokemons.Count-1)
                {
                    count = 0; // Reset to the first starter if we exceed the list
                }
            }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            RouteEncounterChooser routeEncounterViewModel = new RouteEncounterChooser(GameDataManager.Instance.RouteData);
            Encounter encounter = routeEncounterViewModel.GetRandomEncounter("Route 1", "grass");
        }
    }
}
