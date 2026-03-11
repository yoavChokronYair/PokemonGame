// Design: Data Transfer Object — struct-like, properties only, no logic.
// Layer: Domain — maps one SQLite row to an easy-to-use C# object.
﻿using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Domain.Pokemon
{
    public class EvolutionData
    {
        public int PokemonID { get; set; }
        public ushort LevelRequired { get; set; }
        public string BabySpeciesName { get; set; }
        public EvoMethodType Method { get; set; }
        public string SpeciesName { get; set; }
        public PokemonFormData Form { get; set; }
    }
}
