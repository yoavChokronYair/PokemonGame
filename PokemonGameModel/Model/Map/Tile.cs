using PokemonGame.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGameModel.Model.Map
{
    public class Tile
    {
        public int BackgroundID;
        public int? LowerOverlayID; // drawn under player
        public int? UpperOverlayID; // drawn over player
        public TileType type;
    }
}
