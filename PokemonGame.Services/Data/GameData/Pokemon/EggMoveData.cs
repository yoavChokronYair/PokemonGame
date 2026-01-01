using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public sealed class EggMoveData
    {
        private MoveNameType eggMove;
        private int pokemonID;

        public MoveNameType EggMove { get => eggMove; set => eggMove = value; }
        public int PokemonID { get => pokemonID; set => pokemonID = value; }
    }
}
