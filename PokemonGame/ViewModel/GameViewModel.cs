using PokemonGame.Enums;
using PokemonGame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PokemonGame.ViewModel
{
    public class GameViewModel
    {
        public GameMap Map { get; }

        public Dictionary<int, ImageSource> tileImages = new Dictionary<int, ImageSource>
        {
            { 0, new BitmapImage(new Uri("pack://application:,,,/images/TallGrass.png")) },
            { 1, new BitmapImage(new Uri("pack://application:,,,/images/road.png"))},
            { 2, new BitmapImage(new Uri("pack://application:,,,/images/TallGrass.png")) },
            { 3, new BitmapImage(new Uri("pack://application:,,,/images/TallGrass.png")) },
            { 4, new BitmapImage(new Uri("pack://application:,,,/images/TallGrass.png")) },
        };


        public GameViewModel(string rawmap)
        {
            string rawMap = @rawmap;

            Map = new GameMap(rawMap);
        }
    }
}
   