// Design: Data Transfer Object — struct-like, properties only, no logic.
// Layer: Domain — maps one SQLite row to an easy-to-use C# object.
﻿
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Domain.Pokemon
{
    public sealed class LevelUpMoveData
    {
        public MoveNameType MoveName { get; set; }
        public byte Level {  get; set; }
        public int PokemonID { get; set; }
    }
}
