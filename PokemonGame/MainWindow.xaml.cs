using PokemonGame.Core.Scripts.Core;
using PokemonGame.ViewModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
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
            _viewModel = new GameViewModel((GameDataManager.Instance.MapData.maps[0].tiles));
            DrawMap();
        }

        private void DrawMap()
        {
            int tileWidth = 50;
            int tileHeight = 50;

            var tiles = _viewModel.Map.Tiles;

            for (int y = 0; y < tiles.GetLength(0); y++)
            {
                for (int x = 0; x < tiles.GetLength(1); x++)
                {
                   
                    Image title = new Image
                    {
                        Source = _viewModel.tileImages[(int)tiles[y, x].Type],
                        Width = tileWidth,
                        Height = tileHeight,
                    };

                    Canvas.SetLeft(title, x * tileWidth);
                    Canvas.SetTop(title, y * tileHeight);
                    GameCanvas.Children.Add(title);
                }
            }
        }
        
    }
}

