using PokemonGame.Model.Manager;
using PokemonGame.ViewModel;
using System.Linq;
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
            //PokemonStatsViewModel = new PokemonStatsViewModel(GameDataManager.Instance.PokemonData.AllPokemons[count], GameDataManager.Instance.PokemonData.AllPokemons[count]);
            this.DataContext = PokemonStatsViewModel;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            count++;
            //this.PokemonStatsViewModel = new PokemonStatsViewModel(GameDataManager.Instance.PokemonData.AllPokemons[count], GameDataManager.Instance.PokemonData.AllPokemons[count]);
            this.DataContext = PokemonStatsViewModel;
            if (count >= GameDataManager.Instance.PokemonData.AllPokemons.Count-1)
            {
                count = 0; // Reset to the first starter if we exceed the list
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
