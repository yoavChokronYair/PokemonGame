// Design: Data Transfer Object — struct-like, properties only, no logic.
// Layer: Domain — maps one SQLite row to an easy-to-use C# object.
﻿using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Domain.Pokemon
{
    public class BaseStatsData
    {
        public int PokemonID { get; set; }
        public byte HP { get; set; }
        public byte Attack { get; set; }
        public byte Defence { get; set; }
        public byte SpAttack { get; set; }
        public byte SpDefence { get; set; }
        public byte Speed { get; set; }
        public PokemonType Type1 { get; set; }
        public PokemonType Type2 { get; set; }
        public byte CathRate { get; set; }
        public byte EggCycles { get; set; }
        public byte BaseFriendship { get; set; }
        public GenderRatioType GenderRatio { get; set; }
        public GrowthRateType GrowthRate { get; set; }
        public ushort BaseExpYield { get; set; }
        public EggGroupType EggGroup1 { get; set; }
        public EggGroupType EggGroup2 { get; set; }
        public AbilityData? Ability1 { get; set; }
        public AbilityData? Ability2 { get; set; }
        public AbilityData? AbilityH { get; set; }
        public byte FleeRate { get; set; }
        public float Weight { get; set; }
    }
}