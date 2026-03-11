// Design: Data Transfer Object — struct-like, properties only, no logic.
// Layer: Domain — maps one SQLite row to an easy-to-use C# object.
﻿using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Domain.Pokemon
{
    public sealed class EggMoveData
    {
        public MoveNameType EggMoveType { get; set ; }
        public int PokemonID { get; set; }
    }
}
