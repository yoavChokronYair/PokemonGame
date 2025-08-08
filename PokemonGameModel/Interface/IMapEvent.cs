using PokemonGameModel.Model.Data.MapData;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGameModel.Interface
{
    public interface IMapEvent
    {
        void Trigger(MapData currentMap, int tileIndex);
    }

}
