// Design: Data Transfer Object — struct-like, properties only, no logic.
// Layer: Domain — maps one SQLite row to an easy-to-use C# object.
﻿ namespace PokemonGame.Model.Domain.Pokemon
{
    public class PokemonFormData
    {
        public int PokemonID { get; set; }
        public string FormName { get; set; }
        public byte FormID { get; set; }
    }
}