// Design: Data Transfer Object — struct-like, properties only, no logic.
// Layer: Domain — maps one SQLite row to an easy-to-use C# object.
﻿using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Domain.Pokemon
{
    public class AbilityData
    {
        public int AbilityID { get; set; }
        public string AbilityName { get; set; }
        public string AbilityDescription { get; set; }
        public AbilityCategoryType Category { get; set; }
    }
}