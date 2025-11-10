using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.Pokemon
{
    public sealed class PokemonBaseStatsdata
    {
        private int pokemonID;
        private byte hP;
        private byte attack;
        private byte defence;
        private byte spAttack;
        private byte spDefence;
        private byte speed;
        private PokemonType type1;
        private PokemonType type2;
        private byte cathRate;
        private byte eggCycles;
        private byte baseFriendship;
        private GenderRatioType genderRatio;
        private GrowthRateType growthRate;
        private ushort baseExpYield;
        private EggGroupType eggGroup1; 
        private EggGroupType eggGroup2;
        private AbilitysData? ability1;
        private AbilitysData? ability2;
        private AbilitysData? abilityH;
        private byte fleeRate;
        private float weight;

        public int PokemonID { get => pokemonID; set => pokemonID = value; }
        public byte HP { get => hP; set => hP = value; }
        public byte Attack { get => attack; set => attack = value; }
        public byte Defence { get => defence; set => defence = value; }
        public byte SpAttack { get => spAttack; set => spAttack = value; }
        public byte SpDefence { get => spDefence; set => spDefence = value; }
        public byte Speed { get => speed; set => speed = value; }
        public PokemonType Type1 { get => type1; set => type1 = value; }
        public PokemonType Type2 { get => type2; set => type2 = value; }
        public byte CathRate { get => cathRate; set => cathRate = value; }
        public byte EggCycles { get => eggCycles; set => eggCycles = value; }
        public byte BaseFriendship { get => baseFriendship; set => baseFriendship = value; }
        public GenderRatioType GenderRatio { get => genderRatio; set => genderRatio = value; }
        public GrowthRateType GrowthRate { get => growthRate; set => growthRate = value; }
        public ushort BaseExpYield { get => baseExpYield; set => baseExpYield = value; }
        public EggGroupType EggGroup1 { get => eggGroup1; set => eggGroup1 = value; }
        public EggGroupType EggGroup2 { get => eggGroup2; set => eggGroup2 = value; }
        public AbilitysData? Ability1 { get => ability1; set => ability1 = value; }
        public AbilitysData? Ability2 { get => ability2; set => ability2 = value; }
        public AbilitysData? AbilityH { get => abilityH; set => abilityH = value; }
        public byte FleeRate { get => fleeRate; set => fleeRate = value; }
        public float Weight { get => weight; set => weight = value; }
    }
}