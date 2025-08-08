
using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data.MapData;
using PokemonGameModel.Model.Manager;
using System.ComponentModel;

namespace PokemonGameModel.Model.Map
{
    public class GameMap
    {
        private readonly Dictionary<MapData, List<string?>> _data = new Dictionary<MapData, List<string?>>();
        private readonly List<(TileTypeFirstLayer,TileTypeSecondLayer)> _tiles = new List<(TileTypeFirstLayer, TileTypeSecondLayer)>();
        
        public GameMap(MapDataList def)
        {
            foreach(MapData map in def.maps)
            {
                 List<string?> l = new List<string?>() { map.UpMap,map.DownMap,map.LeftMap,map.RightMap};
                _data.Add(map, l);
                GenerateGameMapFromRegions(map);
            }
            
        }

        public void GenerateGameMapFromRegions(MapData def)
        {
           


            // Initialize all tiles
            for (int y = 0; y < def.Height; y++)
            {
                for (int x = 0; x < def.Width; x++)
                {
                    _tiles.Add((TileTypeFirstLayer.Empty, TileTypeSecondLayer.None));
                }
            }

            // Fill terrain tiles
            foreach (var region in def.Regions)
            {
                for (int y = 0; y < region.Height; y++)
                {
                    for (int x = 0; x < region.Width; x++)
                    {
                        int tileX = region.StartX + x;
                        int tileY = region.StartY + y;

                        if (tileX >= 0 && tileX < def.Width && tileY >= 0 && tileY < def.Height)
                            _tiles[tileY * tileX] = (region.Title, _tiles[tileY * tileX].Item2);
                        if(_tiles[tileY * tileX].Item1 == TileTypeFirstLayer.Trainer)
                        {
                            _tiles[tileY * tileX]= (_tiles[tileY * tileX].Item1, TileTypeSecondLayer.Interactable);
                            Direction? direction = GameDataManager.Instance.TrainerData.trainers.FirstOrDefault(m => m.Id == region.ID).FirstDirection;
                            setTrainerVision(Direction.Left, tileX,tileY,def);
                        }
                    }
                }
            }
        }
        public void setTrainerVision(Direction? direction, int startX, int startY, MapData def)
        {
            if (direction == null)
                return;

            int dx = 0, dy = 0;

            switch (direction)
            {
                case Direction.Left:
                    dx = -1;
                    break;
                case Direction.Right:
                    dx = 1;
                    break;
                case Direction.Up:
                    dy = -1;
                    break;
                case Direction.Down:
                    dy = 1;
                    break;
                default:
                    return;
            }

            for (int step = 1; step <= 8; step++)
            {
                int x = 0; int y = 0;
                if(dx != 0)
                {
                     x = startX + dx * step;
                }
                if(dy != 0)
                {
                  y = startY + dy * step;
                }
               

                if (x < 0 || y < 0 || x >= def.Width || y >= def.Height)
                    break;
                int index = startX * startY + dx;
                index = index + x + y;
                _tiles[index] = (_tiles[index].Item1, TileTypeSecondLayer.Event);
            }
        }

      

  

        private void HighlightTrainerDirectionLine()
        {

            
        }

        private void SetPlayerTileRenderInfo()
        {
        }

       


       
    }
}
