namespace PokemonGame.Services.Data.GameData.MapData
{
    public class WorldData
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public class WorldRegion
        {
            public int TileType { get; set; }// to enum later 
            public int ID { get; set; }
            public int StartX { get; set; }
            public int StartY { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }

}
