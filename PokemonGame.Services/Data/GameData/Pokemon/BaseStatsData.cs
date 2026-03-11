namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public sealed class BaseStatsData
    {
        public int PokemonID { get; set; }
        public byte HP { get; set; }
        public byte Attack { get; set; }
        public byte Defense { get; set; }       // DB column: Defense (not Defence)
        public byte SpAttack { get; set; }
        public byte SpDefense { get; set; }     // DB column: SpDefense (not SpDefence)
        public byte Speed { get; set; }

        // DB stores these as TEXT
        public string Type1 { get; set; }
        public string Type2 { get; set; }

        public byte CatchRate { get; set; }     // DB column: CatchRate (not CathRate)
        public byte EggCycles { get; set; }
        public byte BaseFriendship { get; set; }

        // DB stores these as TEXT
        public string GenderRatio { get; set; }
        public string GrowthRate { get; set; }

        public ushort BaseEXPYield { get; set; } // DB column: BaseEXPYield (not BaseExpYield)

        // DB stores these as TEXT
        public string EggGroup1 { get; set; }
        public string EggGroup2 { get; set; }

        // DB stores ability references as IDs — resolved separately if needed
        public int? Ability1ID { get; set; }
        public int? Ability2ID { get; set; }
        public int? AbilityHID { get; set; }

        public float FleeRate { get; set; }
        public float Weight { get; set; }
    }
}