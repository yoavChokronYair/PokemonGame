namespace PokemonGame.Services.Data.GameData.Move
{
    public sealed class MoveData
    {
        public int MoveID { get; set; }         // DB column: MoveID (primary key)
        public string MoveName { get; set; }

        // DB stores these as TEXT
        public string Type { get; set; }
        public string Category { get; set; }

        public sbyte Priority { get; set; }
        public byte PPTier { get; set; }        // DB column: PPTier (not pPTier)
        public byte MovePower { get; set; }
        public byte MoveAccuracy { get; set; }

        // DB stores these as TEXT
        public string Effect { get; set; }

        public int EffectParam { get; set; }

        // DB stores these as TEXT
        public string Targets { get; set; }
        public string Flags { get; set; }
    }
}